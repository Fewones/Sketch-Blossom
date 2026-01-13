using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using SketchBlossom.Progression;

public class PostBattleManager : MonoBehaviour
{
    [Header("Main UI")]
    [SerializeField] private GameObject wildGrowthPanel;
    [SerializeField] private GameObject tamePanel;
    [SerializeField] private Button wildGrowthButton;
    [SerializeField] private Button tameButton;
    [SerializeField] private Image plantImage; // For Wild-Growth option
    [SerializeField] private TextMeshProUGUI tameDescriptionText;

    [Header("Guide System")]
    [SerializeField] private Button guideButton;
    [SerializeField] private GameObject guideBookPanel;
    [SerializeField] private TextMeshProUGUI guidePageTitle;
    [SerializeField] private TextMeshProUGUI guidePageContent;
    [SerializeField] private TextMeshProUGUI pageNumberText;
    [SerializeField] private Button nextPageButton;
    [SerializeField] private Button prevPageButton;
    [SerializeField] private Button closeGuideButton;

    [Header("Scene Settings")]
    [SerializeField] private string nextSceneName = "PlantSelectionScene";

    private int currentPage = 0;
    private readonly string[] pageTitles = new string[]
    {
        "Wild Growth",
        "Tame"
    };

    private readonly string[] pageContents = new string[]
    {
        "<b>Wild Growth</b>\n\n" +
        "Wild Growth allows your plant to grow freely and reach its full natural potential. " +
        "Your plant will maintain its wild, untamed nature and develop powerful natural abilities.\n\n" +
        "<color=green>Benefits:</color>\n" +
        "• Increases base stats significantly\n" +
        "• Unlocks unique wild abilities\n" +
        "• Maintains the plant's original appearance\n" +
        "• Can be used immediately in battle",

        "<b>Tame</b>\n\n" +
        "Taming your plant allows you to add it as a new unit to your roster. " +
        "The plant will become a loyal companion that you can deploy in future battles.\n\n" +
        "<color=blue>Benefits:</color>\n" +
        "• Adds a new unit to your collection\n" +
        "• Can be summoned in future battles\n" +
        "• Grows stronger with experience\n" +
        "• Builds a diverse team of plants"
    };

    private void Awake()
    {
        // Auto-wire components if not set
        if (!wildGrowthButton) wildGrowthButton = GameObject.Find("WildGrowthButton")?.GetComponent<Button>();
        if (!tameButton) tameButton = GameObject.Find("TameButton")?.GetComponent<Button>();
        if (!guideButton) guideButton = GameObject.Find("GuideButton")?.GetComponent<Button>();
        if (!plantImage) plantImage = GameObject.Find("PlantImage")?.GetComponent<Image>();
        if (!tameDescriptionText) tameDescriptionText = GameObject.Find("TameDescriptionText")?.GetComponent<TextMeshProUGUI>();
    }

    private void Start()
    {
        // Setup buttons
        if (wildGrowthButton) wildGrowthButton.onClick.AddListener(OnWildGrowthSelected);
        if (tameButton) tameButton.onClick.AddListener(OnTameSelected);
        if (guideButton) guideButton.onClick.AddListener(OnGuideButtonClicked);
        if (closeGuideButton) closeGuideButton.onClick.AddListener(OnCloseGuide);
        if (nextPageButton) nextPageButton.onClick.AddListener(OnNextPage);
        if (prevPageButton) prevPageButton.onClick.AddListener(OnPreviousPage);

        // Set Tame description
        if (tameDescriptionText)
        {
            tameDescriptionText.text = "Add a new unit to the roster";
        }

        // Load plant image from battle
        LoadPlantImage();

        // Hide guidebook initially
        if (guideBookPanel) guideBookPanel.SetActive(false);

        // Show first page
        UpdateGuidePage();
    }

    private void OnDestroy()
    {
        // Cleanup listeners
        if (wildGrowthButton) wildGrowthButton.onClick.RemoveListener(OnWildGrowthSelected);
        if (tameButton) tameButton.onClick.RemoveListener(OnTameSelected);
        if (guideButton) guideButton.onClick.RemoveListener(OnGuideButtonClicked);
        if (closeGuideButton) closeGuideButton.onClick.RemoveListener(OnCloseGuide);
        if (nextPageButton) nextPageButton.onClick.RemoveListener(OnNextPage);
        if (prevPageButton) prevPageButton.onClick.RemoveListener(OnPreviousPage);
    }

    private void LoadPlantImage()
    {
        if (DrawnUnitData.Instance != null && DrawnUnitData.Instance.drawingTexture != null)
        {
            Texture2D drawingTexture = DrawnUnitData.Instance.drawingTexture;
            Debug.Log($"[PostBattle] DrawnUnitData texture: {drawingTexture.width}x{drawingTexture.height}");

            Sprite plantSprite = Texture2DToSprite(drawingTexture);

            if (plantImage != null && plantSprite != null)
            {
                plantImage.sprite = plantSprite;
                Debug.Log("[PostBattle] Loaded plant image from DrawnUnitData");
                plantImage.color = Color.white;
                plantImage.preserveAspect = true;
            }
            else
            {
                Debug.LogWarning($"[PostBattle] plantImage null? {plantImage == null}, plantSprite null? {plantSprite == null}");
            }
        }
        else
        {
            Debug.LogWarning("[PostBattle] No plant drawing data found. Using default placeholder.");
            if (plantImage)
            {
                plantImage.color = new Color(0.3f, 1f, 0.3f);
            }
        }
    }


    private Sprite Texture2DToSprite(Texture2D texture)
    {
        if (texture == null) return null;

        // Create sprite from texture
        Sprite sprite = Sprite.Create(
            texture,
            new Rect(0, 0, texture.width, texture.height),
            new Vector2(0.5f, 0.5f), // Pivot at center
            100f // Pixels per unit
        );
        return sprite;
    }

    public void OnWildGrowthSelected()
    {
        Debug.Log("Wild Growth selected!");

        // Get the inventory instance
        PlayerInventory inventory = PlayerInventory.Instance;
        if (inventory == null)
        {
            Debug.LogError("PlayerInventory not found! Cannot apply Wild Growth.");
            return;
        }

        // Try to re-select the plant that was actually used in battle
        if (DrawnUnitData.Instance != null &&
            !string.IsNullOrEmpty(DrawnUnitData.Instance.inventoryPlantId))
        {
            inventory.SelectPlant(DrawnUnitData.Instance.inventoryPlantId);
            Debug.Log($"Re-selected plant with ID {DrawnUnitData.Instance.inventoryPlantId} for Wild Growth based on DrawnUnitData.");
        }
        else
        {
            // Fallback: we just keep the current selection, but warn in the log
            Debug.LogWarning("DrawnUnitData has no inventoryPlantId set. Wild Growth will use the currently selected plant in inventory.");
        }

        // IMPORTANT: do NOT apply WildGrowth here anymore.
        // The drawing-based upgrade is handled inside WildGrowthSceneManager
        // after the player finishes their Wild Growth drawing.

        // Store the choice
        PlayerPrefs.SetString("GrowthMode", "WILD");
        PlayerPrefs.Save();

        // Transition to WildGrowthScene
        Debug.Log("Loading WildGrowthScene...");
        SceneManager.LoadScene("WildGrowthScene");
    }


    public void OnTameSelected()
    {
        Debug.Log("Tame selected!");

        // Get the inventory instance
        PlayerInventory inventory = PlayerInventory.Instance;
        if (inventory == null)
        {
            Debug.LogError("PlayerInventory not found! Cannot tame plant.");
            return;
        }

        // Get the enemy plant data
        if (EnemyUnitData.Instance != null && EnemyUnitData.Instance.HasData())
        {
            // Clear enemy data
            EnemyUnitData.Instance.Clear();
        }
        else
        {
            Debug.LogWarning("No enemy plant data found to tame!");
        }

        // Store the choice
        PlayerPrefs.SetString("GrowthMode", "TAME");
        PlayerPrefs.Save();

        // Transition to TameScene
        Debug.Log("Loading TameScene...");
        SceneManager.LoadScene("TameScene");
    }

    private IEnumerator TransitionToNextScene()
    {
        yield return new WaitForSeconds(0.5f);

        // Check if this was a world map encounter
        if (EnemyEncounterData.Instance != null && EnemyEncounterData.Instance.isWorldMapEncounter)
        {
            // Clear encounter data
            EnemyEncounterData.Instance.ClearEncounterData();

            // Return to world map
            Debug.Log("Returning to World Map...");
            SceneManager.LoadScene("WorldMapScene");
        }
        else if (!string.IsNullOrEmpty(nextSceneName))
        {
            // Default behavior: return to plant selection scene
            SceneManager.LoadScene(nextSceneName);
        }
        else
        {
            Debug.LogWarning("Next scene name is not set!");
        }
    }

    // Guide Book Methods
    public void OnGuideButtonClicked()
    {
        if (guideBookPanel)
        {
            guideBookPanel.SetActive(true);
            currentPage = 0;
            UpdateGuidePage();
        }
    }

    public void OnCloseGuide()
    {
        if (guideBookPanel)
        {
            guideBookPanel.SetActive(false);
        }
    }

    public void OnNextPage()
    {
        currentPage = (currentPage + 1) % pageTitles.Length;
        UpdateGuidePage();
    }

    public void OnPreviousPage()
    {
        currentPage--;
        if (currentPage < 0) currentPage = pageTitles.Length - 1;
        UpdateGuidePage();
    }

    private void UpdateGuidePage()
    {
        if (guidePageTitle)
        {
            guidePageTitle.text = pageTitles[currentPage];
        }

        if (guidePageContent)
        {
            guidePageContent.text = pageContents[currentPage];
        }

        if (pageNumberText)
        {
            pageNumberText.text = $"Page {currentPage + 1} / {pageTitles.Length}";
        }
    }

    // Keyboard shortcuts
    private void Update()
    {
        // Press G to open guide
        if (Input.GetKeyDown(KeyCode.G))
        {
            if (guideBookPanel && !guideBookPanel.activeSelf)
            {
                OnGuideButtonClicked();
            }
        }

        // Press Escape to close guide
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (guideBookPanel && guideBookPanel.activeSelf)
            {
                OnCloseGuide();
            }
        }

        // Arrow keys for page navigation
        if (guideBookPanel && guideBookPanel.activeSelf)
        {
            if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                OnNextPage();
            }
            else if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                OnPreviousPage();
            }
        }
    }
}
