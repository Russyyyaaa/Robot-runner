#if UNITY_INCLUDE_TESTS
using NUnit.Framework;

public sealed class ScoreModelPhase1Tests
{
    [Test]
    public void Tick_WhenMovingForward_IncreasesScore()
    {
        ScoreModel scoreModel = new ScoreModel(1f);

        scoreModel.Tick(10f, 1f);

        Assert.AreEqual(10, scoreModel.CurrentScore);
    }

    [Test]
    public void Reset_ClearsScore()
    {
        ScoreModel scoreModel = new ScoreModel(1f);
        scoreModel.Tick(5f, 1f);

        scoreModel.Reset();

        Assert.AreEqual(0, scoreModel.CurrentScore);
    }
}
#endif
