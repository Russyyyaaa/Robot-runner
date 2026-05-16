#if UNITY_INCLUDE_TESTS
using NUnit.Framework;

public sealed class BoosterModelTests
{
    [Test]
    public void ActivateAndTick_EnablesThenExpiresBooster()
    {
        BoosterModel model = new BoosterModel();
        model.Activate(3f, 2f);

        Assert.IsTrue(model.IsActive);
        Assert.AreEqual(3f, model.CurrentSpeedBonus, 0.0001f);

        model.Tick(2.1f);

        Assert.IsFalse(model.IsActive);
        Assert.AreEqual(0f, model.CurrentSpeedBonus, 0.0001f);
    }
}
#endif
