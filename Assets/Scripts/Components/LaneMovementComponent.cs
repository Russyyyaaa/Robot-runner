using System;
using UnityEngine;

[Serializable]
public sealed class LaneMovementComponent
{
    [SerializeField] private int _laneCount = 3;
    [SerializeField] private int _startLaneIndex = 1;
    [SerializeField] private float _laneOffset = 1.7f;
    [SerializeField] private float _laneChangeDuration = 0.2f;

    private LaneModel _laneModel;
    private float _transitionProgress;
    private bool _isTransitionActive;
    private float _fromX;
    private float _toX;

    public int CurrentLaneIndex => _laneModel != null ? _laneModel.CurrentLaneIndex : _startLaneIndex;
    public bool IsTransitionActive => _isTransitionActive;

    public void ApplyConfig(GameBalanceConfig gameBalanceConfig)
    {
        if (gameBalanceConfig == null)
        {
            throw new ArgumentNullException(nameof(gameBalanceConfig));
        }

        _laneCount = gameBalanceConfig.LaneCount;
        _startLaneIndex = gameBalanceConfig.StartLaneIndex;
        _laneOffset = gameBalanceConfig.LaneOffset;
        _laneChangeDuration = gameBalanceConfig.LaneChangeDuration;
    }

    public void Initialize(float currentX)
    {
        _laneModel = new LaneModel(_laneCount, _startLaneIndex);
        _fromX = currentX;
        _toX = _laneModel.GetLanePositionX(_laneOffset);
        _transitionProgress = 1f;
        _isTransitionActive = false;
    }

    public bool TryMoveLeft(float currentX)
    {
        if (_isTransitionActive)
        {
            return false;
        }

        if (_laneModel.TryMoveLeft() == false)
        {
            return false;
        }

        BeginTransition(currentX);
        return true;
    }

    public bool TryMoveRight(float currentX)
    {
        if (_isTransitionActive)
        {
            return false;
        }

        if (_laneModel.TryMoveRight() == false)
        {
            return false;
        }

        BeginTransition(currentX);
        return true;
    }

    public float Tick(float currentX, float deltaTime)
    {
        if (_isTransitionActive == false)
        {
            return _toX;
        }

        if (deltaTime <= 0f)
        {
            return currentX;
        }

        float duration = _laneChangeDuration <= 0f ? 0.01f : _laneChangeDuration;
        _transitionProgress += deltaTime / duration;

        if (_transitionProgress >= 1f)
        {
            _transitionProgress = 1f;
            _isTransitionActive = false;
        }

        return Mathf.Lerp(_fromX, _toX, _transitionProgress);
    }

    private void BeginTransition(float currentX)
    {
        _fromX = currentX;
        _toX = _laneModel.GetLanePositionX(_laneOffset);
        _transitionProgress = 0f;
        _isTransitionActive = true;
    }
}
