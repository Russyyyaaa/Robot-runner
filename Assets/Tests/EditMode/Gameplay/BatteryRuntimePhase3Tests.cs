#if UNITY_INCLUDE_TESTS
using NUnit.Framework;
using UnityEngine;

public sealed class BatteryRuntimePhase3Tests
{
    [Test]
    public void Tick_WhenCalled_DecreasesNormalizedCharge()
    {
        GameBalanceConfig config = RuntimeConfigTestFactory.CreateDefault();
        BatteryRuntime batteryRuntime = new BatteryRuntime(config);

        batteryRuntime.Tick(1f);

        Assert.AreEqual(0.92f, batteryRuntime.NormalizedCharge, 0.0001f);
        Object.DestroyImmediate(config);
    }

    [Test]
    public void RestoreAndDamage_UseConfiguredDefaults()
    {
        GameBalanceConfig config = RuntimeConfigTestFactory.CreateDefault();
        BatteryRuntime batteryRuntime = new BatteryRuntime(config);

        batteryRuntime.ApplyDefaultTrapDamage();
        batteryRuntime.RestoreDefaultAmount();

        Assert.AreEqual(0.9f, batteryRuntime.NormalizedCharge, 0.0001f);
        Object.DestroyImmediate(config);
    }

    [Test]
    public void Reset_RestoresStartCharge()
    {
        GameBalanceConfig config = RuntimeConfigTestFactory.CreateDefault();
        BatteryRuntime batteryRuntime = new BatteryRuntime(config);

        batteryRuntime.ApplyDamage(80f);
        batteryRuntime.Reset();

        Assert.AreEqual(1f, batteryRuntime.NormalizedCharge, 0.0001f);
        Object.DestroyImmediate(config);
    }

    [Test]
    public void Constructor_WithDifficultyMultipliers_AppliesScaledDefaults()
    {
        GameBalanceConfig config = RuntimeConfigTestFactory.CreateDefault();
        BatteryRuntime batteryRuntime = new BatteryRuntime(config, drainMultiplier: 0.5f, restoreMultiplier: 2f, trapDamageMultiplier: 0.5f);

        batteryRuntime.Tick(1f);
        batteryRuntime.ApplyDefaultTrapDamage();

        Assert.AreEqual(0.785f, batteryRuntime.NormalizedCharge, 0.0001f);
        Object.DestroyImmediate(config);
    }
}
#endif
