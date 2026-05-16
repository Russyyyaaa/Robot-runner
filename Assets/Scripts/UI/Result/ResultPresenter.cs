using UnityEngine;

public sealed class ResultPresenter : MonoBehaviour
{
    [SerializeField] private ResultView _view;
    [SerializeField] private RunSessionController _runSessionController;
    [SerializeField] private GameFlowController _gameFlowController;

    private LocalBestScoreService _bestScoreService;

    private void Awake()
    {
        _bestScoreService = new LocalBestScoreService();
    }

    private void OnEnable()
    {
        ResolveDependencies();

        if (_view == null || _runSessionController == null || _gameFlowController == null)
        {
            return;
        }

        _view.RestartClicked += HandleRestartClicked;
        _view.HomeClicked += HandleHomeClicked;
        _runSessionController.RunEnded += HandleRunEnded;
        _runSessionController.StateChanged += HandleStateChanged;

        _view.SetVisible(_runSessionController.CurrentState == GameState.GameOver);
    }

    private void OnDisable()
    {
        if (_view == null)
        {
            return;
        }

        _view.RestartClicked -= HandleRestartClicked;
        _view.HomeClicked -= HandleHomeClicked;

        if (_runSessionController != null)
        {
            _runSessionController.RunEnded -= HandleRunEnded;
            _runSessionController.StateChanged -= HandleStateChanged;
        }
    }

    private void HandleRunEnded(int finalScore)
    {
        int bestScore = _bestScoreService.SaveIfBest(finalScore);
        _view.SetScore(finalScore);
        _view.SetBestScore(bestScore);
        _view.SetVisible(true);
    }

    private void HandleRestartClicked()
    {
        _gameFlowController.RestartRun();
    }

    private void HandleHomeClicked()
    {
        _gameFlowController.GoHome();
    }

    private void HandleStateChanged(GameState gameState)
    {
        _view.SetVisible(gameState == GameState.GameOver);
    }

    private void ResolveDependencies()
    {
        if (_view == null)
        {
            _view = GetComponent<ResultView>();
        }

        if (_runSessionController == null)
        {
            _runSessionController = FindFirstObjectByType<RunSessionController>();
        }

        if (_gameFlowController == null)
        {
            _gameFlowController = FindFirstObjectByType<GameFlowController>();
        }
    }
}
