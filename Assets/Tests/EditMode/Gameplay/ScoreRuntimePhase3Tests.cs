#if UNITY_INCLUDE_TESTS
using NUnit.Framework;
using UnityEngine;

public sealed class ScoreRuntimePhase3Tests
{
    [Test]
    public void Tick_WhenMovingForward_IncreasesScore()
    {
        GameBalanceConfig config = RuntimeConfigTestFactory.CreateDefault();
        ScoreRuntime scoreRuntime = new ScoreRuntime(config);

        scoreRuntime.Tick(10f, 1f, 1f);

        Assert.AreEqual(10, scoreRuntime.CurrentScore);
        Object.DestroyImmediate(config);
    }

    [Test]
    public void Reset_ClearsAccumulatedScore()
    {
        GameBalanceConfig config = RuntimeConfigTestFactory.CreateDefault();
        ScoreRuntime scoreRuntime = new ScoreRuntime(config);
        scoreRuntime.Tick(10f, 1f, 1f);

        scoreRuntime.Reset();

        Assert.AreEqual(0, scoreRuntime.CurrentScore);
        Object.DestroyImmediate(config);
    }
}
#endif
