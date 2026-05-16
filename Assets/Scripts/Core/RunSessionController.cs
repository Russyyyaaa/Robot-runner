using System;
using UnityEngine;

public sealed class RunSessionController : MonoBehaviour
{
    [SerializeField] private GameBalanceConfig _gameBalanceConfig;
    [SerializeField] private Player _player;
    [SerializeField] private RunnerInputRouter _runnerInputRouter;
    [SerializeField] private bool _autoStartRun = true;

    private readonly RunSessionEvents _runSessionEvents = new RunSessionEvents();
    private readonly RunSessionStateModel _runSessionStateModel = new RunSessionStateModel();
    private BatteryRuntime _batteryRuntime;
    private ScoreRuntime _scoreRuntime;
    private BoosterRuntime _boosterRuntime;
    private int _lastPublishedScore = -1;
    private float _lastPublishedBattery = -1f;

    public event Action<GameState> StateChanged
    {
        add => _runSessionEvents.StateChanged += value;
        remove => _runSessionEvents.StateChanged -= value;
    }

    public event Action<int> ScoreChanged
    {
        add => _runSessionEvents.ScoreChanged += value;
        remove => _runSessionEvents.ScoreChanged -= value;
    }

    public event Action<float> BatteryChanged
    {
        add => _runSessionEvents.BatteryChanged += value;
        remove => _runSessionEvents.BatteryChanged -= value;
    }

    public event Action<int> RunEnded
    {
        add => _runSessionEvents.RunEnded += value;
        remove => _runSessionEvents.RunEnded -= value;
    }

    public GameState CurrentState => _runSessionStateModel.CurrentState;
    public bool IsBoosterFlightActive => _boosterRuntime != null && _boosterRuntime.IsActive;

    private void Awake()
    {
        EnsureDependencies();
        EnsureRuntimeBinder();

        _player.ApplyConfig(_gameBalanceConfig);
        GameDifficultyLevel selectedDifficulty = GameDifficultyService.GetSelectedDifficulty();
        DifficultyRuntimeModifiers difficultyModifiers = GameDifficultyService.GetRuntimeModifiers(selectedDifficulty);
        _player.SetBaseSpeedMultiplier(difficultyModifiers.BaseSpeedMultiplier);
        _player.Initialize();
        EnsureCameraFollowTarget();

        _batteryRuntime = new BatteryRuntime(
            _gameBalanceConfig,
            difficultyModifiers.BatteryDrainMultiplier,
            difficultyModifiers.BatteryRestoreMultiplier,
            difficultyModifiers.TrapDamageMultiplier);
        _scoreRuntime = new ScoreRuntime(_gameBalanceConfig);
        _boosterRuntime = new BoosterRuntime(
            _gameBalanceConfig,
            difficultyModifiers.BoosterSpeedBonusMultiplier,
            difficultyModifiers.BoosterDurationMultiplier);
        _player.SetBoosterFlightActive(false);

        PublishScoreIfChanged();
        PublishBatteryIfChanged();
    }

    private void OnEnable()
    {
        if (_runnerInputRouter != null)
        {
            _runnerInputRouter.MoveLeftRequested += HandleMoveLeftRequested;
            _runnerInputRouter.MoveRightRequested += HandleMoveRightRequested;
        }
    }

    private void Start()
    {
        if (_autoStartRun)
        {
            StartRun();
        }
    }

    private void OnDisable()
    {
        if (_runnerInputRouter != null)
        {
            _runnerInputRouter.MoveLeftRequested -= HandleMoveLeftRequested;
            _runnerInputRouter.MoveRightRequested -= HandleMoveRightRequested;
        }
    }

    private void Update()
    {
        if (_runSessionStateModel.IsRunning == false)
        {
            return;
        }

        float deltaTime = Time.deltaTime;
        _boosterRuntime.Tick(deltaTime);
        _player.SetRuntimeSpeedBonus(_boosterRuntime.CurrentSpeedBonus);
        _player.SetBoosterFlightActive(_boosterRuntime.IsActive);
        _player.Tick(deltaTime);

        if (_boosterRuntime.IsActive == false)
        {
            _batteryRuntime.Tick(deltaTime);
        }

        _scoreRuntime.Tick(_player.ForwardSpeed, deltaTime, 1f);

        PublishScoreIfChanged();
        PublishBatteryIfChanged();

        if (_batteryRuntime.IsEmpty)
        {
            EndRun();
        }
    }

    public void StartRun()
    {
        if (_runSessionStateModel.TryStartRun() == false)
        {
            return;
        }

        _batteryRuntime.Reset();
        _scoreRuntime.Reset();
        _boosterRuntime.Reset();
        _player.SetRuntimeSpeedBonus(0f);
        _player.SetBoosterFlightActive(false);

        _lastPublishedScore = -1;
        _lastPublishedBattery = -1f;
        PublishScoreIfChanged();
        PublishBatteryIfChanged();

        _runSessionEvents.RaiseStateChanged(_runSessionStateModel.CurrentState);
    }

    public void EndRun()
    {
        if (_runSessionStateModel.TryEndRun() == false)
        {
            return;
        }

        _player.SetRuntimeSpeedBonus(0f);
        _player.SetBoosterFlightActive(false);
        _runSessionEvents.RaiseStateChanged(_runSessionStateModel.CurrentState);
        _runSessionEvents.RaiseRunEnded(_scoreRuntime.CurrentScore);
    }

    public void RestoreBattery(float amount)
    {
        _batteryRuntime.Restore(amount);
        PublishBatteryIfChanged();
    }

    public void RestoreDefaultBatteryAmount()
    {
        _batteryRuntime.RestoreDefaultAmount();
        PublishBatteryIfChanged();
    }

    public void ApplyTrapDamage(float amount)
    {
        if (IsBoosterFlightActive)
        {
            return;
        }

        _batteryRuntime.ApplyDamage(amount);
        PublishBatteryIfChanged();

        if (_batteryRuntime.IsEmpty)
        {
            EndRun();
        }
    }

    public void ApplyDefaultTrapDamage()
    {
        if (IsBoosterFlightActive)
        {
            return;
        }

        _batteryRuntime.ApplyDefaultTrapDamage();
        PublishBatteryIfChanged();

        if (_batteryRuntime.IsEmpty)
        {
            EndRun();
        }
    }

    public void ActivateBooster(float speedBonus, float durationSeconds)
    {
        _boosterRuntime.Activate(speedBonus, durationSeconds);
        _player.SetBoosterFlightActive(true);
    }

    public void ActivateDefaultBooster()
    {
        _boosterRuntime.ActivateDefault();
        _player.SetBoosterFlightActive(true);
    }

    public void ReturnToMenu()
    {
        if (_runSessionStateModel.TryReturnToMenu() == false)
        {
            return;
        }

        _player.SetRuntimeSpeedBonus(0f);
        _player.SetBoosterFlightActive(false);
        _runSessionEvents.RaiseStateChanged(_runSessionStateModel.CurrentState);
    }

    private void EnsureDependencies()
    {
        if (_gameBalanceConfig == null)
        {
            throw new InvalidOperationException($"{nameof(RunSessionController)} requires {nameof(_gameBalanceConfig)}.");
        }

        if (_player == null)
        {
            throw new InvalidOperationException($"{nameof(RunSessionController)} requires {nameof(_player)}.");
        }
    }

    private static void EnsureRuntimeBinder()
    {
        if (FindFirstObjectByType<RuntimePrefabBinder>() != null)
        {
            return;
        }

        GameObject binderObject = new GameObject("RuntimePrefabBinder");
        binderObject.AddComponent<RuntimePrefabBinder>();
    }

    private void EnsureCameraFollowTarget()
    {
        RunnerCameraFollow cameraFollow = null;
        Camera mainCamera = Camera.main;
        if (mainCamera != null)
        {
            cameraFollow = mainCamera.GetComponent<RunnerCameraFollow>();
        }

        if (cameraFollow == null)
        {
            cameraFollow = FindFirstObjectByType<RunnerCameraFollow>();
        }

        if (cameraFollow == null)
        {
            Camera fallbackCamera = mainCamera != null ? mainCamera : FindFirstObjectByType<Camera>();
            if (fallbackCamera == null)
            {
                return;
            }

            cameraFollow = fallbackCamera.gameObject.AddComponent<RunnerCameraFollow>();
        }

        cameraFollow.SetTarget(_player.transform);
    }

    private void HandleMoveLeftRequested()
    {
        if (_runSessionStateModel.IsRunning == false)
        {
            return;
        }

        _player.MoveLeft();
    }

    private void HandleMoveRightRequested()
    {
        if (_runSessionStateModel.IsRunning == false)
        {
            return;
        }

        _player.MoveRight();
    }

    private void PublishScoreIfChanged()
    {
        int currentScore = _scoreRuntime.CurrentScore;

        if (currentScore == _lastPublishedScore)
        {
            return;
        }

        _lastPublishedScore = currentScore;
        _runSessionEvents.RaiseScoreChanged(currentScore);
    }

    private void PublishBatteryIfChanged()
    {
        float normalizedBattery = _batteryRuntime.NormalizedCharge;

        if (Mathf.Abs(normalizedBattery - _lastPublishedBattery) < 0.0001f)
        {
            return;
        }

        _lastPublishedBattery = normalizedBattery;
        _runSessionEvents.RaiseBatteryChanged(normalizedBattery);
    }
}
