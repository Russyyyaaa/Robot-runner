#if UNITY_INCLUDE_TESTS
using NUnit.Framework;

public sealed class BatteryModelTests
{
    [Test]
    public void DrainRestoreDamage_RespectBoundsAndGameOverThreshold()
    {
        BatteryModel model = new BatteryModel(100f, 100f, 8f);

        model.Tick(10f);
        model.Restore(25f);
        model.ApplyDamage(200f);

        Assert.AreEqual(0f, model.CurrentCharge, 0.0001f);
        Assert.IsTrue(model.IsEmpty);
    }
}
#endif
