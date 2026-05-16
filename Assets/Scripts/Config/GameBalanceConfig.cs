using UnityEngine;

[CreateAssetMenu(fileName = "GameBalanceConfig", menuName = "RobotRun/Config/Game Balance")]
public sealed class GameBalanceConfig : ScriptableObject
{
    [SerializeField] private int _laneCount = 3;
    [SerializeField] private int _startLaneIndex = 1;
    [SerializeField] private float _laneOffset = 1.7f;
    [SerializeField] private float _laneChangeDuration = 0.2f;

    [SerializeField] private float _baseForwardSpeed = 7f;
    [SerializeField] private float _boosterSpeedBonus = 3f;
    [SerializeField] private float _boosterDurationSeconds = 4f;

    [SerializeField] private float _maxBatteryCharge = 100f;
    [SerializeField] private float _startBatteryCharge = 100f;
    [SerializeField] private float _batteryDrainPerSecond = 8f;
    [SerializeField] private float _batteryRestoreAmount = 25f;
    [SerializeField] private float _trapDamageAmount = 20f;

    [SerializeField] private float _scorePerDistanceUnit = 1f;

    public int LaneCount => _laneCount;
    public int StartLaneIndex => _startLaneIndex;
    public float LaneOffset => _laneOffset;
    public float LaneChangeDuration => _laneChangeDuration;

    public float BaseForwardSpeed => _baseForwardSpeed;
    public float BoosterSpeedBonus => _boosterSpeedBonus;
    public float BoosterDurationSeconds => _boosterDurationSeconds;

    public float MaxBatteryCharge => _maxBatteryCharge;
    public float StartBatteryCharge => _startBatteryCharge;
    public float BatteryDrainPerSecond => _batteryDrainPerSecond;
    public float BatteryRestoreAmount => _batteryRestoreAmount;
    public float TrapDamageAmount => _trapDamageAmount;

    public float ScorePerDistanceUnit => _scorePerDistanceUnit;
}
