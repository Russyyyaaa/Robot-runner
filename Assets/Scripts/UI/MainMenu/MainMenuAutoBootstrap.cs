using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public sealed class MainMenuAutoBootstrap : MonoBehaviour
{
    private const string MenuSceneName = "SampleScene";

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void EnsureMainMenu()
    {
        Scene activeScene = SceneManager.GetActiveScene();
        if (activeScene.name != MenuSceneName)
        {
            return;
        }

        if (UnityEngine.Object.FindFirstObjectByType<MainMenuView>(FindObjectsInactive.Include) != null)
        {
            return;
        }

        EnsureEventSystem();
        GameFlowController gameFlowController = EnsureGameFlowController();
        BuildMenuUi(gameFlowController);
    }

    private static void EnsureEventSystem()
    {
        if (UnityEngine.Object.FindFirstObjectByType<EventSystem>() != null)
        {
            return;
        }

        GameObject eventSystemObject = new GameObject("EventSystem");
        eventSystemObject.AddComponent<EventSystem>();

        System.Type inputSystemUiModuleType = System.Type.GetType("UnityEngine.InputSystem.UI.InputSystemUIInputModule, Unity.InputSystem");
        if (inputSystemUiModuleType != null)
        {
            eventSystemObject.AddComponent(inputSystemUiModuleType);
            return;
        }

        eventSystemObject.AddComponent<StandaloneInputModule>();
    }

    private static GameFlowController EnsureGameFlowController()
    {
        GameFlowController existing = UnityEngine.Object.FindFirstObjectByType<GameFlowController>();
        if (existing != null)
        {
            return existing;
        }

        GameObject gameFlowObject = new GameObject("GameFlowController");
        return gameFlowObject.AddComponent<GameFlowController>();
    }

    private static void BuildMenuUi(GameFlowController gameFlowController)
    {
        GameObject canvasObject = new GameObject("MainMenuCanvas");
        Canvas canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasObject.AddComponent<GraphicRaycaster>();

        CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080f, 1920f);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 1f;

        GameObject rootObject = CreateUiObject("MainMenuRoot", canvasObject.transform);
        RectTransform rootRect = rootObject.GetComponent<RectTransform>();
        rootRect.anchorMin = Vector2.zero;
        rootRect.anchorMax = Vector2.one;
        rootRect.offsetMin = Vector2.zero;
        rootRect.offsetMax = Vector2.zero;

        Image background = rootObject.AddComponent<Image>();
        background.color = new Color(0f, 0f, 0f, 0.35f);

        GameObject titleObject = CreateTextObject("Title", rootObject.transform, "ROBOT RUN", 96, TextAnchor.MiddleCenter);
        SetAnchoredRect(titleObject.GetComponent<RectTransform>(), new Vector2(0.5f, 0.82f), new Vector2(700f, 160f), Vector2.zero);

        GameObject bestScoreObject = CreateTextObject("BestScoreText", rootObject.transform, "Best: 0", 54, TextAnchor.MiddleCenter);
        SetAnchoredRect(bestScoreObject.GetComponent<RectTransform>(), new Vector2(0.5f, 0.70f), new Vector2(560f, 90f), Vector2.zero);

        GameObject difficultyTextObject = CreateTextObject("DifficultyText", rootObject.transform, "Difficulty: Normal", 50, TextAnchor.MiddleCenter);
        SetAnchoredRect(difficultyTextObject.GetComponent<RectTransform>(), new Vector2(0.5f, 0.61f), new Vector2(700f, 90f), Vector2.zero);

        Button difficultyButton = CreateButton(rootObject.transform, "DifficultyButton", "Change Difficulty", new Vector2(0.5f, 0.52f), new Vector2(540f, 120f));
        Button playButton = CreateButton(rootObject.transform, "PlayButton", "Play", new Vector2(0.5f, 0.41f), new Vector2(540f, 140f));
        Button settingsButton = CreateButton(rootObject.transform, "SettingsButton", "Difficulty +", new Vector2(0.5f, 0.31f), new Vector2(420f, 110f));

        rootObject.SetActive(false);
        MainMenuView view = rootObject.AddComponent<MainMenuView>();
        view.Configure(
            rootObject,
            playButton,
            settingsButton,
            difficultyButton,
            bestScoreObject.GetComponent<Text>(),
            difficultyTextObject.GetComponent<Text>());

        MainMenuPresenter presenter = rootObject.AddComponent<MainMenuPresenter>();
        presenter.Configure(view, null, gameFlowController);
        rootObject.SetActive(true);
    }

    private static Button CreateButton(Transform parent, string objectName, string label, Vector2 anchor, Vector2 size)
    {
        GameObject buttonObject = CreateUiObject(objectName, parent);
        RectTransform rectTransform = buttonObject.GetComponent<RectTransform>();
        SetAnchoredRect(rectTransform, anchor, size, Vector2.zero);

        Image image = buttonObject.AddComponent<Image>();
        image.color = new Color(0.12f, 0.35f, 0.75f, 0.95f);

        Button button = buttonObject.AddComponent<Button>();
        ColorBlock colors = button.colors;
        colors.highlightedColor = new Color(0.17f, 0.42f, 0.84f, 1f);
        colors.pressedColor = new Color(0.08f, 0.24f, 0.55f, 1f);
        button.colors = colors;

        GameObject labelObject = CreateTextObject("Label", buttonObject.transform, label, 46, TextAnchor.MiddleCenter);
        RectTransform labelRect = labelObject.GetComponent<RectTransform>();
        labelRect.anchorMin = Vector2.zero;
        labelRect.anchorMax = Vector2.one;
        labelRect.offsetMin = Vector2.zero;
        labelRect.offsetMax = Vector2.zero;

        return button;
    }

    private static GameObject CreateTextObject(string objectName, Transform parent, string textValue, int fontSize, TextAnchor alignment)
    {
        GameObject textObject = CreateUiObject(objectName, parent);
        Text text = textObject.AddComponent<Text>();
        Font builtInFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (builtInFont == null)
        {
            builtInFont = Resources.GetBuiltinResource<Font>("Arial.ttf");
        }

        text.font = builtInFont;
        text.fontSize = fontSize;
        text.alignment = alignment;
        text.color = Color.white;
        text.text = textValue;
        text.horizontalOverflow = HorizontalWrapMode.Overflow;
        text.verticalOverflow = VerticalWrapMode.Truncate;
        return textObject;
    }

    private static GameObject CreateUiObject(string objectName, Transform parent)
    {
        GameObject uiObject = new GameObject(objectName);
        RectTransform rectTransform = uiObject.AddComponent<RectTransform>();
        rectTransform.SetParent(parent, false);
        rectTransform.localScale = Vector3.one;
        return uiObject;
    }

    private static void SetAnchoredRect(RectTransform rectTransform, Vector2 anchor, Vector2 size, Vector2 anchoredPosition)
    {
        rectTransform.anchorMin = anchor;
        rectTransform.anchorMax = anchor;
        rectTransform.sizeDelta = size;
        rectTransform.anchoredPosition = anchoredPosition;
    }
}
