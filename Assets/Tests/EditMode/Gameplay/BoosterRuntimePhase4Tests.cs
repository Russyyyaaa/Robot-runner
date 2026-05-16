#if UNITY_INCLUDE_TESTS
using NUnit.Framework;
using UnityEngine;

public sealed class BoosterRuntimePhase4Tests
{
    [Test]
    public void ActivateDefault_WhenTicked_ResetsAfterDuration()
    {
        GameBalanceConfig config = RuntimeConfigTestFactory.CreateDefault();
        BoosterRuntime boosterRuntime = new BoosterRuntime(config);

        boosterRuntime.ActivateDefault();
        float activeBonus = boosterRuntime.CurrentSpeedBonus;
        boosterRuntime.Tick(10f);

        Assert.Greater(activeBonus, 0f);
        Assert.AreEqual(0f, boosterRuntime.CurrentSpeedBonus, 0.0001f);
        Object.DestroyImmediate(config);
    }

    [Test]
    public void Activate_WithCustomValues_UsesProvidedSpeedBonus()
    {
        GameBalanceConfig config = RuntimeConfigTestFactory.CreateDefault();
        BoosterRuntime boosterRuntime = new BoosterRuntime(config);

        boosterRuntime.Activate(6f, 2f);

        Assert.AreEqual(6f, boosterRuntime.CurrentSpeedBonus, 0.0001f);
        Object.DestroyImmediate(config);
    }

    [Test]
    public void ActivateDefault_WithMultipliers_UsesScaledDefaults()
    {
        GameBalanceConfig config = RuntimeConfigTestFactory.CreateDefault();
        BoosterRuntime boosterRuntime = new BoosterRuntime(config, speedBonusMultiplier: 2f, durationMultiplier: 0.5f);

        boosterRuntime.ActivateDefault();
        Assert.AreEqual(6f, boosterRuntime.CurrentSpeedBonus, 0.0001f);
        boosterRuntime.Tick(2.1f);

        Assert.AreEqual(0f, boosterRuntime.CurrentSpeedBonus, 0.0001f);
        Object.DestroyImmediate(config);
    }
}
#endif
