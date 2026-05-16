#if UNITY_INCLUDE_TESTS
using NUnit.Framework;

public sealed class BoosterModelPhase1Tests
{
    [Test]
    public void Activate_SetsActiveAndBonus()
    {
        BoosterModel boosterModel = new BoosterModel();

        boosterModel.Activate(3f, 4f);

        Assert.IsTrue(boosterModel.IsActive);
        Assert.AreEqual(3f, boosterModel.CurrentSpeedBonus, 0.0001f);
    }

    [Test]
    public void Tick_WhenDurationExpires_DeactivatesBooster()
    {
        BoosterModel boosterModel = new BoosterModel();
        boosterModel.Activate(3f, 1f);

        boosterModel.Tick(1.5f);

        Assert.IsFalse(boosterModel.IsActive);
        Assert.AreEqual(0f, boosterModel.CurrentSpeedBonus, 0.0001f);
    }
}
#endif
