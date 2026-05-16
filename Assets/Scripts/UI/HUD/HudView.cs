using UnityEngine;
using UnityEngine.UI;
using TMPro;

public sealed class HudView : MonoBehaviour
{
    [SerializeField] private GameObject _root;
    [SerializeField] private Text _scoreText;
    [SerializeField] private TMP_Text _scoreTmpText;
    [SerializeField] private Text _bestScoreText;
    [SerializeField] private TMP_Text _bestScoreTmpText;
    [SerializeField] private Image _batteryFillImage;
    [SerializeField] private Text _batteryPercentText;
    [SerializeField] private TMP_Text _batteryPercentTmpText;
    [SerializeField] private Button _pauseButton;

    public void SetVisible(bool visible)
    {
        if (_root != null)
        {
            _root.SetActive(visible);
        }
    }

    public void SetScore(int score)
    {
        SetText(_scoreText, _scoreTmpText, $"Score: {score}");
    }

    public void SetBestScore(int bestScore)
    {
        SetText(_bestScoreText, _bestScoreTmpText, $"Best: {bestScore}");
    }

    public void SetBatteryNormalized(float normalizedBattery)
    {
        float clamped = Mathf.Clamp01(normalizedBattery);

        if (_batteryFillImage != null)
        {
            _batteryFillImage.fillAmount = clamped;
        }

        SetText(_batteryPercentText, _batteryPercentTmpText, $"Battery: {Mathf.RoundToInt(clamped * 100f)}%");
    }

    private static void SetText(Text legacyText, TMP_Text tmpText, string value)
    {
        if (tmpText != null)
        {
            tmpText.text = value;
            return;
        }

        if (legacyText != null)
        {
            legacyText.text = value;
        }
    }
}
