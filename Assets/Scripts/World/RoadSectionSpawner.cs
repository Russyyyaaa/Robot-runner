using System;
using System.Collections.Generic;
using UnityEngine;

public sealed class RoadSectionSpawner : MonoBehaviour
{
    [Header("Runtime References")]
    [SerializeField] private RunSessionController _runSessionController;
    [SerializeField] private Transform _followTarget;
    [SerializeField] private Transform _spawnRoot;

    [Header("Progression")]
    [SerializeField] private SectionDifficultyProfile _difficultyProfile;
    [SerializeField] private RoadSectionPool _lowPool;
    [SerializeField] private RoadSectionPool _middlePool;
    [SerializeField] private RoadSectionPool _hardPool;
    [SerializeField] private RoadSectionPool _veryHardPool;
    [SerializeField] private RoadSectionPool _boosterPool;

    [Header("Safety")]
    [SerializeField] private GameObject _safeFallbackPrefab;
    [SerializeField] private float _sectionLength = 30f;
    [SerializeField] private float _sectionGap = 0f;
    [SerializeField] private bool _alignSectionsByFloorBounds = true;
    [SerializeField] private float _minSectionLengthMultiplier = 0.5f;
    [SerializeField] private float _maxSectionLengthMultiplier = 2.5f;
    [SerializeField] private int _sectionsAhead = 8;
    [SerializeField] private int _sectionsBehindToKeep = 2;
    [SerializeField] private int _randomSeed = 42;
    [SerializeField] private int _maxSpawnsPerFrame = 4;
    [SerializeField] private bool _prewarmOnRunStart = true;
    [SerializeField] private int _prewarmMaxSpawns = 256;

    [Header("Debug")]
    [SerializeField] private bool _enableSpawnLogs;
    [SerializeField] private bool _verboseSpawnLogs = true;
    [SerializeField] private int _logEveryNthSpawn = 1;

    private readonly Queue<SpawnedSection> _spawnedSections = new Queue<SpawnedSection>();
    private readonly Dictionary<GameObject, float> _sectionLengthCache = new Dictionary<GameObject, float>();
    private SectionProgressionService _progressionService;
    private SectionSafetyValidator _safetyValidator;
    private GameDifficultyLevel _selectedDifficulty;
    private System.Random _random;
    private float _nextSpawnZ;
    private int _spawnedCount;
    private float _lastSpawnedSectionLength;
    private float _baselineSectionLength;

    private readonly struct SpawnedSection
    {
        public SpawnedSection(GameObject sectionObject, float startZ, float length)
        {
            SectionObject = sectionObject;
            StartZ = startZ;
            Length = length;
        }

        public GameObject SectionObject { get; }
        public float StartZ { get; }
        public float Length { get; }
        public float EndZ => StartZ + Length;
    }

    private void Awake()
    {
        ResolveDependencies();
        EnsureDependencies();

        _selectedDifficulty = GameDifficultyService.GetSelectedDifficulty();
        DifficultyRuntimeModifiers difficultyModifiers = GameDifficultyService.GetRuntimeModifiers(_selectedDifficulty);
        _progressionService = new SectionProgressionService(
            _difficultyProfile,
            _lowPool.Sections,
            _middlePool.Sections,
            _hardPool.Sections,
            _veryHardPool.Sections,
            _boosterPool.Sections,
            difficultyModifiers.SectionThresholdMultiplier,
            difficultyModifiers.BoosterIntervalMultiplier);

        _safetyValidator = new SectionSafetyValidator();
        _random = new System.Random(_randomSeed);
        _baselineSectionLength = ResolveBaselineSectionLength();
        _lastSpawnedSectionLength = Mathf.Max(1f, _baselineSectionLength);

        LogInfo(
            $"Инициализация спаунера: пулы low={_lowPool.Sections.Count}, middle={_middlePool.Sections.Count}, hard={_hardPool.Sections.Count}, veryHard={_veryHardPool.Sections.Count}, booster={_boosterPool.Sections.Count}. " +
            $"Выбрана сложность={_selectedDifficulty}. Длина секции (инспектор)={_sectionLength}, авто-база длины={_baselineSectionLength:F2}, зазор между секциями={_sectionGap}, секций вперед={_sectionsAhead}, секций позади={_sectionsBehindToKeep}, лимит спауна за кадр={_maxSpawnsPerFrame}, seed={_randomSeed}");
    }

    private void OnEnable()
    {
        _runSessionController.StateChanged += HandleStateChanged;
        LogInfo("Подписка на смену состояния ран-сессии.");
    }

    private void OnDisable()
    {
        _runSessionController.StateChanged -= HandleStateChanged;
        LogInfo("Отписка от смены состояния ран-сессии.");
    }

    private void Update()
    {
        if (_runSessionController.CurrentState != GameState.Running)
        {
            return;
        }

        int spawnedThisFrame = 0;
        while (NeedMoreSections())
        {
            if (SpawnNextSection() == false)
            {
                break;
            }

            spawnedThisFrame++;
            if (spawnedThisFrame >= Mathf.Max(1, _maxSpawnsPerFrame))
            {
                break;
            }
        }

        if (spawnedThisFrame > 0 && _verboseSpawnLogs == false)
        {
            LogInfo(
                $"За кадр заспаунено {spawnedThisFrame} секц. Сейчас на сцене: {_spawnedSections.Count}. " +
                $"Осталось доспаунить до буфера: {EstimateSectionsUntilBuffered()} секц.");
        }

        DespawnBehindPlayer();
    }

    private void HandleStateChanged(GameState gameState)
    {
        LogInfo($"Состояние игры: {gameState}.");

        if (gameState == GameState.Running)
        {
            ResetSections();
            PrewarmSections();
            return;
        }

        if (gameState == GameState.Menu)
        {
            ResetSections();
        }
    }

    private bool NeedMoreSections()
    {
        float lookAheadDistance = _sectionsAhead * GetSpawnStride();
        float targetZ = _followTarget.position.z + lookAheadDistance;
        return _nextSpawnZ < targetZ;
    }

    private bool SpawnNextSection()
    {
        int score = EstimateCurrentScoreByDistance();
        SectionDifficultyTier scoreTier = _progressionService.ResolveTierByScore(score);
        SectionDifficultyTier tier = ApplyDifficultyTierFloor(scoreTier);
        bool shouldSpawnBoosterSection = _progressionService.ShouldSpawnBoosterSection(_spawnedCount);
        RoadSectionDefinition candidate = shouldSpawnBoosterSection
            ? _progressionService.PickBoosterSection(_random)
            : _progressionService.PickForTier(tier, _random);

        GameObject prefabToSpawn = ResolveSpawnPrefab(candidate);
        if (prefabToSpawn == null)
        {
            LogWarning(
                $"Не удалось выбрать валидную секцию. score={score}, tier={tier}, booster={shouldSpawnBoosterSection}, candidate={(candidate.Prefab != null ? candidate.Prefab.name : "null")}, safeByDesign={candidate.IsSafeByDesign}, fallback={(_safeFallbackPrefab != null ? _safeFallbackPrefab.name : "null")}");
            return false;
        }

        float spawnedAtZ = _nextSpawnZ;
        float spawnLength = ResolveSectionLength(prefabToSpawn);
        Vector3 spawnPosition = new Vector3(0f, 0f, spawnedAtZ);
        GameObject sectionInstance = Instantiate(prefabToSpawn, spawnPosition, Quaternion.identity, _spawnRoot);
        RuntimePrefabBinder.BindHierarchy(sectionInstance);
        if (_alignSectionsByFloorBounds && TryMeasureSectionBounds(sectionInstance, out float sectionMinZ, out float sectionMaxZ, out bool usedFloorBounds))
        {
            float measuredLength = Mathf.Max(0.01f, sectionMaxZ - sectionMinZ);
            float resolvedMeasuredLength = SanitizeMeasuredSectionLength(measuredLength, prefabToSpawn.name);
            float correctionZ = spawnedAtZ - sectionMinZ;
            if (Mathf.Abs(correctionZ) > 0.0001f)
            {
                sectionInstance.transform.position += new Vector3(0f, 0f, correctionZ);
            }

            spawnLength = resolvedMeasuredLength;
            if (_verboseSpawnLogs)
            {
                string measureSource = usedFloorBounds ? "floor" : "full-bounds";
                LogInfo(
                    $"Выровнена секция '{prefabToSpawn.name}' по {measureSource}: minZ={sectionMinZ:F2}, maxZ={sectionMaxZ:F2}, correctionZ={correctionZ:F3}, measuredLength={measuredLength:F2}, resolvedLength={resolvedMeasuredLength:F2}.");
            }
        }

        _spawnedSections.Enqueue(new SpawnedSection(sectionInstance, spawnedAtZ, spawnLength));
        _nextSpawnZ += spawnLength + Mathf.Max(0f, _sectionGap);
        _lastSpawnedSectionLength = spawnLength;
        _spawnedCount++;
        int spawnIndex = _spawnedCount;
        if (ShouldLogSpawn(spawnIndex))
        {
                LogInfo(
                    $"Спаун #{spawnIndex}: секция '{prefabToSpawn.name}' (кандидат '{(candidate.Prefab != null ? candidate.Prefab.name : "null")}'), " +
                    $"сложность={tier} (по score={scoreTier}), score={score}, бустер-секция={shouldSpawnBoosterSection}, safeByDesign={candidate.IsSafeByDesign}. " +
                    $"Позиция Z={spawnedAtZ:F2}, длина={spawnLength:F2}, зазор={Mathf.Max(0f, _sectionGap):F2}. Сейчас на сцене: {_spawnedSections.Count} секц. " +
                    $"Осталось доспаунить до буфера: {EstimateSectionsUntilBuffered()} секц.");
            }

        return true;
    }

    private GameObject ResolveSpawnPrefab(RoadSectionDefinition candidate)
    {
        GameObject candidatePrefab = candidate.Prefab;
        if (IsValidSpawnPrefab(candidatePrefab) && _safetyValidator.IsSectionSafe(candidate))
        {
            return candidatePrefab;
        }

        if (IsValidSpawnPrefab(_safeFallbackPrefab))
        {
            LogWarning(
                $"Использован fallback. candidate={(candidatePrefab != null ? candidatePrefab.name : "null")}, safeByDesign={candidate.IsSafeByDesign}, fallback={_safeFallbackPrefab.name}");
            return _safeFallbackPrefab;
        }

        LogWarning(
            $"Кандидат отклонен и fallback не задан. candidate={(candidatePrefab != null ? candidatePrefab.name : "null")}, safeByDesign={candidate.IsSafeByDesign}");
        if (IsValidSpawnPrefab(candidatePrefab))
        {
            return candidatePrefab;
        }

        return null;
    }

    private void DespawnBehindPlayer()
    {
        float keepDistance = _sectionsBehindToKeep * GetSpawnStride();
        float minZToKeep = _followTarget.position.z - keepDistance;
        int despawnedCount = 0;

        while (_spawnedSections.Count > 0)
        {
            SpawnedSection oldest = _spawnedSections.Peek();
            if (oldest.SectionObject == null)
            {
                _spawnedSections.Dequeue();
                despawnedCount++;
                continue;
            }

            if (oldest.EndZ >= minZToKeep)
            {
                break;
            }

            _spawnedSections.Dequeue();
            Destroy(oldest.SectionObject);
            despawnedCount++;
        }

        if (despawnedCount > 0)
        {
            LogInfo(
                $"Удалено позади: {despawnedCount} секц. Следование Z={_followTarget.position.z:F2}, граница сохранения Z={minZToKeep:F2}. Сейчас на сцене: {_spawnedSections.Count} секц.");
        }
    }

    private void ResetSections()
    {
        int removedCount = 0;
        foreach (SpawnedSection spawnedSection in _spawnedSections)
        {
            if (spawnedSection.SectionObject != null)
            {
                Destroy(spawnedSection.SectionObject);
                removedCount++;
            }
        }

        _spawnedSections.Clear();
        _sectionLengthCache.Clear();
        _spawnedCount = 0;
        _nextSpawnZ = _followTarget.position.z - GetSpawnStride();
        _baselineSectionLength = ResolveBaselineSectionLength();
        _lastSpawnedSectionLength = Mathf.Max(1f, _baselineSectionLength);
        _random = new System.Random(_randomSeed);
        LogInfo(
            $"Сброс секций. Удалено={removedCount}, followZ={_followTarget.position.z:F2}, nextSpawnZ={_nextSpawnZ:F2}, базовая длина секции={_lastSpawnedSectionLength:F2}, зазор={Mathf.Max(0f, _sectionGap):F2}.");
    }

    private void PrewarmSections()
    {
        if (_prewarmOnRunStart == false)
        {
            return;
        }

        int maxSpawns = Mathf.Max(1, _prewarmMaxSpawns);
        int spawnedCount = 0;
        while (NeedMoreSections() && spawnedCount < maxSpawns)
        {
            if (SpawnNextSection() == false)
            {
                break;
            }

            spawnedCount++;
        }

        LogInfo(
            $"Предзагрузка завершена. Сразу заспаунено: {spawnedCount} секц. На сцене: {_spawnedSections.Count}. Длина буфера вперед: {_sectionsAhead} секц.");
    }

    private int EstimateCurrentScoreByDistance()
    {
        return Mathf.Max(0, Mathf.FloorToInt(_followTarget.position.z));
    }

    private void EnsureDependencies()
    {
        if (_runSessionController == null)
        {
            throw new InvalidOperationException($"{nameof(RoadSectionSpawner)} requires {nameof(_runSessionController)}.");
        }

        if (_followTarget == null)
        {
            throw new InvalidOperationException($"{nameof(RoadSectionSpawner)} requires {nameof(_followTarget)}.");
        }

        if (_difficultyProfile == null)
        {
            throw new InvalidOperationException($"{nameof(RoadSectionSpawner)} requires {nameof(_difficultyProfile)}.");
        }

        if (_lowPool == null || _lowPool.Sections.Count == 0)
        {
            throw new InvalidOperationException($"{nameof(RoadSectionSpawner)} requires non-empty low section pool.");
        }

        if (_middlePool == null || _hardPool == null || _veryHardPool == null || _boosterPool == null)
        {
            throw new InvalidOperationException($"{nameof(RoadSectionSpawner)} requires all section pools assigned.");
        }

        if (_middlePool.Sections.Count == 0)
        {
            throw new InvalidOperationException($"{nameof(RoadSectionSpawner)} requires non-empty middle section pool.");
        }

        if (_hardPool.Sections.Count == 0)
        {
            throw new InvalidOperationException($"{nameof(RoadSectionSpawner)} requires non-empty hard section pool.");
        }

        if (_veryHardPool.Sections.Count == 0)
        {
            throw new InvalidOperationException($"{nameof(RoadSectionSpawner)} requires non-empty very hard section pool.");
        }

        if (_boosterPool.Sections.Count == 0)
        {
            throw new InvalidOperationException($"{nameof(RoadSectionSpawner)} requires non-empty booster section pool.");
        }
    }

    private void ResolveDependencies()
    {
        if (_runSessionController == null)
        {
            _runSessionController = FindFirstObjectByType<RunSessionController>();
        }

        if (_followTarget == null)
        {
            Player player = FindFirstObjectByType<Player>();
            _followTarget = player != null ? player.transform : null;
        }

        if (_spawnRoot == null)
        {
            _spawnRoot = transform;
        }

        if (_verboseSpawnLogs)
        {
            LogInfo(
                $"Зависимости подключены: RunSessionController={(_runSessionController != null ? _runSessionController.name : "null")}, " +
                $"FollowTarget={(_followTarget != null ? _followTarget.name : "null")}, SpawnRoot={(_spawnRoot != null ? _spawnRoot.name : "null")}.");
        }
    }

    private bool IsValidSpawnPrefab(GameObject prefab)
    {
        return prefab != null;
    }

    private float ResolveSectionLength(GameObject prefab)
    {
        if (_sectionLengthCache.TryGetValue(prefab, out float cachedLength))
        {
            if (_verboseSpawnLogs)
            {
                LogInfo($"Section length from cache. prefab={prefab.name}, length={cachedLength:F2}");
            }

            return cachedLength;
        }

        float measuredLength = MeasureSectionLength(prefab);
        float resolvedLength = measuredLength > 0.01f
            ? SanitizeMeasuredSectionLength(measuredLength, prefab.name)
            : Mathf.Max(1f, _baselineSectionLength);

        _sectionLengthCache[prefab] = resolvedLength;

        if (_verboseSpawnLogs)
        {
            LogInfo($"Section length measured. prefab={prefab.name}, measured={measuredLength:F2}, resolved={resolvedLength:F2}");
        }

        return resolvedLength;
    }

    private float SanitizeMeasuredSectionLength(float measuredLength, string prefabName)
    {
        float fallbackLength = Mathf.Max(1f, _baselineSectionLength);
        float clampedMinMultiplier = Mathf.Clamp(_minSectionLengthMultiplier, 0.1f, 10f);
        float clampedMaxMultiplier = Mathf.Max(clampedMinMultiplier + 0.01f, _maxSectionLengthMultiplier);
        float minAllowed = fallbackLength * clampedMinMultiplier;
        float maxAllowed = fallbackLength * clampedMaxMultiplier;
        if (measuredLength < minAllowed || measuredLength > maxAllowed)
        {
            LogWarning(
                $"Аномальная длина секции '{prefabName}': measured={measuredLength:F2}, allowed=[{minAllowed:F2}..{maxAllowed:F2}], fallback={fallbackLength:F2}.");
            return fallbackLength;
        }

        return measuredLength;
    }

    private bool ShouldLogSpawn(int spawnIndex)
    {
        if (_enableSpawnLogs == false)
        {
            return false;
        }

        if (_verboseSpawnLogs)
        {
            return true;
        }

        int interval = Mathf.Max(1, _logEveryNthSpawn);
        return spawnIndex % interval == 0;
    }

    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    [System.Diagnostics.Conditional("DEVELOPMENT_BUILD")]
    private void LogInfo(string message)
    {
        if (_enableSpawnLogs == false)
        {
            return;
        }

        Debug.Log($"[RoadSectionSpawner] {message}", this);
    }

    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    [System.Diagnostics.Conditional("DEVELOPMENT_BUILD")]
    private void LogWarning(string message)
    {
        if (_enableSpawnLogs == false)
        {
            return;
        }

        Debug.LogWarning($"[RoadSectionSpawner] {message}", this);
    }

    private int EstimateSectionsUntilBuffered()
    {
        float stride = GetSpawnStride();
        float lookAheadDistance = _sectionsAhead * stride;
        float targetZ = _followTarget.position.z + lookAheadDistance;
        float remainingDistance = targetZ - _nextSpawnZ;
        if (remainingDistance <= 0f)
        {
            return 0;
        }

        return Mathf.CeilToInt(remainingDistance / stride);
    }

    private float GetSpawnStride()
    {
        float sectionLength = Mathf.Max(1f, _lastSpawnedSectionLength);
        return sectionLength + Mathf.Max(0f, _sectionGap);
    }

    private SectionDifficultyTier ApplyDifficultyTierFloor(SectionDifficultyTier tierByScore)
    {
        SectionDifficultyTier minTierByDifficulty = _selectedDifficulty switch
        {
            GameDifficultyLevel.Hard => SectionDifficultyTier.Hard,
            _ => SectionDifficultyTier.Low
        };

        return (SectionDifficultyTier)Mathf.Max((int)tierByScore, (int)minTierByDifficulty);
    }

    private float ResolveBaselineSectionLength()
    {
        float summedLength = 0f;
        int validSections = 0;
        AppendPoolToBaseline(_lowPool.Sections, ref summedLength, ref validSections);
        AppendPoolToBaseline(_middlePool.Sections, ref summedLength, ref validSections);
        AppendPoolToBaseline(_hardPool.Sections, ref summedLength, ref validSections);
        AppendPoolToBaseline(_veryHardPool.Sections, ref summedLength, ref validSections);
        AppendPoolToBaseline(_boosterPool.Sections, ref summedLength, ref validSections);

        if (validSections == 0)
        {
            return Mathf.Max(1f, _sectionLength);
        }

        float averageLength = summedLength / validSections;
        return Mathf.Max(1f, averageLength);
    }

    private void AppendPoolToBaseline(IReadOnlyList<RoadSectionDefinition> sections, ref float summedLength, ref int validSections)
    {
        if (sections == null)
        {
            return;
        }

        for (int i = 0; i < sections.Count; i++)
        {
            GameObject prefab = sections[i].Prefab;
            if (prefab == null)
            {
                continue;
            }

            float measuredLength = MeasureSectionLength(prefab);
            if (measuredLength <= 0.01f)
            {
                continue;
            }

            summedLength += measuredLength;
            validSections++;
        }
    }

    private float MeasureSectionLength(GameObject prefab)
    {
        if (prefab == null)
        {
            return 0f;
        }

        if (TryMeasureSectionBounds(prefab, out float minZ, out float maxZ, out _))
        {
            return Mathf.Max(0f, maxZ - minZ);
        }

        return 0f;
    }

    private static bool TryMeasureSectionBounds(GameObject sectionObject, out float minZ, out float maxZ, out bool usedFloorBounds)
    {
        minZ = 0f;
        maxZ = 0f;
        usedFloorBounds = false;
        if (sectionObject == null)
        {
            return false;
        }

        Transform root = sectionObject.transform;
        if (TryMeasureBounds(root, onlyFloorHierarchy: true, out Bounds floorBounds))
        {
            minZ = floorBounds.min.z;
            maxZ = floorBounds.max.z;
            usedFloorBounds = true;
            return true;
        }

        if (TryMeasureBounds(root, onlyFloorHierarchy: false, out Bounds fullBounds))
        {
            minZ = fullBounds.min.z;
            maxZ = fullBounds.max.z;
            return true;
        }

        return false;
    }

    private static bool TryMeasureBounds(Transform root, bool onlyFloorHierarchy, out Bounds bounds)
    {
        bounds = default;
        bool hasBounds = false;

        Collider[] colliders = root.GetComponentsInChildren<Collider>(true);
        for (int i = 0; i < colliders.Length; i++)
        {
            Collider collider = colliders[i];
            if (collider == null)
            {
                continue;
            }

            if (onlyFloorHierarchy && IsUnderFloorNode(collider.transform, root) == false)
            {
                continue;
            }

            if (hasBounds == false)
            {
                bounds = collider.bounds;
                hasBounds = true;
                continue;
            }

            bounds.Encapsulate(collider.bounds);
        }

        if (hasBounds)
        {
            return true;
        }

        Renderer[] renderers = root.GetComponentsInChildren<Renderer>(true);
        for (int i = 0; i < renderers.Length; i++)
        {
            Renderer renderer = renderers[i];
            if (renderer == null)
            {
                continue;
            }

            if (onlyFloorHierarchy && IsUnderFloorNode(renderer.transform, root) == false)
            {
                continue;
            }

            if (hasBounds == false)
            {
                bounds = renderer.bounds;
                hasBounds = true;
                continue;
            }

            bounds.Encapsulate(renderer.bounds);
        }

        return hasBounds;
    }

    private static bool IsUnderFloorNode(Transform node, Transform sectionRoot)
    {
        if (node == null || sectionRoot == null)
        {
            return false;
        }

        Transform current = node;
        while (current != null)
        {
            if (current.name.IndexOf("floor", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return true;
            }

            if (current == sectionRoot)
            {
                break;
            }

            current = current.parent;
        }

        return false;
    }
}
