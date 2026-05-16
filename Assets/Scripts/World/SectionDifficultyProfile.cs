using UnityEngine;

[CreateAssetMenu(fileName = "SectionDifficultyProfile", menuName = "RobotRun/World/Section Difficulty Profile")]
public sealed class SectionDifficultyProfile : ScriptableObject
{
    [SerializeField] private int _middleThresholdScore = 150;
    [SerializeField] private int _hardThresholdScore = 400;
    [SerializeField] private int _veryHardThresholdScore = 900;
    [SerializeField] private int _boosterIntervalSections = 6;

    public int MiddleThresholdScore => _middleThresholdScore;
    public int HardThresholdScore => _hardThresholdScore;
    public int VeryHardThresholdScore => _veryHardThresholdScore;
    public int BoosterIntervalSections => _boosterIntervalSections;
}
