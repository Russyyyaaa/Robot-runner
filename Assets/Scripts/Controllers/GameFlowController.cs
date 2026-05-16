using System;
using UnityEngine;
using UnityEngine.SceneManagement;

[DisallowMultipleComponent]
public sealed class GameFlowController : MonoBehaviour
{
    [SerializeField] private string _menuSceneName = "SampleScene";
    [SerializeField] private string _gameSceneName = "Game";
    [SerializeField] private RunSessionController _runSessionController;
    private static GameFlowController _instance;

    public void StartGameplayFromMenu(GameDifficultyLevel difficultyLevel)
    {
        GameDifficultyService.SetSelectedDifficulty(difficultyLevel);
        StartGameplayFromMenu();
    }

    public void StartGameplayFromMenu()
    {
        if (SceneManager.GetActiveScene().name != _gameSceneName)
        {
            SceneManager.LoadScene(_gameSceneName);
            return;
        }

        if (_runSessionController == null)
        {
            throw new InvalidOperationException($"{nameof(GameFlowController)} requires {nameof(_runSessionController)}.");
        }

        _runSessionController.StartRun();
    }

    public void RestartRun()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void GoHome()
    {
        if (SceneManager.GetActiveScene().name == _menuSceneName)
        {
            return;
        }

        SceneManager.LoadScene(_menuSceneName);
    }

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;

        if (_runSessionController == null)
        {
            _runSessionController = FindFirstObjectByType<RunSessionController>();
        }
    }
}
