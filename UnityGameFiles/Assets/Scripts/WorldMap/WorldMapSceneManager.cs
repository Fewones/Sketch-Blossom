using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Main manager for the World Map scene
/// Handles player spawning, enemy setup, and scene transitions
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
    [SerializeField] private Transform[] enemySpawnPoints;

    [Header("UI References")]
    [SerializeField] private BattlePreviewUI battlePreviewUI;

    [Header("Scene Settings")]
    [SerializeField] private bool spawnEnemiesOnStart = true;
    [SerializeField] private int numberOfEnemies = 2;

    private void Start()
    {
        // Initialize player
        SetupPlayer();

        // Initialize enemies
        if (spawnEnemiesOnStart)
        {
            SetupEnemies();
        }

        // Ensure EnemyEncounterData singleton exists
        if (EnemyEncounterData.Instance == null)
        {
            GameObject encounterDataObj = new GameObject("EnemyEncounterData");
            encounterDataObj.AddComponent<EnemyEncounterData>();
        }

        // Find battle preview UI if not assigned
        if (battlePreviewUI == null)
        {
            battlePreviewUI = FindObjectOfType<BattlePreviewUI>();
        }

        Debug.Log("World Map Scene initialized successfully!");
    }

    private void SetupPlayer()
    {
        // If player controller is already assigned, just set position
        if (playerController != null)
        {
            playerController.transform.position = playerSpawnPosition;
            return;
        }

        // Try to find existing player in scene
        playerController = FindObjectOfType<PlayerController>();

        // If no player exists, spawn one
        if (playerController == null && playerPrefab != null)
        {
            GameObject playerObj = Instantiate(playerPrefab, playerSpawnPosition, Quaternion.identity);
            playerObj.name = "Player";
            playerObj.tag = "Player";
            playerController = playerObj.GetComponent<PlayerController>();

            // Add PlayerController if not on prefab
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

        // Otherwise, spawn new enemies
        if (enemyPrefab == null)
        {
            Debug.LogWarning("Enemy prefab not assigned! Cannot spawn enemies.");
            return;
        }

        // Determine spawn points
        Vector3[] spawnPositions = GetEnemySpawnPositions();

        // Spawn enemies
        enemies = new WorldMapEnemy[numberOfEnemies];
        for (int i = 0; i < numberOfEnemies; i++)
        {
            Vector3 spawnPos = spawnPositions[i];
            GameObject enemyObj = Instantiate(enemyPrefab, spawnPos, Quaternion.identity);
            enemyObj.name = $"Enemy_{i + 1}";

            WorldMapEnemy enemy = enemyObj.GetComponent<WorldMapEnemy>();
            if (enemy == null)
            {
                enemy = enemyObj.AddComponent<WorldMapEnemy>();
            }

            enemies[i] = enemy;

            Debug.Log($"Spawned enemy {i + 1} at {spawnPos}");
        }
    }

    private Vector3[] GetEnemySpawnPositions()
    {
        // If spawn points are defined, use those
        if (enemySpawnPoints != null && enemySpawnPoints.Length >= numberOfEnemies)
        {
            Vector3[] positions = new Vector3[numberOfEnemies];
            for (int i = 0; i < numberOfEnemies; i++)
            {
                positions[i] = enemySpawnPoints[i].position;
            }
            return positions;
        }

        // Otherwise, generate random positions around the player
        Vector3[] randomPositions = new Vector3[numberOfEnemies];
        for (int i = 0; i < numberOfEnemies; i++)
        {
            // Random position in a circle around player
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

    /// <summary>
    /// Return to main menu
    /// </summary>
    public void ReturnToMainMenu()
    {
        SceneManager.LoadScene("MainMenuScene");
    }

    /// <summary>
    /// Open inventory scene
    /// </summary>
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
            // Find and remove the defeated enemy
            foreach (WorldMapEnemy enemy in enemies)
            {
                if (enemy != null && enemy.GetPlantType() == EnemyEncounterData.Instance.encounterPlantType)
                {
                    enemy.RemoveEnemy();
                    break;
                }
            }

            // Clear encounter data
            EnemyEncounterData.Instance.ClearEncounterData();
        }
    }
}
