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
        SceneManager.LoadScene("MainMenuScene");
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
