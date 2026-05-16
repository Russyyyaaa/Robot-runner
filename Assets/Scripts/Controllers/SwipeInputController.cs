using System;
using UnityEngine;
using UnityEngine.InputSystem;

public sealed class SwipeInputController : MonoBehaviour
{
    [SerializeField] private float _minSwipeDistance = 40f;

    private Vector2 _startPosition;
    private bool _hasStartPosition;

    public event Action MoveLeftRequested;
    public event Action MoveRightRequested;

    private void Update()
    {
        Touchscreen touchScreen = Touchscreen.current;
        if (touchScreen == null)
        {
            return;
        }

        UnityEngine.InputSystem.Controls.TouchControl touch = touchScreen.primaryTouch;
        Vector2 touchPosition = touch.position.ReadValue();

        if (touch.press.wasPressedThisFrame)
        {
            _startPosition = touchPosition;
            _hasStartPosition = true;
            return;
        }

        if (_hasStartPosition == false)
        {
            return;
        }

        if (touch.press.wasReleasedThisFrame)
        {
            Vector2 delta = touchPosition - _startPosition;
            _hasStartPosition = false;

            if (Mathf.Abs(delta.x) < _minSwipeDistance)
            {
                return;
            }

            if (Mathf.Abs(delta.x) < Mathf.Abs(delta.y))
            {
                return;
            }

            if (delta.x > 0f)
            {
                MoveRightRequested?.Invoke();
                return;
            }

            MoveLeftRequested?.Invoke();
        }
    }
}
