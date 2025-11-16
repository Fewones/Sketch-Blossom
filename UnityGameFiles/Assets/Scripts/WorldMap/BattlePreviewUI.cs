using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

/// <summary>
/// UI popup that displays battle preview information when interacting with an enemy
/// </summary>
public class BattlePreviewUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject popupPanel;
    [SerializeField] private TextMeshProUGUI enemyNameText;
    [SerializeField] private TextMeshProUGUI elementText;
    [SerializeField] private TextMeshProUGUI difficultyText;
    [SerializeField] private TextMeshProUGUI flavorText;
    [SerializeField] private Button goToBattleButton;
    [SerializeField] private Button cancelButton;
    [SerializeField] private Image enemyElementIcon; // Optional: icon for element type

    [Header("Visual Settings")]
    [SerializeField] private Color fireColor = new Color(1f, 0.3f, 0.3f);
    [SerializeField] private Color waterColor = new Color(0.3f, 0.5f, 1f);
    [SerializeField] private Color grassColor = new Color(0.3f, 1f, 0.3f);

    private WorldMapEnemy currentEnemy;

    private void Awake()
    {
        // Setup button listeners
        if (goToBattleButton != null)
        {
            goToBattleButton.onClick.AddListener(OnGoToBattleClicked);
        }

        if (cancelButton != null)
        {
            cancelButton.onClick.AddListener(OnCancelClicked);
        }

        // Hide popup initially
        if (popupPanel != null)
        {
            popupPanel.SetActive(false);
        }
    }

    /// <summary>
    /// Show the battle preview popup with enemy data
    /// </summary>
    public void ShowPreview(WorldMapEnemy enemy)
    {
        if (enemy == null)
        {
            Debug.LogError("Cannot show preview: enemy is null");
            return;
        }

        currentEnemy = enemy;

        // Populate UI with enemy data
        if (enemyNameText != null)
        {
            enemyNameText.text = enemy.GetDisplayName();
        }

        if (elementText != null)
        {
            PlantRecognitionSystem.ElementType element = enemy.GetElement();
            elementText.text = $"Element: {element}";

            // Color the element text
            elementText.color = GetElementColor(element);
        }

        if (difficultyText != null)
        {
            int difficulty = enemy.GetDifficulty();
            difficultyText.text = $"Difficulty: {GetDifficultyStars(difficulty)}";
        }

        if (flavorText != null)
        {
            flavorText.text = enemy.GetFlavorText();
        }

        // Set element icon color if available
        if (enemyElementIcon != null)
        {
            enemyElementIcon.color = GetElementColor(enemy.GetElement());
        }

        // Show the popup
        if (popupPanel != null)
        {
            popupPanel.SetActive(true);
        }

        // Pause player movement
        PlayerController player = FindObjectOfType<PlayerController>();
        if (player != null)
        {
            player.SetMovementEnabled(false);
        }
    }

    /// <summary>
    /// Hide the battle preview popup
    /// </summary>
    public void HidePreview()
    {
        if (popupPanel != null)
        {
            popupPanel.SetActive(false);
        }

        currentEnemy = null;

        // Resume player movement
        PlayerController player = FindObjectOfType<PlayerController>();
        if (player != null)
        {
            player.SetMovementEnabled(true);
        }
    }

    private void OnGoToBattleClicked()
    {
        if (currentEnemy == null)
        {
            Debug.LogError("Cannot start battle: no enemy selected");
            return;
        }

        // Store enemy encounter data for the battle scene
        if (EnemyEncounterData.Instance != null)
        {
            EnemyEncounterData.Instance.SetEncounterData(
                currentEnemy.GetPlantType(),
                currentEnemy.GetElement(),
                currentEnemy.GetDisplayName(),
                currentEnemy.GetDifficulty(),
                currentEnemy.GetFlavorText()
            );
        }
        else
        {
            Debug.LogWarning("EnemyEncounterData singleton not found! Creating one...");
            GameObject encounterDataObj = new GameObject("EnemyEncounterData");
            EnemyEncounterData encounterData = encounterDataObj.AddComponent<EnemyEncounterData>();
            encounterData.SetEncounterData(
                currentEnemy.GetPlantType(),
                currentEnemy.GetElement(),
                currentEnemy.GetDisplayName(),
                currentEnemy.GetDifficulty(),
                currentEnemy.GetFlavorText()
            );
        }

        // Hide the popup
        HidePreview();

        // Transition to plant selection scene (where player chooses which plant to battle with)
        Debug.Log("Loading PlantSelectionScene for battle preparation...");
        SceneManager.LoadScene("PlantSelectionScene");
    }

    private void OnCancelClicked()
    {
        HidePreview();
    }

    /// <summary>
    /// Get color based on element type
    /// </summary>
    private Color GetElementColor(PlantRecognitionSystem.ElementType element)
    {
        switch (element)
        {
            case PlantRecognitionSystem.ElementType.Fire:
                return fireColor;
            case PlantRecognitionSystem.ElementType.Water:
                return waterColor;
            case PlantRecognitionSystem.ElementType.Grass:
                return grassColor;
            default:
                return Color.white;
        }
    }

    /// <summary>
    /// Get difficulty as stars (★★★☆☆)
    /// </summary>
    private string GetDifficultyStars(int difficulty)
    {
        string stars = "";
        for (int i = 0; i < 5; i++)
        {
            stars += i < difficulty ? "★" : "☆";
        }
        return stars;
    }
}
