using System;

public sealed class ScoreModel
{
    private readonly float _scorePerDistanceUnit;
    private float _score;

    public ScoreModel(float scorePerDistanceUnit)
    {
        if (scorePerDistanceUnit <= 0f)
        {
            throw new ArgumentOutOfRangeException(nameof(scorePerDistanceUnit));
        }

        _scorePerDistanceUnit = scorePerDistanceUnit;
    }

    public int CurrentScore => (int)_score;
    public float RawScore => _score;

    public void Tick(float forwardSpeed, float deltaTime, float scoreMultiplier = 1f)
    {
        if (forwardSpeed <= 0f || deltaTime <= 0f || scoreMultiplier <= 0f)
        {
            return;
        }

        float distanceDelta = forwardSpeed * deltaTime;
        _score += distanceDelta * _scorePerDistanceUnit * scoreMultiplier;
    }

    public void Reset()
    {
        _score = 0f;
    }
}
