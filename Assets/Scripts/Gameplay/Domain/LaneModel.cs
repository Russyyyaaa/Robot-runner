using System;

public sealed class LaneModel
{
    private readonly int _laneCount;
    private int _currentLaneIndex;

    public LaneModel(int laneCount, int startLaneIndex)
    {
        if (laneCount <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(laneCount));
        }

        if (startLaneIndex < 0 || startLaneIndex >= laneCount)
        {
            throw new ArgumentOutOfRangeException(nameof(startLaneIndex));
        }

        _laneCount = laneCount;
        _currentLaneIndex = startLaneIndex;
    }

    public int LaneCount => _laneCount;
    public int CurrentLaneIndex => _currentLaneIndex;
    public bool CanMoveLeft => _currentLaneIndex > 0;
    public bool CanMoveRight => _currentLaneIndex < _laneCount - 1;

    public bool TryMoveLeft()
    {
        if (CanMoveLeft == false)
        {
            return false;
        }

        _currentLaneIndex--;
        return true;
    }

    public bool TryMoveRight()
    {
        if (CanMoveRight == false)
        {
            return false;
        }

        _currentLaneIndex++;
        return true;
    }

    public float GetLanePositionX(float laneOffset)
    {
        float centerIndex = (_laneCount - 1) * 0.5f;
        return (_currentLaneIndex - centerIndex) * laneOffset;
    }
}
