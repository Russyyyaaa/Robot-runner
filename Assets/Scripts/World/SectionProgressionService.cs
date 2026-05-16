using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct RoadSectionDefinition
{
    [SerializeField] private GameObject _prefab;
    [SerializeField] private bool _isSafeByDesign;

    public RoadSectionDefinition(GameObject prefab, bool isSafeByDesign)
    {
        _prefab = prefab;
        _isSafeByDesign = isSafeByDesign;
    }

    public GameObject Prefab => _prefab;
    public bool IsSafeByDesign => _isSafeByDesign;
}

[Serializable]
public sealed class RoadSectionPool
{
    [SerializeField] private List<RoadSectionDefinition> _sections = new List<RoadSectionDefinition>();

    public IReadOnlyList<RoadSectionDefinition> Sections => _sections;
}

public enum SectionDifficultyTier
{
    Low = 0,
    Middle = 1,
    Hard = 2,
    VeryHard = 3
}

public sealed class SectionProgressionService
{
    private readonly IReadOnlyList<RoadSectionDefinition> _lowSections;
    private readonly IReadOnlyList<RoadSectionDefinition> _middleSections;
    private readonly IReadOnlyList<RoadSectionDefinition> _hardSections;
    private readonly IReadOnlyList<RoadSectionDefinition> _veryHardSections;
    private readonly IReadOnlyList<RoadSectionDefinition> _boosterSections;
    private readonly IReadOnlyList<RoadSectionDefinition> _lowAndMiddleSections;
    private readonly IReadOnlyList<RoadSectionDefinition> _middleAndHardSections;
    private readonly IReadOnlyList<RoadSectionDefinition> _hardAndVeryHardSections;
    private readonly int _middleThresholdScore;
    private readonly int _hardThresholdScore;
    private readonly int _veryHardThresholdScore;
    private readonly int _boosterIntervalSections;

    public SectionProgressionService(
        SectionDifficultyProfile profile,
        IReadOnlyList<RoadSectionDefinition> lowSections,
        IReadOnlyList<RoadSectionDefinition> middleSections,
        IReadOnlyList<RoadSectionDefinition> hardSections,
        IReadOnlyList<RoadSectionDefinition> veryHardSections,
        IReadOnlyList<RoadSectionDefinition> boosterSections,
        float scoreThresholdMultiplier = 1f,
        float boosterIntervalMultiplier = 1f)
    {
        if (profile == null)
        {
            throw new ArgumentNullException(nameof(profile));
        }

        _lowSections = lowSections ?? throw new ArgumentNullException(nameof(lowSections));
        _middleSections = middleSections ?? throw new ArgumentNullException(nameof(middleSections));
        _hardSections = hardSections ?? throw new ArgumentNullException(nameof(hardSections));
        _veryHardSections = veryHardSections ?? throw new ArgumentNullException(nameof(veryHardSections));
        _boosterSections = boosterSections ?? throw new ArgumentNullException(nameof(boosterSections));
        _lowAndMiddleSections = Combine(_lowSections, _middleSections);
        _middleAndHardSections = Combine(_middleSections, _hardSections);
        _hardAndVeryHardSections = Combine(_hardSections, _veryHardSections);

        float resolvedThresholdMultiplier = scoreThresholdMultiplier <= 0f ? 1f : scoreThresholdMultiplier;
        float resolvedBoosterIntervalMultiplier = boosterIntervalMultiplier <= 0f ? 1f : boosterIntervalMultiplier;

        _middleThresholdScore = Mathf.Max(1, Mathf.RoundToInt(profile.MiddleThresholdScore * resolvedThresholdMultiplier));
        _hardThresholdScore = Mathf.Max(_middleThresholdScore + 1, Mathf.RoundToInt(profile.HardThresholdScore * resolvedThresholdMultiplier));
        _veryHardThresholdScore = Mathf.Max(_hardThresholdScore + 1, Mathf.RoundToInt(profile.VeryHardThresholdScore * resolvedThresholdMultiplier));
        _boosterIntervalSections = Mathf.Max(1, Mathf.RoundToInt(profile.BoosterIntervalSections * resolvedBoosterIntervalMultiplier));
    }

    public SectionDifficultyTier ResolveTierByScore(int score)
    {
        if (score >= _veryHardThresholdScore)
        {
            return SectionDifficultyTier.VeryHard;
        }

        if (score >= _hardThresholdScore)
        {
            return SectionDifficultyTier.Hard;
        }

        if (score >= _middleThresholdScore)
        {
            return SectionDifficultyTier.Middle;
        }

        return SectionDifficultyTier.Low;
    }

    public bool ShouldSpawnBoosterSection(int spawnedSectionsCount)
    {
        return spawnedSectionsCount > 0 && spawnedSectionsCount % _boosterIntervalSections == 0;
    }

    public RoadSectionDefinition PickBoosterSection(System.Random random)
    {
        return PickFrom(_boosterSections, random);
    }

    public RoadSectionDefinition PickForScore(int score, System.Random random)
    {
        SectionDifficultyTier tier = ResolveTierByScore(score);
        return PickForTier(tier, random);
    }

    public RoadSectionDefinition PickForTier(SectionDifficultyTier tier, System.Random random)
    {
        return tier switch
        {
            SectionDifficultyTier.Low => PickFrom(_lowSections, random),
            SectionDifficultyTier.Middle => PickFrom(_lowAndMiddleSections, random),
            SectionDifficultyTier.Hard => PickFrom(_middleAndHardSections, random),
            SectionDifficultyTier.VeryHard => PickFrom(_hardAndVeryHardSections, random),
            _ => throw new ArgumentOutOfRangeException(nameof(tier), tier, null)
        };
    }

    private static RoadSectionDefinition PickFrom(IReadOnlyList<RoadSectionDefinition> sections, System.Random random)
    {
        if (sections == null || sections.Count == 0)
        {
            throw new InvalidOperationException("Road section pool is empty.");
        }

        int index = random != null ? random.Next(0, sections.Count) : UnityEngine.Random.Range(0, sections.Count);
        return sections[index];
    }

    private static IReadOnlyList<RoadSectionDefinition> Combine(
        IReadOnlyList<RoadSectionDefinition> first,
        IReadOnlyList<RoadSectionDefinition> second)
    {
        List<RoadSectionDefinition> combined = new List<RoadSectionDefinition>(first.Count + second.Count);
        combined.AddRange(first);
        combined.AddRange(second);
        return combined;
    }
}
