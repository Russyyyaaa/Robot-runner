using System;
using UnityEngine;

public sealed class Player : MonoBehaviour
{
    [SerializeField] private Transform _movableTransform;
    [SerializeField] private LaneMovementComponent _laneMovementComponent;
    [SerializeField] private float _baseForwardSpeed = 7f;
    [SerializeField] private Animator _animator;
    [SerializeField] private float _flightHeightOffset = 0.9f;
    [SerializeField] private float _flightHeightLerpSpeed = 8f;
    [SerializeField] private string _startFlyTriggerName = "StartFly";
    [SerializeField] private string _stopFlyTriggerName = "StopFly";

    private bool _isInitialized;
    private bool _isBoosterFlightActive;
    private bool _animatorConfigured;
    private float _runtimeSpeedBonus;
    private float _baseSpeedMultiplier = 1f;
    private float _groundY;
    private int _startFlyTriggerHash;
    private int _stopFlyTriggerHash;

    public float ForwardSpeed => (_baseForwardSpeed * _baseSpeedMultiplier) + _runtimeSpeedBonus;

    public void ApplyConfig(GameBalanceConfig gameBalanceConfig)
    {
        if (gameBalanceConfig == null)
        {
            throw new ArgumentNullException(nameof(gameBalanceConfig));
        }

        if (_laneMovementComponent == null)
        {
            throw new InvalidOperationException($"{nameof(Player)} requires {nameof(_laneMovementComponent)}.");
        }

        _baseForwardSpeed = gameBalanceConfig.BaseForwardSpeed;
        _laneMovementComponent.ApplyConfig(gameBalanceConfig);
    }

    public void Initialize()
    {
        if (_movableTransform == null)
        {
            throw new InvalidOperationException($"{nameof(Player)} requires {nameof(_movableTransform)}.");
        }

        if (_laneMovementComponent == null)
        {
            throw new InvalidOperationException($"{nameof(Player)} requires {nameof(_laneMovementComponent)}.");
        }

        _laneMovementComponent.Initialize(_movableTransform.position.x);
        _groundY = _movableTransform.position.y;
        ConfigureAnimatorIfNeeded();
        _isInitialized = true;
    }

    public void Tick(float deltaTime)
    {
        EnsureInitialized();

        if (deltaTime <= 0f)
        {
            return;
        }

        Vector3 currentPosition = _movableTransform.position;
        float xPosition = _laneMovementComponent.Tick(currentPosition.x, deltaTime);
        float yPosition = ResolveYPosition(currentPosition.y, deltaTime);
        float zPosition = currentPosition.z + ForwardSpeed * deltaTime;
        _movableTransform.position = new Vector3(xPosition, yPosition, zPosition);
    }

    public bool MoveLeft()
    {
        EnsureInitialized();
        return _laneMovementComponent.TryMoveLeft(_movableTransform.position.x);
    }

    public bool MoveRight()
    {
        EnsureInitialized();
        return _laneMovementComponent.TryMoveRight(_movableTransform.position.x);
    }

    public void SetRuntimeSpeedBonus(float speedBonus)
    {
        _runtimeSpeedBonus = speedBonus <= 0f ? 0f : speedBonus;
    }

    public void SetBaseSpeedMultiplier(float speedMultiplier)
    {
        _baseSpeedMultiplier = speedMultiplier <= 0f ? 1f : speedMultiplier;
    }

    public void SetBoosterFlightActive(bool isActive)
    {
        EnsureInitialized();

        if (_isBoosterFlightActive == isActive)
        {
            return;
        }

        _isBoosterFlightActive = isActive;
        TriggerFlightAnimation(_isBoosterFlightActive);
    }

    private void EnsureInitialized()
    {
        if (_isInitialized)
        {
            return;
        }

        Initialize();
    }

    private float ResolveYPosition(float currentY, float deltaTime)
    {
        float targetY = _groundY;
        if (_isBoosterFlightActive)
        {
            targetY += Mathf.Max(0f, _flightHeightOffset);
        }

        float lerpSpeed = Mathf.Max(0.01f, _flightHeightLerpSpeed);
        return Mathf.Lerp(currentY, targetY, deltaTime * lerpSpeed);
    }

    private void ConfigureAnimatorIfNeeded()
    {
        if (_animatorConfigured)
        {
            return;
        }

        if (_animator == null)
        {
            _animator = GetComponentInChildren<Animator>();
        }

        if (_animator == null)
        {
            _animatorConfigured = true;
            return;
        }

        string startFlyTrigger = string.IsNullOrWhiteSpace(_startFlyTriggerName) ? "StartFly" : _startFlyTriggerName;
        string stopFlyTrigger = string.IsNullOrWhiteSpace(_stopFlyTriggerName) ? "StopFly" : _stopFlyTriggerName;
        _startFlyTriggerHash = Animator.StringToHash(startFlyTrigger);
        _stopFlyTriggerHash = Animator.StringToHash(stopFlyTrigger);
        _animatorConfigured = true;
    }

    private void TriggerFlightAnimation(bool isActive)
    {
        ConfigureAnimatorIfNeeded();
        if (_animator == null)
        {
            return;
        }

        if (isActive)
        {
            _animator.ResetTrigger(_stopFlyTriggerHash);
            _animator.SetTrigger(_startFlyTriggerHash);
            return;
        }

        _animator.ResetTrigger(_startFlyTriggerHash);
        _animator.SetTrigger(_stopFlyTriggerHash);
    }
}
