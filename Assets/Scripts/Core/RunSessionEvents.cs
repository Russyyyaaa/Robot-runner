using System;

public sealed class RunSessionEvents
{
    public event Action<GameState> StateChanged;
    public event Action<int> ScoreChanged;
    public event Action<float> BatteryChanged;
    public event Action<int> RunEnded;

    public void RaiseStateChanged(GameState gameState)
    {
        StateChanged?.Invoke(gameState);
    }

    public void RaiseScoreChanged(int score)
    {
        ScoreChanged?.Invoke(score);
    }

    public void RaiseBatteryChanged(float normalizedBattery)
    {
        BatteryChanged?.Invoke(normalizedBattery);
    }

    public void RaiseRunEnded(int finalScore)
    {
        RunEnded?.Invoke(finalScore);
    }
}
