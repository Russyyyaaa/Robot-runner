#if UNITY_INCLUDE_TESTS
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

public sealed class SectionProgressionServicePhase5Tests
{
    private readonly List<GameObject> _createdObjects = new List<GameObject>();
    private SectionDifficultyProfile _createdProfile;

    [TearDown]
    public void TearDown()
    {
        if (_createdProfile != null)
        {
            Object.DestroyImmediate(_createdProfile);
            _createdProfile = null;
        }

        for (int i = 0; i < _createdObjects.Count; i++)
        {
            GameObject createdObject = _createdObjects[i];
            if (createdObject != null)
            {
                Object.DestroyImmediate(createdObject);
            }
        }

        _createdObjects.Clear();
    }

    [Test]
    public void ResolveTierByScore_UsesMilestoneThresholds()
    {
        SectionProgressionService service = CreateService();

        Assert.AreEqual(SectionDifficultyTier.Low, service.ResolveTierByScore(149));
        Assert.AreEqual(SectionDifficultyTier.Middle, service.ResolveTierByScore(150));
        Assert.AreEqual(SectionDifficultyTier.Hard, service.ResolveTierByScore(400));
        Assert.AreEqual(SectionDifficultyTier.VeryHard, service.ResolveTierByScore(900));
    }

    [Test]
    public void ShouldSpawnBoosterSection_ReturnsTrueOnConfiguredInterval()
    {
        SectionProgressionService service = CreateService();

        Assert.IsFalse(service.ShouldSpawnBoosterSection(5));
        Assert.IsTrue(service.ShouldSpawnBoosterSection(6));
    }

    [Test]
    public void ResolveTierByScore_WhenThresholdMultiplierApplied_ShiftsDifficultyWindows()
    {
        SectionProgressionService hardService = CreateService(scoreThresholdMultiplier: 0.75f);
        SectionProgressionService easyService = CreateService(scoreThresholdMultiplier: 1.35f);

        Assert.AreEqual(SectionDifficultyTier.Middle, hardService.ResolveTierByScore(113));
        Assert.AreEqual(SectionDifficultyTier.Low, easyService.ResolveTierByScore(202));
        Assert.AreEqual(SectionDifficultyTier.Middle, easyService.ResolveTierByScore(203));
    }

    [Test]
    public void ShouldSpawnBoosterSection_WhenIntervalMultiplierApplied_UsesScaledInterval()
    {
        SectionProgressionService service = CreateService(boosterIntervalMultiplier: 0.5f);

        Assert.IsFalse(service.ShouldSpawnBoosterSection(2));
        Assert.IsTrue(service.ShouldSpawnBoosterSection(3));
    }

    private SectionProgressionService CreateService(float scoreThresholdMultiplier = 1f, float boosterIntervalMultiplier = 1f)
    {
        _createdProfile = ScriptableObject.CreateInstance<SectionDifficultyProfile>();

        List<RoadSectionDefinition> low = new List<RoadSectionDefinition> { CreateSection(true) };
        List<RoadSectionDefinition> middle = new List<RoadSectionDefinition> { CreateSection(true) };
        List<RoadSectionDefinition> hard = new List<RoadSectionDefinition> { CreateSection(false) };
        List<RoadSectionDefinition> veryHard = new List<RoadSectionDefinition> { CreateSection(false) };
        List<RoadSectionDefinition> booster = new List<RoadSectionDefinition> { CreateSection(true) };

        return new SectionProgressionService(
            _createdProfile,
            low,
            middle,
            hard,
            veryHard,
            booster,
            scoreThresholdMultiplier,
            boosterIntervalMultiplier);
    }

    private RoadSectionDefinition CreateSection(bool safe)
    {
        GameObject section = new GameObject("section");
        _createdObjects.Add(section);
        return new RoadSectionDefinition(section, safe);
    }
}
#endif
