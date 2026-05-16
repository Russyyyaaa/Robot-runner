using System;

public sealed class BatteryModel
{
    private readonly float _maxCharge;
    private readonly float _drainPerSecond;
    private readonly float _startCharge;
    private float _currentCharge;

    public BatteryModel(float maxCharge, float startCharge, float drainPerSecond)
    {
        if (maxCharge <= 0f)
        {
            throw new ArgumentOutOfRangeException(nameof(maxCharge));
        }

        if (drainPerSecond < 0f)
        {
            throw new ArgumentOutOfRangeException(nameof(drainPerSecond));
        }

        _maxCharge = maxCharge;
        _drainPerSecond = drainPerSecond;
        _startCharge = Clamp(startCharge, 0f, _maxCharge);
        _currentCharge = _startCharge;
    }

    public float CurrentCharge => _currentCharge;
    public float MaxCharge => _maxCharge;
    public bool IsEmpty => _currentCharge <= 0f;
    public float NormalizedCharge => _maxCharge > 0f ? _currentCharge / _maxCharge : 0f;

    public void Tick(float deltaTime)
    {
        if (deltaTime <= 0f)
        {
            return;
        }

        _currentCharge = Clamp(_currentCharge - _drainPerSecond * deltaTime, 0f, _maxCharge);
    }

    public void Restore(float amount)
    {
        if (amount <= 0f)
        {
            return;
        }

        _currentCharge = Clamp(_currentCharge + amount, 0f, _maxCharge);
    }

    public void ApplyDamage(float amount)
    {
        if (amount <= 0f)
        {
            return;
        }

        _currentCharge = Clamp(_currentCharge - amount, 0f, _maxCharge);
    }

    public void Reset()
    {
        _currentCharge = _startCharge;
    }

    private static float Clamp(float value, float min, float max)
    {
        if (value < min)
        {
            return min;
        }

        if (value > max)
        {
            return max;
        }

        return value;
    }
}
