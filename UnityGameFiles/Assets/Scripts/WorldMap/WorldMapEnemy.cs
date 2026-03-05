using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Represents an enemy entity on the world map that the player can interact with.
/// Difficulty level determines the number of plants in the encounter:
///   Difficulty 1 = 1 random plant
///   Difficulty 2 = 2 random plants
///   Difficulty 3 = 3 random plants
/// </summary>
public class WorldMapEnemy : MonoBehaviour
{
    [Header("Enemy Data")]
    [SerializeField] private PlantRecognitionSystem.PlantType enemyPlantType;
    [SerializeField] private PlantRecognitionSystem.ElementType enemyElement;
    [SerializeField] private string enemyDisplayName;
    [SerializeField] private int difficulty = 1; // 1-3: also determines number of plants
    [SerializeField] [TextArea(3, 5)] private string flavorText;

    [Header("Multi-Plant Encounter")]
    [SerializeField] private List<EnemyPlantEntry> enemyPlants = new List<EnemyPlantEntry>();

    [Header("Interaction Settings")]
    [SerializeField] private float interactionRange = 2f;
    [SerializeField] private KeyCode interactionKey = KeyCode.E;

    [Header("Visual Feedback")]
    [SerializeField] private GameObject interactionPrompt;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Sprite[] enemySprites;
    // enemySprites = {Sunflower, Flame tulip, Fire rose, Bubble Flower, Water Lily, Coral Bloom, Cactus, Grass Sprout, Vine Flower}
    [SerializeField] private Color highlightColor = Color.yellow;

    private Transform player;
    private bool playerInRange = false;
    private Color originalColor;
    private WorldMapSceneManager sceneManager;

    private void Awake()
    {
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }

        // Auto-generate enemy data if not set
        if (string.IsNullOrEmpty(enemyDisplayName))
        {
            GenerateRandomEnemy();
        }
    }

    private void Start()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }

        sceneManager = FindObjectOfType<WorldMapSceneManager>();

        if (interactionPrompt == null)
        {
            CreateInteractionPrompt();
        }

        if (interactionPrompt != null)
        {
            interactionPrompt.SetActive(false);
        }
    }

    /// <summary>
    /// Dynamically creates a simple interaction prompt above the enemy
    /// </summary>
    private void CreateInteractionPrompt()
    {
        GameObject promptObj = new GameObject("InteractionPrompt");
        promptObj.transform.SetParent(transform);
        promptObj.transform.localPosition = new Vector3(0, 1.5f, 0);

        Canvas canvas = promptObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;

        RectTransform canvasRect = canvas.GetComponent<RectTransform>();
        canvasRect.sizeDelta = new Vector2(2f, 0.5f);
        canvasRect.localScale = new Vector3(0.01f, 0.01f, 0.01f);

        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(promptObj.transform);

        UnityEngine.UI.Text text = textObj.AddComponent<UnityEngine.UI.Text>();
        text.text = "Press E";
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = 24;
        text.alignment = TextAnchor.MiddleCenter;
        text.color = Color.white;
        text.horizontalOverflow = HorizontalWrapMode.Overflow;
        text.verticalOverflow = VerticalWrapMode.Overflow;

        RectTransform textRect = text.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;

        GameObject bgObj = new GameObject("Background");
        bgObj.transform.SetParent(promptObj.transform);
        bgObj.transform.SetAsFirstSibling();

        UnityEngine.UI.Image bgImage = bgObj.AddComponent<UnityEngine.UI.Image>();
        bgImage.color = new Color(0, 0, 0, 0.7f);

        RectTransform bgRect = bgImage.GetComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.offsetMin = Vector2.zero;
        bgRect.offsetMax = Vector2.zero;

        interactionPrompt = promptObj;
    }

    private void Update()
    {
        if (player == null) return;

        float distance = Vector2.Distance(transform.position, player.position);
        bool wasInRange = playerInRange;
        playerInRange = distance <= interactionRange;

        if (playerInRange != wasInRange)
        {
            OnRangeChanged(playerInRange);
        }

        if (playerInRange && Input.GetKeyDown(interactionKey))
        {
            OnInteract();
        }
    }

    private void OnRangeChanged(bool inRange)
    {
        if (interactionPrompt != null)
        {
            interactionPrompt.SetActive(inRange);
        }

        if (spriteRenderer != null)
        {
            spriteRenderer.color = inRange ? highlightColor : originalColor;
        }
    }

    private void OnInteract()
    {
        if (sceneManager != null)
        {
            sceneManager.ShowBattlePreview(this);
        }
        else
        {
            Debug.LogError("WorldMapSceneManager not found!");
        }
    }

    /// <summary>
    /// Generate a random enemy encounter. Difficulty determines number of plants (1-3).
    /// </summary>
    private void GenerateRandomEnemy()
    {
        // Random difficulty 1-3
        difficulty = Random.Range(1, 4);

        // Generate plants based on difficulty (difficulty = number of plants)
        GenerateEncounterPlants();

        // Set primary display data from first plant
        if (enemyPlants.Count > 0)
        {
            var firstPlant = enemyPlants[0];
            enemyPlantType = firstPlant.plantType;
            enemyElement = firstPlant.element;
            enemyDisplayName = GetEncounterDisplayName();
        }

        // Generate flavor text
        flavorText = GenerateFlavorText();

        // Set sprite color based on difficulty
        if (spriteRenderer != null)
        {
            highlightColor = GetDifficultyColor();
            spriteRenderer.sprite = GetEnemySprite(enemyDisplayName);
        }
    }

    /// <summary>
    /// Generate random plants for the encounter based on difficulty level
    /// </summary>
    private void GenerateEncounterPlants()
    {
        enemyPlants.Clear();
        System.Array plantTypes = System.Enum.GetValues(typeof(PlantRecognitionSystem.PlantType));

        for (int i = 0; i < difficulty; i++)
        {
            var plantType = (PlantRecognitionSystem.PlantType)plantTypes.GetValue(Random.Range(0, plantTypes.Length));
            var plantData = PlantRecognitionSystem.GetPlantData(plantType);
            int artVariant = Random.Range(0, 3); // 1 of 3 art variations

            enemyPlants.Add(new EnemyPlantEntry(
                plantType,
                plantData.element,
                plantData.displayName,
                artVariant
            ));
        }
    }

    /// <summary>
    /// Get a display name for the encounter based on difficulty and plants
    /// </summary>
    private string GetEncounterDisplayName()
    {
        if (enemyPlants.Count == 1)
        {
            return enemyPlants[0].displayName;
        }
        else
        {
            return $"Wild Pack ({enemyPlants.Count} plants)";
        }
    }

    private string GenerateFlavorText()
    {
        string[] easyTexts = new string[]
        {
            "A lone wild plant stands in your path!",
            "A solitary bloom challenges you to battle!",
            "A single wild plant guards this area!"
        };

        string[] mediumTexts = new string[]
        {
            "A pair of wild plants have joined forces!",
            "Two fierce blooms block your way!",
            "A duo of wild plants stand ready to fight!"
        };

        string[] hardTexts = new string[]
        {
            "A trio of wild plants form an imposing wall!",
            "Three fierce blooms challenge all who approach!",
            "A fearsome pack of wild plants guards this territory!"
        };

        switch (difficulty)
        {
            case 1: return easyTexts[Random.Range(0, easyTexts.Length)];
            case 2: return mediumTexts[Random.Range(0, mediumTexts.Length)];
            case 3: return hardTexts[Random.Range(0, hardTexts.Length)];
            default: return "A mysterious plant appears before you!";
        }
    }

    /// <summary>
    /// Get a color representing the difficulty level for the world map sprite
    /// </summary>
    private Color GetDifficultyColor()
    {
        switch (difficulty)
        {
            case 1: return new Color(0.4f, 0.9f, 0.4f); // Green - Easy
            case 2: return new Color(0.9f, 0.7f, 0.2f); // Yellow/Orange - Medium
            case 3: return new Color(0.9f, 0.3f, 0.3f); // Red - Hard
            default: return Color.white;
        }
    }

    private Color GetElementColor(PlantRecognitionSystem.ElementType element)
    {
        switch (element)
        {
            case PlantRecognitionSystem.ElementType.Fire:
                return new Color(1f, 0.3f, 0.3f);
            case PlantRecognitionSystem.ElementType.Water:
                return new Color(0.3f, 0.5f, 1f);
            case PlantRecognitionSystem.ElementType.Grass:
                return new Color(0.3f, 1f, 0.3f);
            default:
                return Color.white;
        }
    }

    private Sprite GetEnemySprite(string enemyDisplayName)
    {
        int spriteNumber = 0;
        switch (enemyDisplayName)
        {
            case "Sunflower": spriteNumber = 0;
            break;
            case "Flame Tulip": spriteNumber = 1;
            break;
            case "Fire Rose": spriteNumber = 2;
            break;
            case "Bubble Flower": spriteNumber = 3;
            break;
            case "Water Lily": spriteNumber = 4;
            break;
            case "Coral Bloom": spriteNumber = 5;
            break;
            case "Cactus": spriteNumber = 6;
            break;
            case "Grass Sprout": spriteNumber = 7;
            break;
            case "Vine Flower": spriteNumber = 8;
            break;
        }
        return enemySprites[spriteNumber];
    }

    // Public getters for enemy data
    public PlantRecognitionSystem.PlantType GetPlantType() => enemyPlantType;
    public PlantRecognitionSystem.ElementType GetElement() => enemyElement;
    public string GetDisplayName() => enemyDisplayName;
    public int GetDifficulty() => difficulty;
    public string GetFlavorText() => flavorText;
    public List<EnemyPlantEntry> GetEncounterPlants() => enemyPlants;

    /// <summary>
    /// Set enemy data with a specific difficulty and auto-generate plants
    /// </summary>
    public void SetEnemyData(int difficultyLevel, string customFlavorText = "")
    {
        difficulty = Mathf.Clamp(difficultyLevel, 1, 3);
        GenerateEncounterPlants();

        if (enemyPlants.Count > 0)
        {
            var firstPlant = enemyPlants[0];
            enemyPlantType = firstPlant.plantType;
            enemyElement = firstPlant.element;
            enemyDisplayName = GetEncounterDisplayName();
        }

        flavorText = !string.IsNullOrEmpty(customFlavorText) ? customFlavorText : GenerateFlavorText();

        if (spriteRenderer != null)
        {
            highlightColor = GetDifficultyColor();
        }
    }

    /// <summary>
    /// Legacy setter for backward compatibility
    /// </summary>
    public void SetEnemyData(PlantRecognitionSystem.PlantType plantType, int difficultyLevel, string customFlavorText = "")
    {
        difficulty = Mathf.Clamp(difficultyLevel, 1, 3);
        enemyPlantType = plantType;
        var plantData = PlantRecognitionSystem.GetPlantData(enemyPlantType);
        enemyElement = plantData.element;
        enemyDisplayName = plantData.displayName;

        // Build plants list with the specified plant as first
        enemyPlants.Clear();
        enemyPlants.Add(new EnemyPlantEntry(plantType, plantData.element, plantData.displayName, Random.Range(0, 3)));

        // Add additional random plants for higher difficulties
        System.Array plantTypes = System.Enum.GetValues(typeof(PlantRecognitionSystem.PlantType));
        for (int i = 1; i < difficulty; i++)
        {
            var randomType = (PlantRecognitionSystem.PlantType)plantTypes.GetValue(Random.Range(0, plantTypes.Length));
            var randomData = PlantRecognitionSystem.GetPlantData(randomType);
            enemyPlants.Add(new EnemyPlantEntry(randomType, randomData.element, randomData.displayName, Random.Range(0, 3)));
        }

        enemyDisplayName = GetEncounterDisplayName();
        flavorText = !string.IsNullOrEmpty(customFlavorText) ? customFlavorText : GenerateFlavorText();

        if (spriteRenderer != null)
        {
            highlightColor = GetDifficultyColor();
            spriteRenderer.sprite = GetEnemySprite(enemyDisplayName);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionRange);
    }

    public void RemoveEnemy()
    {
        Destroy(gameObject);
    }

    public void SetActive(bool active)
    {
        gameObject.SetActive(active);
    }
}
