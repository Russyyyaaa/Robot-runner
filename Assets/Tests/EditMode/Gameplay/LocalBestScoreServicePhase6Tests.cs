#if UNITY_INCLUDE_TESTS
using NUnit.Framework;
using UnityEngine;

public sealed class LocalBestScoreServicePhase6Tests
{
    [SetUp]
    public void SetUp()
    {
        PlayerPrefs.DeleteKey("robot_run_best_score");
    }

    [TearDown]
    public void TearDown()
    {
        PlayerPrefs.DeleteKey("robot_run_best_score");
    }

    [Test]
    public void SaveIfBest_WhenScoreIsHigher_UpdatesBestScore()
    {
        LocalBestScoreService service = new LocalBestScoreService();

        int saved = service.SaveIfBest(120);

        Assert.AreEqual(120, saved);
        Assert.AreEqual(120, service.GetBestScore());
    }

    [Test]
    public void SaveIfBest_WhenScoreIsLower_KeepsPreviousBestScore()
    {
        LocalBestScoreService service = new LocalBestScoreService();
        service.SaveIfBest(200);

        int saved = service.SaveIfBest(100);

        Assert.AreEqual(200, saved);
        Assert.AreEqual(200, service.GetBestScore());
    }
}
#endif
