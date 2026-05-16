using System;

public sealed class ScoreRuntime
{
    private readonly ScoreModel _scoreModel;

    public ScoreRuntime(GameBalanceConfig gameBalanceConfig)
    {
        if (gameBalanceConfig == null)
        {
            throw new ArgumentNullException(nameof(gameBalanceConfig));
        }

        _scoreModel = new ScoreModel(gameBalanceConfig.ScorePerDistanceUnit);
    }

    public int CurrentScore => _scoreModel.CurrentScore;

    public void Tick(float forwardSpeed, float deltaTime, float scoreMultiplier)
    {
        _scoreModel.Tick(forwardSpeed, deltaTime, scoreMultiplier);
    }

    public void Reset()
    {
        _scoreModel.Reset();
    }
}
