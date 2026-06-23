using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
#endif

public class MainMenuController : MonoBehaviour
{
    private Canvas canvas;
    private Button startButton;
    private Button stageSelectButton;
    private Button settingsButton;
    private Button resetButton;
    private GameObject stageSelectPanel;
    private readonly System.Collections.Generic.List<Button> stageButtons = new System.Collections.Generic.List<Button>();

    private void Awake()
    {
        if (SceneManager.GetActiveScene().name != "Main")
        {
            Destroy(gameObject);
            return;
        }

#if ENABLE_INPUT_SYSTEM
        EnsureInputSystemUiModule();
#endif

        canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            Debug.LogWarning("MainMenuController: Canvas not found");
            return;
        }

        startButton = FindButton("StartButton");
        stageSelectButton = FindButton("StageSelectButton");
        settingsButton = FindButton("SettingButton");
        resetButton = FindButton("ResetButton");
        if (resetButton == null)
        {
            resetButton = FindButton("StageReset");
        }
        if (resetButton == null)
        {
            resetButton = FindButton("StageResetButton");
        }

        if (resetButton == null)
        {
            resetButton = CreateResetButtonFromTemplate(settingsButton);
        }

        WireButtons();
        BuildStageSelectPanel();
    }

    private void OnEnable()
    {
        if (SceneManager.GetActiveScene().name != "Main")
        {
            return;
        }

#if ENABLE_INPUT_SYSTEM
        EnsureInputSystemUiModule();
#endif

        if (canvas == null)
        {
            canvas = FindObjectOfType<Canvas>();
        }

        if (startButton == null) startButton = FindButton("StartButton");
        if (stageSelectButton == null) stageSelectButton = FindButton("StageSelectButton");
        if (settingsButton == null) settingsButton = FindButton("SettingButton");
        if (resetButton == null) resetButton = FindButton("ResetButton");
        if (resetButton == null) resetButton = FindButton("StageReset");
        if (resetButton == null) resetButton = FindButton("StageResetButton");

        WireButtons();
        RefreshStageButtons();
    }

    private void WireButtons()
    {
        if (startButton != null)
        {
            startButton.onClick.RemoveAllListeners();
            startButton.onClick.AddListener(() => StageSelectionManager.LoadStage(StageSelectionManager.GetHighestUnlockedStage()));
        }

        if (stageSelectButton != null)
        {
            stageSelectButton.onClick.RemoveAllListeners();
            stageSelectButton.onClick.AddListener(ToggleStageSelectPanel);
        }

        if (settingsButton != null)
        {
            settingsButton.onClick.RemoveAllListeners();
            settingsButton.onClick.AddListener(() => Debug.Log("Settings is not implemented yet."));
        }

        if (resetButton != null)
        {
            resetButton.onClick.RemoveAllListeners();
            resetButton.onClick.AddListener(ResetStageProgress);
        }
    }

    private void BuildStageSelectPanel()
    {
        if (stageSelectPanel != null || canvas == null)
        {
            return;
        }

        Button templateButton = FindButton("StartButton");
        TMP_Text templateText = templateButton != null ? templateButton.GetComponentInChildren<TMP_Text>(true) : null;
        stageButtons.Clear();

        stageSelectPanel = new GameObject("StageSelectPanel", typeof(RectTransform), typeof(Image));
        stageSelectPanel.transform.SetParent(canvas.transform, false);
        RectTransform panelRect = stageSelectPanel.GetComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;
        stageSelectPanel.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.7f);

        GameObject window = new GameObject("Window", typeof(RectTransform), typeof(Image));
        window.transform.SetParent(stageSelectPanel.transform, false);
        RectTransform windowRect = window.GetComponent<RectTransform>();
        windowRect.anchorMin = new Vector2(0.08f, 0.1f);
        windowRect.anchorMax = new Vector2(0.92f, 0.9f);
        windowRect.offsetMin = Vector2.zero;
        windowRect.offsetMax = Vector2.zero;
        window.GetComponent<Image>().color = new Color(0.86f, 0.74f, 0.76f, 0.98f);

        TMP_Text title = CreateLabel("Stage Select", templateText, window.transform);
        RectTransform titleRect = title.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0.5f, 1f);
        titleRect.anchorMax = new Vector2(0.5f, 1f);
        titleRect.pivot = new Vector2(0.5f, 1f);
        titleRect.anchoredPosition = new Vector2(0f, -30f);
        titleRect.sizeDelta = new Vector2(500f, 80f);
        title.fontSize = 48f;

        Button closeButton = CreateActionButton("CloseButton", "Close", templateButton, window.transform, templateText);
        RectTransform closeRect = closeButton.GetComponent<RectTransform>();
        closeRect.anchorMin = new Vector2(1f, 1f);
        closeRect.anchorMax = new Vector2(1f, 1f);
        closeRect.pivot = new Vector2(1f, 1f);
        closeRect.anchoredPosition = new Vector2(-30f, -30f);
        closeRect.sizeDelta = new Vector2(180f, 70f);
        closeButton.onClick.AddListener(ToggleStageSelectPanel);

        GameObject scrollRoot = new GameObject("StageScrollView", typeof(RectTransform), typeof(Image), typeof(Mask), typeof(ScrollRect));
        scrollRoot.transform.SetParent(window.transform, false);
        RectTransform scrollRectTransform = scrollRoot.GetComponent<RectTransform>();
        scrollRectTransform.anchorMin = new Vector2(0.08f, 0.08f);
        scrollRectTransform.anchorMax = new Vector2(0.92f, 0.8f);
        scrollRectTransform.offsetMin = Vector2.zero;
        scrollRectTransform.offsetMax = Vector2.zero;
        Image scrollImage = scrollRoot.GetComponent<Image>();
        scrollImage.color = new Color(1f, 1f, 1f, 0.15f);
        scrollRoot.GetComponent<Mask>().showMaskGraphic = false;

        GameObject content = new GameObject("Content", typeof(RectTransform), typeof(GridLayoutGroup), typeof(ContentSizeFitter));
        content.transform.SetParent(scrollRoot.transform, false);
        RectTransform contentRect = content.GetComponent<RectTransform>();
        contentRect.anchorMin = new Vector2(0.5f, 1f);
        contentRect.anchorMax = new Vector2(0.5f, 1f);
        contentRect.pivot = new Vector2(0.5f, 1f);
        contentRect.anchoredPosition = Vector2.zero;
        contentRect.sizeDelta = new Vector2(560f, 0f);

        GridLayoutGroup grid = content.GetComponent<GridLayoutGroup>();
        grid.cellSize = new Vector2(160f, 80f);
        grid.spacing = new Vector2(20f, 20f);
        grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        grid.constraintCount = 3;
        grid.padding = new RectOffset(20, 20, 20, 20);
        grid.childAlignment = TextAnchor.UpperCenter;

        ContentSizeFitter fitter = content.GetComponent<ContentSizeFitter>();
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;

        ScrollRect scrollRect = scrollRoot.GetComponent<ScrollRect>();
        scrollRect.content = contentRect;
        scrollRect.viewport = scrollRectTransform;
        scrollRect.horizontal = false;
        scrollRect.vertical = true;
        scrollRect.movementType = ScrollRect.MovementType.Clamped;
        scrollRect.scrollSensitivity = 30f;

        for (int stageId = 1; stageId <= StageSelectionManager.StageCount; stageId++)
        {
            int capturedStageId = stageId;
            Button stageButton = CreateActionButton($"Stage{stageId:00}", $"Stage {stageId}", templateButton, content.transform, templateText);
            stageButton.onClick.AddListener(() => StageSelectionManager.LoadStage(capturedStageId));
            stageButtons.Add(stageButton);
        }

        RefreshStageButtons();
        stageSelectPanel.SetActive(false);
    }

    private void ToggleStageSelectPanel()
    {
        if (stageSelectPanel != null)
        {
            RefreshStageButtons();
            stageSelectPanel.SetActive(!stageSelectPanel.activeSelf);
        }
    }

    private Button FindButton(string objectName)
    {
        Button[] buttons = FindObjectsOfType<Button>(true);
        foreach (Button button in buttons)
        {
            if (button.name == objectName)
            {
                return button;
            }
        }

        return null;
    }

    private Button CreateResetButtonFromTemplate(Button template)
    {
        if (template == null)
        {
            return null;
        }

        GameObject buttonObject = Object.Instantiate(template.gameObject, template.transform.parent);
        buttonObject.name = "ResetButton";
        buttonObject.SetActive(true);

        RectTransform templateRect = template.GetComponent<RectTransform>();
        RectTransform resetRect = buttonObject.GetComponent<RectTransform>();
        resetRect.anchorMin = templateRect.anchorMin;
        resetRect.anchorMax = templateRect.anchorMax;
        resetRect.pivot = templateRect.pivot;
        resetRect.sizeDelta = templateRect.sizeDelta;
        resetRect.anchoredPosition = templateRect.anchoredPosition + new Vector2(0f, -160f);

        TMP_Text labelText = buttonObject.GetComponentInChildren<TMP_Text>(true);
        if (labelText != null)
        {
            labelText.text = "Stage Reset";
        }

        return buttonObject.GetComponent<Button>();
    }

    private Button CreateActionButton(string objectName, string label, Button template, Transform parent, TMP_Text templateText)
    {
        GameObject buttonObject;
        if (template != null)
        {
            buttonObject = Object.Instantiate(template.gameObject, parent);
            buttonObject.name = objectName;
            buttonObject.SetActive(true);
        }
        else
        {
            buttonObject = new GameObject(objectName, typeof(RectTransform), typeof(Image), typeof(Button));
            buttonObject.transform.SetParent(parent, false);
            buttonObject.GetComponent<Image>().color = Color.white;
        }

        Button button = buttonObject.GetComponent<Button>();
        button.onClick.RemoveAllListeners();

        TMP_Text labelText = buttonObject.GetComponentInChildren<TMP_Text>(true);
        if (labelText == null)
        {
            labelText = CreateLabel(label, templateText, buttonObject.transform);
            RectTransform labelRect = labelText.GetComponent<RectTransform>();
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.offsetMin = Vector2.zero;
            labelRect.offsetMax = Vector2.zero;
        }

        labelText.text = label;
        labelText.alignment = TextAlignmentOptions.Center;
        labelText.enableWordWrapping = false;
        labelText.fontSize = 34f;
        return button;
    }

    private TMP_Text CreateLabel(string text, TMP_Text templateText, Transform parent)
    {
        GameObject labelObject = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
        labelObject.transform.SetParent(parent, false);
        TMP_Text label = labelObject.GetComponent<TMP_Text>();
        if (templateText != null)
        {
            label.font = templateText.font;
            label.fontSharedMaterial = templateText.fontSharedMaterial;
            label.color = templateText.color;
        }

        label.text = text;
        label.alignment = TextAlignmentOptions.Center;
        label.enableWordWrapping = false;
        label.fontSize = 36f;
        return label;
    }

    private void RefreshStageButtons()
    {
        for (int i = 0; i < stageButtons.Count; i++)
        {
            int stageId = i + 1;
            bool unlocked = StageSelectionManager.IsStageUnlocked(stageId);
            Button button = stageButtons[i];
            button.interactable = unlocked;

            Image image = button.GetComponent<Image>();
            if (image != null)
            {
                Color color = image.color;
                color.a = unlocked ? 1f : 0.4f;
                image.color = color;
            }

            TMP_Text text = button.GetComponentInChildren<TMP_Text>(true);
            if (text != null)
            {
                Color color = text.color;
                color.a = unlocked ? 1f : 0.45f;
                text.color = color;
            }
        }
    }

    private void ResetStageProgress()
    {
        StageSelectionManager.ResetProgress();
        RefreshStageButtons();
        Debug.Log("MainMenuController: Stage progress reset");
    }

#if ENABLE_INPUT_SYSTEM
    private static void EnsureInputSystemUiModule()
    {
        EventSystem eventSystem = Object.FindObjectOfType<EventSystem>();
        if (eventSystem == null)
        {
            return;
        }

        InputSystemUIInputModule inputModule = eventSystem.GetComponent<InputSystemUIInputModule>();
        if (inputModule == null)
        {
            inputModule = eventSystem.gameObject.AddComponent<InputSystemUIInputModule>();
            inputModule.AssignDefaultActions();
        }

        StandaloneInputModule standaloneModule = eventSystem.GetComponent<StandaloneInputModule>();
        if (standaloneModule != null)
        {
            Object.Destroy(standaloneModule);
        }
    }
#endif
}
