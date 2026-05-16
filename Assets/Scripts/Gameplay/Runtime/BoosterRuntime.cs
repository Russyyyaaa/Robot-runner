using System;

public sealed class BoosterRuntime
{
    private readonly float _defaultSpeedBonus;
    private readonly float _defaultDurationSeconds;
    private readonly BoosterModel _boosterModel;

    public BoosterRuntime(
        GameBalanceConfig gameBalanceConfig,
        float speedBonusMultiplier = 1f,
        float durationMultiplier = 1f)
    {
        if (gameBalanceConfig == null)
        {
            throw new ArgumentNullException(nameof(gameBalanceConfig));
        }

        float resolvedSpeedBonusMultiplier = speedBonusMultiplier <= 0f ? 1f : speedBonusMultiplier;
        float resolvedDurationMultiplier = durationMultiplier <= 0f ? 1f : durationMultiplier;
        _defaultSpeedBonus = gameBalanceConfig.BoosterSpeedBonus * resolvedSpeedBonusMultiplier;
        _defaultDurationSeconds = gameBalanceConfig.BoosterDurationSeconds * resolvedDurationMultiplier;
        _boosterModel = new BoosterModel();
    }

    public float CurrentSpeedBonus => _boosterModel.CurrentSpeedBonus;
    public bool IsActive => _boosterModel.IsActive;

    public void Tick(float deltaTime)
    {
        _boosterModel.Tick(deltaTime);
    }

    public void Activate(float speedBonus, float durationSeconds)
    {
        _boosterModel.Activate(speedBonus, durationSeconds);
    }

    public void ActivateDefault()
    {
        _boosterModel.Activate(_defaultSpeedBonus, _defaultDurationSeconds);
    }

    public void Reset()
    {
        _boosterModel.Reset();
    }
}
