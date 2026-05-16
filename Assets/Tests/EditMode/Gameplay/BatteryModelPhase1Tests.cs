#if UNITY_INCLUDE_TESTS
using NUnit.Framework;

public sealed class BatteryModelPhase1Tests
{
    [Test]
    public void Tick_WhenDrainApplied_DecreasesCharge()
    {
        BatteryModel batteryModel = new BatteryModel(100f, 100f, 8f);

        batteryModel.Tick(1f);

        Assert.AreEqual(92f, batteryModel.CurrentCharge, 0.0001f);
    }

    [Test]
    public void Restore_WhenAboveMax_ClampsToMax()
    {
        BatteryModel batteryModel = new BatteryModel(100f, 90f, 8f);

        batteryModel.Restore(25f);

        Assert.AreEqual(100f, batteryModel.CurrentCharge, 0.0001f);
    }
}
#endif
