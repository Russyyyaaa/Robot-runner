using UnityEngine;

public sealed class MainMenuPresenter : MonoBehaviour
{
    private static readonly string[] DifficultyLabels =
    {
        GameDifficultyService.GetDisplayName(GameDifficultyLevel.Easy),
        GameDifficultyService.GetDisplayName(GameDifficultyLevel.Normal),
        GameDifficultyService.GetDisplayName(GameDifficultyLevel.Hard)
    };

    [SerializeField] private MainMenuView _view;
    [SerializeField] private RunSessionController _runSessionController;
    [SerializeField] private GameFlowController _gameFlowController;

    private LocalBestScoreService _bestScoreService;
    private GameDifficultyLevel _selectedDifficulty = GameDifficultyLevel.Normal;

    private void Awake()
    {
        _bestScoreService = new LocalBestScoreService();
    }

    private void OnEnable()
    {
        ResolveDependencies();

        if (_view == null || _gameFlowController == null)
        {
            return;
        }

        _view.PlayClicked += HandlePlayClicked;
        _view.SettingsClicked += HandleSettingsClicked;
        _view.DifficultyClicked += HandleDifficultyClicked;
        _view.DifficultyDropdownChanged += HandleDifficultyDropdownChanged;

        _view.SetBestScore(_bestScoreService.GetBestScore());
        _selectedDifficulty = GameDifficultyService.GetSelectedDifficulty();
        _view.SetDifficultyDropdownOptions(DifficultyLabels);
        ApplyDifficultyToView();

        if (_runSessionController != null)
        {
            _runSessionController.StateChanged += HandleStateChanged;
            _view.SetVisible(_runSessionController.CurrentState == GameState.Menu);
            return;
        }

        _view.SetVisible(true);
    }

    private void OnDisable()
    {
        if (_view == null)
        {
            return;
        }

        _view.PlayClicked -= HandlePlayClicked;
        _view.SettingsClicked -= HandleSettingsClicked;
        _view.DifficultyClicked -= HandleDifficultyClicked;
        _view.DifficultyDropdownChanged -= HandleDifficultyDropdownChanged;

        if (_runSessionController != null)
        {
            _runSessionController.StateChanged -= HandleStateChanged;
        }
    }

    private void HandlePlayClicked()
    {
        GameDifficultyService.SetSelectedDifficulty(_selectedDifficulty);
        _gameFlowController.StartGameplayFromMenu(_selectedDifficulty);
    }

    private void HandleSettingsClicked()
    {
        HandleDifficultyClicked();
    }

    private void HandleDifficultyClicked()
    {
        _selectedDifficulty = GameDifficultyService.GetNextDifficulty(_selectedDifficulty);
        GameDifficultyService.SetSelectedDifficulty(_selectedDifficulty);
        ApplyDifficultyToView();
    }

    private void HandleDifficultyDropdownChanged(int selectedIndex)
    {
        _selectedDifficulty = NormalizeDifficultyIndex(selectedIndex);
        GameDifficultyService.SetSelectedDifficulty(_selectedDifficulty);
        ApplyDifficultyToView();
    }

    private void HandleStateChanged(GameState gameState)
    {
        _view.SetVisible(gameState == GameState.Menu);
    }

    private void ResolveDependencies()
    {
        if (_view == null)
        {
            _view = GetComponent<MainMenuView>();
        }

        if (_gameFlowController == null)
        {
            _gameFlowController = FindFirstObjectByType<GameFlowController>();
        }

        if (_runSessionController == null)
        {
            _runSessionController = FindFirstObjectByType<RunSessionController>();
        }
    }

    public void Configure(MainMenuView view, RunSessionController runSessionController, GameFlowController gameFlowController)
    {
        _view = view;
        _runSessionController = runSessionController;
        _gameFlowController = gameFlowController;
    }

    private void ApplyDifficultyToView()
    {
        _view.SetDifficultyLabel(GameDifficultyService.GetDisplayName(_selectedDifficulty));
        _view.SetDifficultyDropdownValue((int)_selectedDifficulty);
    }

    private static GameDifficultyLevel NormalizeDifficultyIndex(int selectedIndex)
    {
        return selectedIndex switch
        {
            0 => GameDifficultyLevel.Easy,
            1 => GameDifficultyLevel.Normal,
            2 => GameDifficultyLevel.Hard,
            _ => GameDifficultyLevel.Normal
        };
    }
}
