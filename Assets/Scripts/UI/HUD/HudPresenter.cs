using UnityEngine;

public sealed class HudPresenter : MonoBehaviour
{
    [SerializeField] private HudView _view;
    [SerializeField] private RunSessionController _runSessionController;

    private LocalBestScoreService _bestScoreService;
    private int _bestScore;

    private void Awake()
    {
        _bestScoreService = new LocalBestScoreService();
    }

    private void OnEnable()
    {
        if (_view == null)
        {
            _view = GetComponent<HudView>();
        }

        if (_view == null)
        {
            _view = FindFirstObjectByType<HudView>(FindObjectsInactive.Include);
        }

        if (_runSessionController == null)
        {
            _runSessionController = FindFirstObjectByType<RunSessionController>();
        }

        if (_view == null || _runSessionController == null)
        {
            return;
        }

        _runSessionController.ScoreChanged += HandleScoreChanged;
        _runSessionController.BatteryChanged += HandleBatteryChanged;
        _runSessionController.StateChanged += HandleStateChanged;

        _bestScore = _bestScoreService.GetBestScore();
        _view.SetBestScore(_bestScore);
        _view.SetVisible(_runSessionController.CurrentState == GameState.Running);
    }

    private void OnDisable()
    {
        if (_runSessionController == null)
        {
            return;
        }

        _runSessionController.ScoreChanged -= HandleScoreChanged;
        _runSessionController.BatteryChanged -= HandleBatteryChanged;
        _runSessionController.StateChanged -= HandleStateChanged;
    }

    private void HandleScoreChanged(int score)
    {
        _view.SetScore(score);

        if (score > _bestScore)
        {
            _bestScore = score;
            _view.SetBestScore(_bestScore);
        }
    }

    private void HandleBatteryChanged(float normalizedBattery)
    {
        _view.SetBatteryNormalized(normalizedBattery);
    }

    private void HandleStateChanged(GameState gameState)
    {
        _view.SetVisible(gameState == GameState.Running);
    }
}
