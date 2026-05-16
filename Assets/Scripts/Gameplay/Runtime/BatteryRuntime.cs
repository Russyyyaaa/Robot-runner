using System;

public sealed class BatteryRuntime
{
    private readonly float _restoreAmount;
    private readonly float _trapDamageAmount;
    private readonly BatteryModel _batteryModel;

    public BatteryRuntime(
        GameBalanceConfig gameBalanceConfig,
        float drainMultiplier = 1f,
        float restoreMultiplier = 1f,
        float trapDamageMultiplier = 1f)
    {
        if (gameBalanceConfig == null)
        {
            throw new ArgumentNullException(nameof(gameBalanceConfig));
        }

        float resolvedDrainMultiplier = drainMultiplier <= 0f ? 1f : drainMultiplier;
        float resolvedRestoreMultiplier = restoreMultiplier <= 0f ? 1f : restoreMultiplier;
        float resolvedTrapDamageMultiplier = trapDamageMultiplier <= 0f ? 1f : trapDamageMultiplier;

        _batteryModel = new BatteryModel(
            gameBalanceConfig.MaxBatteryCharge,
            gameBalanceConfig.StartBatteryCharge,
            gameBalanceConfig.BatteryDrainPerSecond * resolvedDrainMultiplier);

        _restoreAmount = gameBalanceConfig.BatteryRestoreAmount * resolvedRestoreMultiplier;
        _trapDamageAmount = gameBalanceConfig.TrapDamageAmount * resolvedTrapDamageMultiplier;
    }

    public float NormalizedCharge => _batteryModel.NormalizedCharge;
    public bool IsEmpty => _batteryModel.IsEmpty;

    public void Tick(float deltaTime)
    {
        _batteryModel.Tick(deltaTime);
    }

    public void RestoreDefaultAmount()
    {
        _batteryModel.Restore(_restoreAmount);
    }

    public void ApplyDefaultTrapDamage()
    {
        _batteryModel.ApplyDamage(_trapDamageAmount);
    }

    public void Restore(float amount)
    {
        _batteryModel.Restore(amount);
    }

    public void ApplyDamage(float amount)
    {
        _batteryModel.ApplyDamage(amount);
    }

    public void Reset()
    {
        _batteryModel.Reset();
    }
}
