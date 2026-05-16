using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class MainMenuView : MonoBehaviour
{
    [SerializeField] private GameObject _root;
    [SerializeField] private Button _playButton;
    [SerializeField] private Button _settingsButton;
    [SerializeField] private Button _difficultyButton;
    [SerializeField] private Text _bestScoreText;
    [SerializeField] private Text _difficultyText;
    [SerializeField] private TMP_Text _bestScoreTextTmp;
    [SerializeField] private TMP_Text _difficultyTextTmp;
    [SerializeField] private TMP_Dropdown _difficultyDropdown;

    public event Action PlayClicked;
    public event Action SettingsClicked;
    public event Action DifficultyClicked;
    public event Action<int> DifficultyDropdownChanged;

    public void Configure(
        GameObject root,
        Button playButton,
        Button settingsButton,
        Button difficultyButton,
        Text bestScoreText,
        Text difficultyText)
    {
        _root = root;
        _playButton = playButton;
        _settingsButton = settingsButton;
        _difficultyButton = difficultyButton;
        _bestScoreText = bestScoreText;
        _difficultyText = difficultyText;
    }

    private void OnEnable()
    {
        if (_playButton != null)
        {
            _playButton.onClick.AddListener(HandlePlayClicked);
        }

        if (_settingsButton != null)
        {
            _settingsButton.onClick.AddListener(HandleSettingsClicked);
        }

        if (_difficultyButton != null)
        {
            _difficultyButton.onClick.AddListener(HandleDifficultyClicked);
        }

        if (_difficultyDropdown != null)
        {
            _difficultyDropdown.onValueChanged.AddListener(HandleDifficultyDropdownChanged);
        }
    }

    private void OnDisable()
    {
        if (_playButton != null)
        {
            _playButton.onClick.RemoveListener(HandlePlayClicked);
        }

        if (_settingsButton != null)
        {
            _settingsButton.onClick.RemoveListener(HandleSettingsClicked);
        }

        if (_difficultyButton != null)
        {
            _difficultyButton.onClick.RemoveListener(HandleDifficultyClicked);
        }

        if (_difficultyDropdown != null)
        {
            _difficultyDropdown.onValueChanged.RemoveListener(HandleDifficultyDropdownChanged);
        }
    }

    public void SetVisible(bool visible)
    {
        if (_root != null)
        {
            _root.SetActive(visible);
        }
    }

    public void SetBestScore(int bestScore)
    {
        string textValue = $"Best: {bestScore}";
        if (_bestScoreText != null)
        {
            _bestScoreText.text = textValue;
        }

        if (_bestScoreTextTmp != null)
        {
            _bestScoreTextTmp.text = textValue;
        }
    }

    public void SetDifficultyLabel(string difficultyLabel)
    {
        string textValue = $"Difficulty: {difficultyLabel}";
        if (_difficultyText == null)
        {
            if (_difficultyTextTmp != null)
            {
                _difficultyTextTmp.text = textValue;
            }

            return;
        }

        _difficultyText.text = textValue;
        if (_difficultyTextTmp != null)
        {
            _difficultyTextTmp.text = textValue;
        }
    }

    public void SetDifficultyDropdownOptions(string[] labels)
    {
        if (_difficultyDropdown == null || labels == null || labels.Length == 0)
        {
            return;
        }

        _difficultyDropdown.options.Clear();
        for (int i = 0; i < labels.Length; i++)
        {
            _difficultyDropdown.options.Add(new TMP_Dropdown.OptionData(labels[i]));
        }

        _difficultyDropdown.RefreshShownValue();
    }

    public void SetDifficultyDropdownValue(int index)
    {
        if (_difficultyDropdown == null || _difficultyDropdown.options == null || _difficultyDropdown.options.Count == 0)
        {
            return;
        }

        int clamped = Mathf.Clamp(index, 0, _difficultyDropdown.options.Count - 1);
        _difficultyDropdown.SetValueWithoutNotify(clamped);
        _difficultyDropdown.RefreshShownValue();
    }

    private void HandlePlayClicked()
    {
        PlayClicked?.Invoke();
    }

    private void HandleSettingsClicked()
    {
        SettingsClicked?.Invoke();
    }

    private void HandleDifficultyClicked()
    {
        DifficultyClicked?.Invoke();
    }

    private void HandleDifficultyDropdownChanged(int index)
    {
        DifficultyDropdownChanged?.Invoke(index);
    }
}
