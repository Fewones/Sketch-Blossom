using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Main manager for the World Map scene.
/// Handles player spawning, enemy setup, and scene transitions.
/// Spawns enemies with varying difficulty levels (1-3) representing different encounter sizes.
/// </summary>
public class WorldMapSceneManager : MonoBehaviour
{
    [Header("Player Setup")]
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private Vector3 playerSpawnPosition = Vector3.zero;
    [SerializeField] private PlayerController playerController;

    [Header("Enemy Setup")]
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private WorldMapEnemy[] enemies;
    private Vector3[] enemySpawnPoints = {new Vector3(-1.6f, -0.44f, 0f), new Vector3(1.95f, -0.44f, 0f), new Vector3(6.4f, -0.44f, 0f)};

    [Header("Healing Center")]
    [SerializeField] private GameObject healingCenterPrefab;
    [SerializeField] private HealingCenter healingCenter;
    [SerializeField] private Vector3 healingCenterPosition = new Vector3(-5.5f, -0.44f, 0f);

    [Header("UI References")]
    [SerializeField] private BattlePreviewUI battlePreviewUI;

    [Header("Scene Settings")]
    [SerializeField] private bool spawnEnemiesOnStart = true;
    private int numberOfEnemies = 3; // One of each difficulty by default
    [SerializeField] private bool isFirstEncounter = false; // Set true for tutorial/first map

    private void Start()
    {
        SetupPlayer();

        if (spawnEnemiesOnStart)
        {
            SetupEnemies();
        }

        SetupHealingCenter();

        if (EnemyEncounterData.Instance == null)
        {
            GameObject encounterDataObj = new GameObject("EnemyEncounterData");
            encounterDataObj.AddComponent<EnemyEncounterData>();
        }

        if (battlePreviewUI == null)
        {
            battlePreviewUI = FindObjectOfType<BattlePreviewUI>();
        }

        Debug.Log("World Map Scene initialized successfully!");
    }

    private void SetupPlayer()
    {
        if (playerController != null)
        {
            playerController.transform.position = playerSpawnPosition;
            return;
        }

        playerController = FindObjectOfType<PlayerController>();

        if (playerController == null && playerPrefab != null)
        {
            GameObject playerObj = Instantiate(playerPrefab, playerSpawnPosition, Quaternion.identity);
            playerObj.name = "Player";
            playerObj.tag = "Player";
            playerController = playerObj.GetComponent<PlayerController>();

            if (playerController == null)
            {
                playerController = playerObj.AddComponent<PlayerController>();
            }
        }

        if (playerController == null)
        {
            Debug.LogError("Failed to setup player! Make sure Player prefab is assigned or exists in scene.");
        }
    }

    private void SetupEnemies()
    {
        // If enemies are already placed in scene, use those
        enemies = FindObjectsOfType<WorldMapEnemy>();

        if (enemies.Length > 0)
        {
            Debug.Log($"Found {enemies.Length} enemies already in scene");
            return;
        }

        if (enemyPrefab == null)
        {
            Debug.LogWarning("Enemy prefab not assigned! Cannot spawn enemies.");
            return;
        }

        Vector3[] spawnPositions = GetEnemySpawnPositions();

        enemies = new WorldMapEnemy[numberOfEnemies];
        for (int i = 0; i < numberOfEnemies; i++)
        {
            Vector3 spawnPos = spawnPositions[i];
            GameObject enemyObj = Instantiate(enemyPrefab, spawnPos, Quaternion.identity);

            // Determine difficulty: if first encounter, all are difficulty 1
            // Otherwise, cycle through difficulties 1, 2, 3
            int difficulty;
            if (isFirstEncounter)
            {
                difficulty = 1;
            }
            else
            {
                difficulty = (i % 3) + 1; // 1, 2, 3, 1, 2, 3...
            }

            string diffLabel = difficulty == 1 ? "Easy" : difficulty == 2 ? "Medium" : "Hard";
            enemyObj.name = $"Enemy_{diffLabel}_{i + 1}";

            WorldMapEnemy enemy = enemyObj.GetComponent<WorldMapEnemy>();
            if (enemy == null)
            {
                enemy = enemyObj.AddComponent<WorldMapEnemy>();
            }

            // Set difficulty which also generates appropriate number of plants
            enemy.SetEnemyData(difficulty);

            enemies[i] = enemy;

            Debug.Log($"Spawned {diffLabel} enemy (difficulty {difficulty}) at {spawnPos}");
        }
    }

    private void SetupHealingCenter()
    {
        // Check if one already exists in the scene
        healingCenter = FindObjectOfType<HealingCenter>();
        if (healingCenter != null)
        {
            Debug.Log("Found Healing Center already in scene");
            return;
        }

        // Create the Healing Center game object
        GameObject healingObj;
        if (healingCenterPrefab != null)
        {
            healingObj = Instantiate(healingCenterPrefab, healingCenterPosition, Quaternion.identity);
        }
        else
        {
            healingObj = new GameObject("HealingCenter");
            healingObj.transform.position = healingCenterPosition;

            SpriteRenderer sr = healingObj.AddComponent<SpriteRenderer>();

            // Load HealingCenter sprite from Michael's Assets
            Texture2D loadedTex = Resources.Load<Texture2D>("HealingCenter");
            if (loadedTex != null)
            {
                // Make white/near-white pixels transparent
                Texture2D processedTex = new Texture2D(loadedTex.width, loadedTex.height, TextureFormat.RGBA32, false);
                processedTex.filterMode = FilterMode.Point;
                Color[] pixels = loadedTex.GetPixels();
                for (int i = 0; i < pixels.Length; i++)
                {
                    Color p = pixels[i];
                    // Treat pure white pixels as transparent (tight threshold to preserve art)
                    if (p.r > 0.98f && p.g > 0.98f && p.b > 0.98f)
                    {
                        pixels[i] = Color.clear;
                    }
                }
                processedTex.SetPixels(pixels);
                processedTex.Apply();

                sr.sprite = Sprite.Create(processedTex,
                    new Rect(0, 0, processedTex.width, processedTex.height),
                    new Vector2(0.5f, 0.5f), 100f);
                // Scale down to fit world map (adjust as needed)
                healingObj.transform.localScale = new Vector3(0.5f, 0.5f, 1f);
            }
            else
            {
                // Fallback: procedural placeholder if sprite not found
                sr.color = new Color(0.4f, 0.9f, 0.5f);
                Texture2D tex = new Texture2D(32, 32);
                Color fillColor = new Color(0.3f, 0.8f, 0.4f);
                Color crossColor = Color.white;
                for (int x = 0; x < 32; x++)
                {
                    for (int y = 0; y < 32; y++)
                    {
                        bool isCross = (x >= 12 && x <= 19) || (y >= 12 && y <= 19);
                        tex.SetPixel(x, y, isCross ? crossColor : fillColor);
                    }
                }
                tex.Apply();
                sr.sprite = Sprite.Create(tex, new Rect(0, 0, 32, 32), new Vector2(0.5f, 0.5f), 32f);
                Debug.LogWarning("HealingCenter sprite not found in Resources. Using placeholder.");
            }
        }

        healingObj.name = "HealingCenter";
        healingCenter = healingObj.GetComponent<HealingCenter>();
        if (healingCenter == null)
        {
            healingCenter = healingObj.AddComponent<HealingCenter>();
        }

        // Add the HealingCenterUI to the scene if not present
        HealingCenterUI healingUI = FindObjectOfType<HealingCenterUI>();
        if (healingUI == null)
        {
            GameObject uiObj = new GameObject("HealingCenterUI");
            uiObj.AddComponent<HealingCenterUI>();
        }

        Debug.Log($"Healing Center set up at {healingCenterPosition}");
    }

    private Vector3[] GetEnemySpawnPositions()
    {
        if (enemySpawnPoints != null && enemySpawnPoints.Length >= numberOfEnemies)
        {
            return enemySpawnPoints;
        }

        Vector3[] randomPositions = new Vector3[numberOfEnemies];
        for (int i = 0; i < numberOfEnemies; i++)
        {
            float angle = (360f / numberOfEnemies) * i;
            float distance = Random.Range(5f, 10f);

            float x = playerSpawnPosition.x + Mathf.Cos(angle * Mathf.Deg2Rad) * distance;
            float y = playerSpawnPosition.y + Mathf.Sin(angle * Mathf.Deg2Rad) * distance;

            randomPositions[i] = new Vector3(x, y, 0);
        }

        return randomPositions;
    }

    /// <summary>
    /// Called by WorldMapEnemy when player interacts with it
    /// </summary>
    public void ShowBattlePreview(WorldMapEnemy enemy)
    {
        if (battlePreviewUI == null)
        {
            Debug.LogError("BattlePreviewUI not found!");
            return;
        }

        battlePreviewUI.ShowPreview(enemy);
    }

    public void ReturnToMainMenu()
    {
        SceneManager.LoadScene("IntroScene");
    }

    public void OpenInventory()
    {
        SceneManager.LoadScene("InventoryScene");
    }

    /// <summary>
    /// Refresh enemies after battle (optional: remove defeated enemy)
    /// </summary>
    public void RefreshEnemies(bool removeDefeated = true)
    {
        if (removeDefeated && EnemyEncounterData.Instance != null && EnemyEncounterData.Instance.isWorldMapEncounter)
        {
            foreach (WorldMapEnemy enemy in enemies)
            {
                if (enemy != null && enemy.GetPlantType() == EnemyEncounterData.Instance.encounterPlantType)
                {
                    enemy.RemoveEnemy();
                    break;
                }
            }

            EnemyEncounterData.Instance.ClearEncounterData();
        }
    }
}
