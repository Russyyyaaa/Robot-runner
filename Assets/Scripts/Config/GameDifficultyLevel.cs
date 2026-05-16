public enum GameDifficultyLevel
{
    Easy = 0,
    Normal = 1,
    Hard = 2
}

public readonly struct DifficultyRuntimeModifiers
{
    public DifficultyRuntimeModifiers(
        float baseSpeedMultiplier,
        float batteryDrainMultiplier,
        float batteryRestoreMultiplier,
        float trapDamageMultiplier,
        float boosterSpeedBonusMultiplier,
        float boosterDurationMultiplier,
        float sectionThresholdMultiplier,
        float boosterIntervalMultiplier)
    {
        BaseSpeedMultiplier = baseSpeedMultiplier;
        BatteryDrainMultiplier = batteryDrainMultiplier;
        BatteryRestoreMultiplier = batteryRestoreMultiplier;
        TrapDamageMultiplier = trapDamageMultiplier;
        BoosterSpeedBonusMultiplier = boosterSpeedBonusMultiplier;
        BoosterDurationMultiplier = boosterDurationMultiplier;
        SectionThresholdMultiplier = sectionThresholdMultiplier;
        BoosterIntervalMultiplier = boosterIntervalMultiplier;
    }

    public float BaseSpeedMultiplier { get; }
    public float BatteryDrainMultiplier { get; }
    public float BatteryRestoreMultiplier { get; }
    public float TrapDamageMultiplier { get; }
    public float BoosterSpeedBonusMultiplier { get; }
    public float BoosterDurationMultiplier { get; }
    public float SectionThresholdMultiplier { get; }
    public float BoosterIntervalMultiplier { get; }
}
