using UnityEngine;

/// <summary>
/// Represents an enemy entity on the world map that the player can interact with
/// </summary>
public class WorldMapEnemy : MonoBehaviour
{
    [Header("Enemy Data")]
    [SerializeField] private PlantRecognitionSystem.PlantType enemyPlantType;
    [SerializeField] private PlantRecognitionSystem.ElementType enemyElement;
    [SerializeField] private string enemyDisplayName;
    [SerializeField] private int difficulty = 1; // 1-5 difficulty rating
    [SerializeField] [TextArea(3, 5)] private string flavorText;

    [Header("Interaction Settings")]
    [SerializeField] private float interactionRange = 2f;
    [SerializeField] private KeyCode interactionKey = KeyCode.E;

    [Header("Visual Feedback")]
    [SerializeField] private GameObject interactionPrompt; // "Press E to Interact" UI
    [SerializeField] private SpriteRenderer spriteRenderer;
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
        // Find the player
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }

        // Find the scene manager
        sceneManager = FindObjectOfType<WorldMapSceneManager>();

        // Hide interaction prompt initially
        if (interactionPrompt != null)
        {
            interactionPrompt.SetActive(false);
        }
    }

    private void Update()
    {
        if (player == null) return;

        // Check distance to player
        float distance = Vector2.Distance(transform.position, player.position);
        bool wasInRange = playerInRange;
        playerInRange = distance <= interactionRange;

        // Update visual feedback
        if (playerInRange != wasInRange)
        {
            OnRangeChanged(playerInRange);
        }

        // Check for interaction input
        if (playerInRange && Input.GetKeyDown(interactionKey))
        {
            OnInteract();
        }
    }

    private void OnRangeChanged(bool inRange)
    {
        // Show/hide interaction prompt
        if (interactionPrompt != null)
        {
            interactionPrompt.SetActive(inRange);
        }

        // Highlight enemy when player is in range
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
    /// Generate random enemy data
    /// </summary>
    private void GenerateRandomEnemy()
    {
        // Random plant type
        System.Array plantTypes = System.Enum.GetValues(typeof(PlantRecognitionSystem.PlantType));
        enemyPlantType = (PlantRecognitionSystem.PlantType)plantTypes.GetValue(Random.Range(0, plantTypes.Length));

        // Get plant data
        var plantData = PlantRecognitionSystem.GetPlantData(enemyPlantType);
        enemyElement = plantData.element;
        enemyDisplayName = plantData.displayName;

        // Random difficulty (1-5)
        difficulty = Random.Range(1, 6);

        // Generate flavor text based on plant type
        flavorText = GenerateFlavorText(enemyPlantType, enemyElement);

        // Set sprite color based on element
        if (spriteRenderer != null)
        {
            spriteRenderer.color = GetElementColor(enemyElement);
            originalColor = spriteRenderer.color;
        }
    }

    private string GenerateFlavorText(PlantRecognitionSystem.PlantType plantType, PlantRecognitionSystem.ElementType element)
    {
        string[] fireTexts = new string[]
        {
            "A fierce plant warrior burning with determination!",
            "This blazing bloom won't go down without a fight!",
            "Feel the heat of battle against this fiery foe!"
        };

        string[] waterTexts = new string[]
        {
            "A calm yet powerful plant waiting to strike!",
            "This aquatic bloom flows with ancient wisdom!",
            "Dive into battle against this mysterious water guardian!"
        };

        string[] grassTexts = new string[]
        {
            "A resilient plant rooted in strength!",
            "This verdant warrior draws power from the earth!",
            "Nature's champion stands ready to defend its territory!"
        };

        switch (element)
        {
            case PlantRecognitionSystem.ElementType.Fire:
                return fireTexts[Random.Range(0, fireTexts.Length)];
            case PlantRecognitionSystem.ElementType.Water:
                return waterTexts[Random.Range(0, waterTexts.Length)];
            case PlantRecognitionSystem.ElementType.Grass:
                return grassTexts[Random.Range(0, grassTexts.Length)];
            default:
                return "A mysterious plant appears before you!";
        }
    }

    private Color GetElementColor(PlantRecognitionSystem.ElementType element)
    {
        switch (element)
        {
            case PlantRecognitionSystem.ElementType.Fire:
                return new Color(1f, 0.3f, 0.3f); // Red
            case PlantRecognitionSystem.ElementType.Water:
                return new Color(0.3f, 0.5f, 1f); // Blue
            case PlantRecognitionSystem.ElementType.Grass:
                return new Color(0.3f, 1f, 0.3f); // Green
            default:
                return Color.white;
        }
    }

    // Public getters for enemy data
    public PlantRecognitionSystem.PlantType GetPlantType() => enemyPlantType;
    public PlantRecognitionSystem.ElementType GetElement() => enemyElement;
    public string GetDisplayName() => enemyDisplayName;
    public int GetDifficulty() => difficulty;
    public string GetFlavorText() => flavorText;

    // Public setters for manual configuration
    public void SetEnemyData(PlantRecognitionSystem.PlantType plantType, int difficultyLevel, string customFlavorText = "")
    {
        enemyPlantType = plantType;
        var plantData = PlantRecognitionSystem.GetPlantData(enemyPlantType);
        enemyElement = plantData.element;
        enemyDisplayName = plantData.displayName;
        difficulty = difficultyLevel;

        if (!string.IsNullOrEmpty(customFlavorText))
        {
            flavorText = customFlavorText;
        }
        else
        {
            flavorText = GenerateFlavorText(enemyPlantType, enemyElement);
        }

        if (spriteRenderer != null)
        {
            spriteRenderer.color = GetElementColor(enemyElement);
            originalColor = spriteRenderer.color;
        }
    }

    // Optional: Visualize interaction range in editor
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionRange);
    }

    /// <summary>
    /// Remove this enemy from the world map (e.g., after defeating it)
    /// </summary>
    public void RemoveEnemy()
    {
        Destroy(gameObject);
    }

    /// <summary>
    /// Temporarily disable this enemy (e.g., during battle)
    /// </summary>
    public void SetActive(bool active)
    {
        gameObject.SetActive(active);
    }
}
