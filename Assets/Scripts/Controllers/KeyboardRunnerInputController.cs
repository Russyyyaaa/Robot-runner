using System;
using UnityEngine;
using UnityEngine.InputSystem;

public sealed class KeyboardRunnerInputController : MonoBehaviour
{
    public event Action MoveLeftRequested;
    public event Action MoveRightRequested;

    private void Update()
    {
        Keyboard keyboard = Keyboard.current;
        if (keyboard == null)
        {
            return;
        }

        if (keyboard.aKey.wasPressedThisFrame || keyboard.leftArrowKey.wasPressedThisFrame)
        {
            MoveLeftRequested?.Invoke();
        }

        if (keyboard.dKey.wasPressedThisFrame || keyboard.rightArrowKey.wasPressedThisFrame)
        {
            MoveRightRequested?.Invoke();
        }
    }
}
