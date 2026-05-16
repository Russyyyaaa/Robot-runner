using UnityEngine;
using UnityEngine.SceneManagement;

public sealed class RuntimePrefabBinder : MonoBehaviour
{
    [SerializeField] private bool _enableBindingLogs;
    [SerializeField] private bool _verboseBindingLogs;
    [SerializeField] private bool _enableLegacyNameBinding = true;

    private static RuntimePrefabBinder _instance;
    private static bool _enableBindingLogsRuntime;
    private static bool _verboseBindingLogsRuntime;
    private static bool _enableLegacyNameBindingRuntime = true;

    private enum BindKind
    {
        None = 0,
        Trap = 1,
        Booster = 2,
        Battery = 3
    }

    private readonly struct BindStats
    {
        public BindStats(int scannedColliders, int trapBound, int boosterBound, int batteryBound)
        {
            ScannedColliders = scannedColliders;
            TrapBound = trapBound;
            BoosterBound = boosterBound;
            BatteryBound = batteryBound;
        }

        public int ScannedColliders { get; }
        public int TrapBound { get; }
        public int BoosterBound { get; }
        public int BatteryBound { get; }
    }

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
        DontDestroyOnLoad(gameObject);
        _enableBindingLogsRuntime = _enableBindingLogs;
        _verboseBindingLogsRuntime = _verboseBindingLogs;
        _enableLegacyNameBindingRuntime = _enableLegacyNameBinding;

        RebindSceneObjects();
        LogInfo($"Runtime binder инициализирован. Legacy recovery mode={_enableLegacyNameBindingRuntime}.");
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += HandleSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= HandleSceneLoaded;
    }

    private void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        LogInfo($"Сцена загружена: {scene.name} ({mode}). Запускаю повторную привязку.");
        RebindSceneObjects();
    }

    public static void RebindSceneObjects()
    {
        int addedHandlerCount = EnsureRunnerHasCollisionHandler();
        BindStats stats = BindPickupsAndTraps();

        LogInfo(
            $"Привязка завершена. Проверено коллайдеров={stats.ScannedColliders}, добавлено trap={stats.TrapBound}, booster={stats.BoosterBound}, battery={stats.BatteryBound}, добавлен RunnerCollisionHandler={addedHandlerCount}.");

        if (_verboseBindingLogsRuntime && stats.TrapBound + stats.BoosterBound + stats.BatteryBound == 0)
        {
            LogWarning("Ни один триггер не был привязан автоматически. Для секций нужны TriggerActorMarker (Trap/Booster/Battery) или заранее добавленные Adapter-компоненты.");
        }
    }

    private static int EnsureRunnerHasCollisionHandler()
    {
        Player player = FindFirstObjectByType<Player>();
        if (player == null)
        {
            LogWarning("Player не найден при проверке RunnerCollisionHandler.");
            return 0;
        }

        EnsureRunnerPhysics(player.gameObject);

        if (player.GetComponent<RunnerCollisionHandler>() == null)
        {
            player.gameObject.AddComponent<RunnerCollisionHandler>();
            LogInfo($"RunnerCollisionHandler добавлен на {player.name}.");
            return 1;
        }

        return 0;
    }

    private static BindStats BindPickupsAndTraps()
    {
        Collider[] colliders = FindObjectsByType<Collider>(FindObjectsSortMode.None);
        int trapBound = 0;
        int boosterBound = 0;
        int batteryBound = 0;

        for (int i = 0; i < colliders.Length; i++)
        {
            Collider collider = colliders[i];
            BindKind bindKind = TryBindCollider(collider);
            if (bindKind == BindKind.Trap)
            {
                trapBound++;
                continue;
            }

            if (bindKind == BindKind.Booster)
            {
                boosterBound++;
                continue;
            }

            if (bindKind == BindKind.Battery)
            {
                batteryBound++;
            }
        }

        return new BindStats(colliders.Length, trapBound, boosterBound, batteryBound);
    }

    public static void BindHierarchy(GameObject root)
    {
        if (root == null)
        {
            return;
        }

        Collider[] colliders = root.GetComponentsInChildren<Collider>(true);
        int trapBound = 0;
        int boosterBound = 0;
        int batteryBound = 0;

        for (int i = 0; i < colliders.Length; i++)
        {
            Collider collider = colliders[i];
            BindKind bindKind = TryBindCollider(collider);
            if (bindKind == BindKind.Trap)
            {
                trapBound++;
                continue;
            }

            if (bindKind == BindKind.Booster)
            {
                boosterBound++;
                continue;
            }

            if (bindKind == BindKind.Battery)
            {
                batteryBound++;
            }
        }

        if (_verboseBindingLogsRuntime)
        {
            LogInfo(
                $"BindHierarchy для '{root.name}': проверено={colliders.Length}, добавлено trap={trapBound}, booster={boosterBound}, battery={batteryBound}.");
        }
    }

    private static BindKind TryBindCollider(Collider collider)
    {
        if (collider == null)
        {
            return BindKind.None;
        }

        if (collider.GetComponentInParent<Player>() != null)
        {
            return BindKind.None;
        }

        ITriggerActor existingActor = collider.GetComponentInParent<ITriggerActor>();
        if (existingActor != null)
        {
            EnsureTriggerCollider(collider);
            if (existingActor is MonoBehaviour existingActorBehaviour)
            {
                EnsureActorTriggerColliders(existingActorBehaviour.gameObject);
            }

            return BindKind.None;
        }

        TriggerActorMarker marker = collider.GetComponentInParent<TriggerActorMarker>();
        if (marker == null)
        {
            if (_enableLegacyNameBindingRuntime == false)
            {
                return BindKind.None;
            }

            GameObject legacyTarget = ResolveLegacyBindingTarget(collider);
            if (legacyTarget == null)
            {
                return BindKind.None;
            }

            BindKind legacyKind = ResolveBindKindByName(legacyTarget);
            if (legacyKind == BindKind.None)
            {
                return BindKind.None;
            }

            EnsureTriggerCollider(collider);
            return BindToTarget(legacyTarget, legacyKind, $"legacy:{legacyTarget.name}");
        }

        BindKind bindKind = ResolveMarkerKind(marker.Kind);
        if (bindKind == BindKind.None)
        {
            if (_verboseBindingLogsRuntime)
            {
                LogWarning($"Маркер на '{marker.name}' имеет Kind=None. Коллайдер '{collider.name}' пропущен.");
            }

            return BindKind.None;
        }

        EnsureTriggerCollider(collider);
        GameObject markerTarget = marker.gameObject;
        return BindToTarget(markerTarget, bindKind, $"marker:{marker.Kind}");
    }

    private static BindKind ResolveMarkerKind(TriggerActorMarkerKind markerKind)
    {
        if (markerKind == TriggerActorMarkerKind.Trap)
        {
            return BindKind.Trap;
        }

        if (markerKind == TriggerActorMarkerKind.Booster)
        {
            return BindKind.Booster;
        }

        if (markerKind == TriggerActorMarkerKind.Battery)
        {
            return BindKind.Battery;
        }

        return BindKind.None;
    }

    private static bool EnsureComponent<T>(GameObject target) where T : Component
    {
        if (target.GetComponentInParent<T>() != null)
        {
            return false;
        }

        target.AddComponent<T>();
        return true;
    }

    private static BindKind BindToTarget(GameObject target, BindKind bindKind, string source)
    {
        if (target == null || bindKind == BindKind.None)
        {
            return BindKind.None;
        }

        if (bindKind == BindKind.Trap)
        {
            bool added = EnsureComponent<TrapZoneAdapter>(target);
            EnsureActorTriggerColliders(target);
            if (added && _verboseBindingLogsRuntime)
            {
                LogInfo($"TrapZoneAdapter added on {target.name} ({source}).");
            }

            return added ? BindKind.Trap : BindKind.None;
        }

        if (bindKind == BindKind.Booster)
        {
            bool added = EnsureComponent<BoosterPickupAdapter>(target);
            EnsureActorTriggerColliders(target);
            if (added && _verboseBindingLogsRuntime)
            {
                LogInfo($"BoosterPickupAdapter added on {target.name} ({source}).");
            }

            return added ? BindKind.Booster : BindKind.None;
        }

        if (bindKind == BindKind.Battery)
        {
            bool added = EnsureComponent<BatteryPickupAdapter>(target);
            EnsureActorTriggerColliders(target);
            if (added && _verboseBindingLogsRuntime)
            {
                LogInfo($"BatteryPickupAdapter added on {target.name} ({source}).");
            }

            return added ? BindKind.Battery : BindKind.None;
        }

        return BindKind.None;
    }

    private static GameObject ResolveLegacyBindingTarget(Collider collider)
    {
        Transform current = collider.transform;
        Transform resolved = null;
        int depth = 0;

        while (current != null && depth < 12)
        {
            string loweredName = current.name.ToLowerInvariant();
            if (loweredName.Contains("roadsection"))
            {
                break;
            }

            if (IsIgnoredLegacyNode(loweredName) == false)
            {
                resolved = current;
                if (IsStrongLegacyRoot(loweredName))
                {
                    break;
                }
            }

            current = current.parent;
            depth++;
        }

        return resolved != null ? resolved.gameObject : null;
    }

    private static bool IsIgnoredLegacyNode(string loweredName)
    {
        return loweredName.Contains("itemholder")
            || loweredName.Contains("shadowholder")
            || loweredName.Contains("shadow")
            || loweredName.Contains("center")
            || loweredName.Contains("glow")
            || loweredName.Contains("capsule")
            || loweredName.Contains("cylinder")
            || loweredName.Contains("cube")
            || loweredName.Contains("mesh")
            || loweredName.Contains("floor");
    }

    private static bool IsStrongLegacyRoot(string loweredName)
    {
        return loweredName.Contains("battery")
            || loweredName.Contains("booster")
            || loweredName.Contains("trap")
            || loweredName.Contains("item");
    }

    private static BindKind ResolveBindKindByName(GameObject target)
    {
        if (target == null)
        {
            return BindKind.None;
        }

        Transform transform = target.transform;
        if (NameInHierarchy(transform, "battery") || NameInSubtree(transform, "battery") || NameInSubtree(transform, "akum1"))
        {
            return BindKind.Battery;
        }

        if (NameInHierarchy(transform, "booster") || NameInSubtree(transform, "booster") || NameInSubtree(transform, "akum2"))
        {
            return BindKind.Booster;
        }

        if (NameInHierarchy(transform, "trap")
            || NameInSubtree(transform, "trap")
            || NameInSubtree(transform, "trapzone")
            || NameInSubtree(transform, "lightning")
            || NameInSubtree(transform, "orb"))
        {
            return BindKind.Trap;
        }

        return BindKind.None;
    }

    private static bool NameInHierarchy(Transform transform, string token)
    {
        if (transform == null || string.IsNullOrWhiteSpace(token))
        {
            return false;
        }

        string loweredToken = token.ToLowerInvariant();
        Transform current = transform;
        int depth = 0;
        while (current != null && depth < 8)
        {
            if (current.name.ToLowerInvariant().Contains(loweredToken))
            {
                return true;
            }

            current = current.parent;
            depth++;
        }

        return false;
    }

    private static bool NameInSubtree(Transform transform, string token)
    {
        if (transform == null || string.IsNullOrWhiteSpace(token))
        {
            return false;
        }

        string loweredToken = token.ToLowerInvariant();
        Transform[] nodes = transform.GetComponentsInChildren<Transform>(true);
        int maxNodesToScan = Mathf.Min(nodes.Length, 64);
        for (int i = 0; i < maxNodesToScan; i++)
        {
            Transform node = nodes[i];
            if (node == null)
            {
                continue;
            }

            if (node.name.ToLowerInvariant().Contains(loweredToken))
            {
                return true;
            }
        }

        return false;
    }

    private static void EnsureRunnerPhysics(GameObject playerObject)
    {
        Rigidbody body = playerObject.GetComponent<Rigidbody>();
        if (body == null)
        {
            body = playerObject.AddComponent<Rigidbody>();
        }

        body.isKinematic = true;
        body.useGravity = false;

        Collider collider = playerObject.GetComponent<Collider>();
        if (collider == null)
        {
            CapsuleCollider capsuleCollider = playerObject.AddComponent<CapsuleCollider>();
            capsuleCollider.height = 2f;
            capsuleCollider.radius = 0.5f;
            collider = capsuleCollider;
        }

        collider.isTrigger = false;
    }

    private static void EnsureTriggerCollider(Collider collider)
    {
        collider.isTrigger = true;
    }

    private static void EnsureActorTriggerColliders(GameObject actorRoot)
    {
        if (actorRoot == null)
        {
            return;
        }

        Collider[] colliders = actorRoot.GetComponentsInChildren<Collider>(true);
        for (int i = 0; i < colliders.Length; i++)
        {
            Collider actorCollider = colliders[i];
            if (actorCollider == null)
            {
                continue;
            }

            actorCollider.isTrigger = true;
        }
    }

    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    [System.Diagnostics.Conditional("DEVELOPMENT_BUILD")]
    private static void LogInfo(string message)
    {
        if (_enableBindingLogsRuntime == false)
        {
            return;
        }

        if (_instance != null)
        {
            Debug.Log($"[RuntimePrefabBinder] {message}", _instance);
            return;
        }

        Debug.Log($"[RuntimePrefabBinder] {message}");
    }

    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    [System.Diagnostics.Conditional("DEVELOPMENT_BUILD")]
    private static void LogWarning(string message)
    {
        if (_enableBindingLogsRuntime == false)
        {
            return;
        }

        if (_instance != null)
        {
            Debug.LogWarning($"[RuntimePrefabBinder] {message}", _instance);
            return;
        }

        Debug.LogWarning($"[RuntimePrefabBinder] {message}");
    }
}
