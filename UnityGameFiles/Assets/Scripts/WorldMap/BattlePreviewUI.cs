using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

/// <summary>
/// UI popup that displays battle preview information when interacting with an enemy.
/// Shows difficulty level, plant count, and a list of enemy plants in the encounter.
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
    [SerializeField] private Image enemyElementIcon;

    [Header("Visual Settings")]
    [SerializeField] private Color fireColor = new Color(1f, 0.3f, 0.3f);
    [SerializeField] private Color waterColor = new Color(0.3f, 0.5f, 1f);
    [SerializeField] private Color grassColor = new Color(0.3f, 1f, 0.3f);
    [SerializeField] private Color easyColor = new Color(0.4f, 0.9f, 0.4f);
    [SerializeField] private Color mediumColor = new Color(0.9f, 0.7f, 0.2f);
    [SerializeField] private Color hardColor = new Color(0.9f, 0.3f, 0.3f);

    private WorldMapEnemy currentEnemy;

    private void Awake()
    {
        if (goToBattleButton != null)
        {
            goToBattleButton.onClick.AddListener(OnGoToBattleClicked);
        }

        if (cancelButton != null)
        {
            cancelButton.onClick.AddListener(OnCancelClicked);
        }

        if (popupPanel != null)
        {
            popupPanel.SetActive(false);
        }
    }

    /// <summary>
    /// Show the battle preview popup with enemy data including difficulty and plant list
    /// </summary>
    public void ShowPreview(WorldMapEnemy enemy)
    {
        if (enemy == null)
        {
            Debug.LogError("Cannot show preview: enemy is null");
            return;
        }

        currentEnemy = enemy;
        int difficulty = enemy.GetDifficulty();
        List<EnemyPlantEntry> plants = enemy.GetEncounterPlants();

        // Encounter title
        if (enemyNameText != null)
        {
            enemyNameText.text = enemy.GetDisplayName();
        }

        // Element text - show all elements present in the encounter
        if (elementText != null)
        {
            if (plants.Count == 1)
            {
                PlantRecognitionSystem.ElementType element = plants[0].element;
                string colorHex = ColorUtility.ToHtmlStringRGB(GetElementColor(element));
                elementText.text = $"Element: <color=#{colorHex}>{element}</color>";
            }
            else
            {
                string elementsStr = "";
                for (int i = 0; i < plants.Count; i++)
                {
                    string colorHex = ColorUtility.ToHtmlStringRGB(GetElementColor(plants[i].element));
                    if (i > 0) elementsStr += "  ";
                    elementsStr += $"<color=#{colorHex}>{plants[i].displayName} ({plants[i].element})</color>\n";
                }
                elementText.text = elementsStr;
            }
            elementText.color = Color.white; // Use white since we have rich text coloring
        }

        // Difficulty display with label and stars
        if (difficultyText != null)
        {
            string diffLabel = GetDifficultyLabel(difficulty);
            string stars = GetDifficultyStars(difficulty);
            string diffColorHex = ColorUtility.ToHtmlStringRGB(GetDifficultyColor(difficulty));
            difficultyText.text = $"Difficulty: <color=#{diffColorHex}>{diffLabel} {stars}</color>\nPlants: {plants.Count}";
        }

        // Flavor text
        if (flavorText != null)
        {
            flavorText.text = enemy.GetFlavorText();
        }

        // Set element icon to difficulty color
        if (enemyElementIcon != null)
        {
            enemyElementIcon.color = GetDifficultyColor(difficulty);
        }

        if (popupPanel != null)
        {
            popupPanel.SetActive(true);
        }

        PlayerController player = FindObjectOfType<PlayerController>();
        if (player != null)
        {
            player.SetMovementEnabled(false);
        }
    }

    public void HidePreview()
    {
        if (popupPanel != null)
        {
            popupPanel.SetActive(false);
        }

        currentEnemy = null;

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

        // Store encounter data with all plants
        if (EnemyEncounterData.Instance != null)
        {
            EnemyEncounterData.Instance.SetEncounterData(
                currentEnemy.GetEncounterPlants(),
                currentEnemy.GetDifficulty(),
                currentEnemy.GetFlavorText(),
                currentEnemy.worldMapIndex
            );
        }
        else
        {
            Debug.LogWarning("EnemyEncounterData singleton not found! Creating one...");
            GameObject encounterDataObj = new GameObject("EnemyEncounterData");
            EnemyEncounterData encounterData = encounterDataObj.AddComponent<EnemyEncounterData>();
            encounterData.SetEncounterData(
                currentEnemy.GetEncounterPlants(),
                currentEnemy.GetDifficulty(),
                currentEnemy.GetFlavorText(),
                currentEnemy.worldMapIndex
            );
        }

        HidePreview();

        PlayerController player = FindObjectOfType<PlayerController>();
        player.SetSpawnPosition(player.currentWorldMap, Vector2.zero);
        if (player.movingBackground)
        {
           player.SetBackgroundPosition(player.currentWorldMap); 
        }

        Debug.Log("Loading PlantSelectionScene for battle preparation...");
        SceneManager.LoadScene("PlantSelectionScene");
    }

    private void OnCancelClicked()
    {
        HidePreview();
    }

    private Color GetElementColor(PlantRecognitionSystem.ElementType element)
    {
        switch (element)
        {
            case PlantRecognitionSystem.ElementType.Fire: return fireColor;
            case PlantRecognitionSystem.ElementType.Water: return waterColor;
            case PlantRecognitionSystem.ElementType.Grass: return grassColor;
            default: return Color.white;
        }
    }

    private Color GetDifficultyColor(int difficulty)
    {
        switch (difficulty)
        {
            case 1: return easyColor;
            case 2: return mediumColor;
            case 3: return hardColor;
            default: return Color.white;
        }
    }

    private string GetDifficultyLabel(int difficulty)
    {
        switch (difficulty)
        {
            case 1: return "Easy";
            case 2: return "Medium";
            case 3: return "Hard";
            default: return "Unknown";
        }
    }

    private string GetDifficultyStars(int difficulty)
    {
        string stars = "";
        for (int i = 0; i < 3; i++)
        {
            stars += i < difficulty ? "★" : "☆";
        }
        return stars;
    }
}
