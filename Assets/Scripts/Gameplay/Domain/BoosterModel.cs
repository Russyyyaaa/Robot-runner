using System;

public sealed class BoosterModel
{
    private float _remainingTime;
    private float _speedBonus;

    public bool IsActive => _remainingTime > 0f;
    public float CurrentSpeedBonus => IsActive ? _speedBonus : 0f;

    public void Activate(float speedBonus, float durationSeconds)
    {
        if (speedBonus <= 0f)
        {
            throw new ArgumentOutOfRangeException(nameof(speedBonus));
        }

        if (durationSeconds <= 0f)
        {
            throw new ArgumentOutOfRangeException(nameof(durationSeconds));
        }

        _speedBonus = speedBonus;
        _remainingTime = durationSeconds;
    }

    public void Tick(float deltaTime)
    {
        if (IsActive == false || deltaTime <= 0f)
        {
            return;
        }

        _remainingTime -= deltaTime;

        if (_remainingTime <= 0f)
        {
            _remainingTime = 0f;
            _speedBonus = 0f;
        }
    }

    public void Reset()
    {
        _remainingTime = 0f;
        _speedBonus = 0f;
    }
}
