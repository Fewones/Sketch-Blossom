using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public enum GrowthMode { None, Tame, Wild }

public class PostBattleManager : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Button tameButton;
    [SerializeField] private Button wildButton;
    [SerializeField] private Button finishButton;   // your renamed Continue → Finish
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI subtitleText;
    [SerializeField] private TextMeshProUGUI descriptionText; // optional helper text

    [Header("Scene Names")]
    [SerializeField] private string drawingSceneName = "DrawingScene";
    [SerializeField] private string nextSceneName = "WorldMapScene"; // or BattleScene, configure in Inspector

    private GrowthMode selected = GrowthMode.None;

    private const string PREF_GROWTH_MODE = "GrowthMode"; // shared flag for DrawingScene

    private void Awake()
    {
        // Basic safety: find by name if not wired yet
        if (!tameButton)      tameButton = GameObject.Find("TameButton")?.GetComponent<Button>();
        if (!wildButton)      wildButton = GameObject.Find("WildButton")?.GetComponent<Button>();
        if (!finishButton)    finishButton = GameObject.Find("FinishButton")?.GetComponent<Button>()
                                 ?? GameObject.Find("ContinueButton opt")?.GetComponent<Button>();
        if (!titleText)       titleText = GameObject.Find("TitleText")?.GetComponent<TextMeshProUGUI>();
        if (!subtitleText)    subtitleText = GameObject.Find("SubtitleText")?.GetComponent<TextMeshProUGUI>();
        if (!descriptionText) descriptionText = GameObject.Find("DescriptionText")?.GetComponent<TextMeshProUGUI>();
    }

    private void Start()
    {
        // Initial UI
        if (titleText)    titleText.text = "Victory! Choose Your Growth Path";
        if (subtitleText) subtitleText.text = "Choose one option:";
        if (descriptionText) descriptionText.text = "";

        if (finishButton) finishButton.gameObject.SetActive(false);

        // Wire buttons
        if (tameButton)   tameButton.onClick.AddListener(OnTameSelected);
        if (wildButton)   wildButton.onClick.AddListener(OnWildSelected);
        if (finishButton) finishButton.onClick.AddListener(OnFinish);
    }

    private void OnDestroy()
    {
        // Unwire to avoid leaks on domain reload
        if (tameButton)   tameButton.onClick.RemoveListener(OnTameSelected);
        if (wildButton)   wildButton.onClick.RemoveListener(OnWildSelected);
        if (finishButton) finishButton.onClick.RemoveListener(OnFinish);
    }

    public void OnTameSelected()
    {
        selected = GrowthMode.Tame;
        PlayerPrefs.SetString(PREF_GROWTH_MODE, "TAME");

        if (descriptionText)
            descriptionText.text = "🌱 TAME: Draw a brand new plant with limited strokes. " +
                                   "Your drawing will be analyzed to generate a new card.";

        // Jump directly into the Drawing Scene
        LoadDrawingScene();
    }

    public void OnWildSelected()
    {
        selected = GrowthMode.Wild;
        PlayerPrefs.SetString(PREF_GROWTH_MODE, "WILD");

        if (descriptionText)
            descriptionText.text = "🌿 WILD GROWTH: Choose a card and add thorns/leaves/flowers. " +
                                   "Stats will evolve from your additions.";

        // Load WildGrowthScene instead of DrawingScene
        SceneManager.LoadScene("WildGrowthScene");
    }

    // If you ever want Finish to skip drawing and continue, keep this:
    public void OnFinish()
    {
        // Proceed to map/next encounter after post-battle
        if (!string.IsNullOrEmpty(nextSceneName))
        {
            SceneManager.LoadScene(nextSceneName);
        }
        else
        {
            Debug.LogWarning("PostBattleManager: nextSceneName is empty.");
        }
    }

    private void LoadDrawingScene()
    {
        if (!string.IsNullOrEmpty(drawingSceneName))
        {
            SceneManager.LoadScene(drawingSceneName);
        }
        else
        {
            Debug.LogError("PostBattleManager: drawingSceneName is empty.");
        }
    }
}
