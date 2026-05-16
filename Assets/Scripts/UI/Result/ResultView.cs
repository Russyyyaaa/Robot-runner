using System;
using UnityEngine;
using UnityEngine.UI;

public sealed class ResultView : MonoBehaviour
{
    [SerializeField] private GameObject _root;
    [SerializeField] private Text _scoreText;
    [SerializeField] private Text _bestScoreText;
    [SerializeField] private Button _restartButton;
    [SerializeField] private Button _homeButton;

    public event Action RestartClicked;
    public event Action HomeClicked;

    private void OnEnable()
    {
        if (_restartButton != null)
        {
            _restartButton.onClick.AddListener(HandleRestartClicked);
        }

        if (_homeButton != null)
        {
            _homeButton.onClick.AddListener(HandleHomeClicked);
        }
    }

    private void OnDisable()
    {
        if (_restartButton != null)
        {
            _restartButton.onClick.RemoveListener(HandleRestartClicked);
        }

        if (_homeButton != null)
        {
            _homeButton.onClick.RemoveListener(HandleHomeClicked);
        }
    }

    public void SetVisible(bool visible)
    {
        if (_root != null)
        {
            _root.SetActive(visible);
        }
    }

    public void SetScore(int score)
    {
        if (_scoreText != null)
        {
            _scoreText.text = $"Score: {score}";
        }
    }

    public void SetBestScore(int bestScore)
    {
        if (_bestScoreText != null)
        {
            _bestScoreText.text = $"Best: {bestScore}";
        }
    }

    private void HandleRestartClicked()
    {
        RestartClicked?.Invoke();
    }

    private void HandleHomeClicked()
    {
        HomeClicked?.Invoke();
    }
}
