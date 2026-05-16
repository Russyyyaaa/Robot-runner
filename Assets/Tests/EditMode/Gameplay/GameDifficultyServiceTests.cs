#if UNITY_INCLUDE_TESTS
using NUnit.Framework;

public sealed class GameDifficultyServiceTests
{
    [SetUp]
    public void SetUp()
    {
        GameDifficultyService.SetSelectedDifficulty(GameDifficultyLevel.Normal);
    }

    [TearDown]
    public void TearDown()
    {
        GameDifficultyService.SetSelectedDifficulty(GameDifficultyLevel.Normal);
    }

    [Test]
    public void SetSelectedDifficulty_WhenSaved_ReturnsSameValue()
    {
        GameDifficultyService.SetSelectedDifficulty(GameDifficultyLevel.Hard);

        Assert.AreEqual(GameDifficultyLevel.Hard, GameDifficultyService.GetSelectedDifficulty());
    }

    [Test]
    public void GetNextDifficulty_CyclesInExpectedOrder()
    {
        Assert.AreEqual(GameDifficultyLevel.Normal, GameDifficultyService.GetNextDifficulty(GameDifficultyLevel.Easy));
        Assert.AreEqual(GameDifficultyLevel.Hard, GameDifficultyService.GetNextDifficulty(GameDifficultyLevel.Normal));
        Assert.AreEqual(GameDifficultyLevel.Easy, GameDifficultyService.GetNextDifficulty(GameDifficultyLevel.Hard));
    }

    [Test]
    public void GetRuntimeModifiers_ForHard_HasIncreasedChallenge()
    {
        DifficultyRuntimeModifiers modifiers = GameDifficultyService.GetRuntimeModifiers(GameDifficultyLevel.Hard);

        Assert.Greater(modifiers.BaseSpeedMultiplier, 1f);
        Assert.Greater(modifiers.BatteryDrainMultiplier, 1f);
        Assert.Greater(modifiers.TrapDamageMultiplier, 1f);
        Assert.Less(modifiers.SectionThresholdMultiplier, 1f);
    }
}
#endif
