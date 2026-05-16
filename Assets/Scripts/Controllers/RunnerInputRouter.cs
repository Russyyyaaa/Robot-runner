using System;
using UnityEngine;

public sealed class RunnerInputRouter : MonoBehaviour
{
    [SerializeField] private KeyboardRunnerInputController _keyboardRunnerInputController;
    [SerializeField] private SwipeInputController _swipeInputController;

    public event Action MoveLeftRequested;
    public event Action MoveRightRequested;

    private void OnEnable()
    {
        if (_keyboardRunnerInputController != null)
        {
            _keyboardRunnerInputController.MoveLeftRequested += HandleMoveLeftRequested;
            _keyboardRunnerInputController.MoveRightRequested += HandleMoveRightRequested;
        }

        if (_swipeInputController != null)
        {
            _swipeInputController.MoveLeftRequested += HandleMoveLeftRequested;
            _swipeInputController.MoveRightRequested += HandleMoveRightRequested;
        }
    }

    private void OnDisable()
    {
        if (_keyboardRunnerInputController != null)
        {
            _keyboardRunnerInputController.MoveLeftRequested -= HandleMoveLeftRequested;
            _keyboardRunnerInputController.MoveRightRequested -= HandleMoveRightRequested;
        }

        if (_swipeInputController != null)
        {
            _swipeInputController.MoveLeftRequested -= HandleMoveLeftRequested;
            _swipeInputController.MoveRightRequested -= HandleMoveRightRequested;
        }
    }

    private void HandleMoveLeftRequested()
    {
        MoveLeftRequested?.Invoke();
    }

    private void HandleMoveRightRequested()
    {
        MoveRightRequested?.Invoke();
    }
}
