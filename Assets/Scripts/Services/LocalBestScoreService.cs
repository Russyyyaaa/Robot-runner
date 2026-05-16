using UnityEngine;

public sealed class LocalBestScoreService
{
    private const string BestScoreKey = "robot_run_best_score";

    public int GetBestScore()
    {
        return PlayerPrefs.GetInt(BestScoreKey, 0);
    }

    public int SaveIfBest(int score)
    {
        int bestScore = GetBestScore();
        if (score <= bestScore)
        {
            return bestScore;
        }

        PlayerPrefs.SetInt(BestScoreKey, score);
        PlayerPrefs.Save();
        return score;
    }
}
