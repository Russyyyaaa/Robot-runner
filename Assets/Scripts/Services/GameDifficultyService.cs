using UnityEngine;

public static class GameDifficultyService
{
    private const string SelectedDifficultyKey = "robot_run.selected_difficulty";
    private static readonly string[] DifficultyLabels = { "Easy", "Normal", "Hard" };

    public static GameDifficultyLevel GetSelectedDifficulty()
    {
        int defaultValue = (int)GameDifficultyLevel.Normal;
        int savedValue = PlayerPrefs.GetInt(SelectedDifficultyKey, defaultValue);

        return Normalize((GameDifficultyLevel)savedValue);
    }

    public static void SetSelectedDifficulty(GameDifficultyLevel difficulty)
    {
        GameDifficultyLevel normalizedDifficulty = Normalize(difficulty);
        PlayerPrefs.SetInt(SelectedDifficultyKey, (int)normalizedDifficulty);
        PlayerPrefs.Save();
    }

    public static GameDifficultyLevel GetNextDifficulty(GameDifficultyLevel current)
    {
        GameDifficultyLevel normalizedCurrent = Normalize(current);
        return normalizedCurrent switch
        {
            GameDifficultyLevel.Easy => GameDifficultyLevel.Normal,
            GameDifficultyLevel.Normal => GameDifficultyLevel.Hard,
            _ => GameDifficultyLevel.Easy
        };
    }

    public static string GetDisplayName(GameDifficultyLevel difficulty)
    {
        GameDifficultyLevel normalizedDifficulty = Normalize(difficulty);
        return DifficultyLabels[(int)normalizedDifficulty];
    }

    public static DifficultyRuntimeModifiers GetRuntimeModifiers(GameDifficultyLevel difficulty)
    {
        GameDifficultyLevel normalizedDifficulty = Normalize(difficulty);
        return normalizedDifficulty switch
        {
            GameDifficultyLevel.Easy => new DifficultyRuntimeModifiers(
                baseSpeedMultiplier: 0.92f,
                batteryDrainMultiplier: 0.75f,
                batteryRestoreMultiplier: 1.15f,
                trapDamageMultiplier: 0.75f,
                boosterSpeedBonusMultiplier: 1.05f,
                boosterDurationMultiplier: 1.2f,
                sectionThresholdMultiplier: 1.35f,
                boosterIntervalMultiplier: 0.8f),

            GameDifficultyLevel.Hard => new DifficultyRuntimeModifiers(
                baseSpeedMultiplier: 1.15f,
                batteryDrainMultiplier: 1.25f,
                batteryRestoreMultiplier: 0.9f,
                trapDamageMultiplier: 1.2f,
                boosterSpeedBonusMultiplier: 0.95f,
                boosterDurationMultiplier: 0.9f,
                sectionThresholdMultiplier: 0.75f,
                boosterIntervalMultiplier: 1.25f),

            _ => new DifficultyRuntimeModifiers(
                baseSpeedMultiplier: 1f,
                batteryDrainMultiplier: 1f,
                batteryRestoreMultiplier: 1f,
                trapDamageMultiplier: 1f,
                boosterSpeedBonusMultiplier: 1f,
                boosterDurationMultiplier: 1f,
                sectionThresholdMultiplier: 1f,
                boosterIntervalMultiplier: 1f)
        };
    }

    private static GameDifficultyLevel Normalize(GameDifficultyLevel difficulty)
    {
        return difficulty switch
        {
            GameDifficultyLevel.Easy => GameDifficultyLevel.Easy,
            GameDifficultyLevel.Normal => GameDifficultyLevel.Normal,
            GameDifficultyLevel.Hard => GameDifficultyLevel.Hard,
            _ => GameDifficultyLevel.Normal
        };
    }
}
