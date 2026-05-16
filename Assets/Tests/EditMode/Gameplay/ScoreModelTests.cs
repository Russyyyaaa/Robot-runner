#if UNITY_INCLUDE_TESTS
using NUnit.Framework;

public sealed class ScoreModelTests
{
    [Test]
    public void Tick_WithSpeedAndMultiplier_AccumulatesScore()
    {
        ScoreModel model = new ScoreModel(1f);

        model.Tick(10f, 1f, 2f);

        Assert.AreEqual(20, model.CurrentScore);
    }
}
#endif
