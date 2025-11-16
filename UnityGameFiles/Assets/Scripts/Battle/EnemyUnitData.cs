using UnityEngine;

/// <summary>
/// Singleton that stores enemy plant data from the battle scene
/// Persists between scenes so it can be accessed by PostBattleManager
/// Used when the player chooses to "Tame" the defeated enemy
/// </summary>
public class EnemyUnitData : MonoBehaviour
{
    public static EnemyUnitData Instance { get; private set; }

    [Header("Enemy Plant Info")]
    public PlantRecognitionSystem.PlantType plantType;
    public PlantRecognitionSystem.ElementType element;
    public string plantDisplayName = "Enemy Plant";

    [Header("Enemy Stats")]
    public int health = 30;
    public int attack = 10;
    public int defense = 10;

    [Header("Visual Data")]
    public Color unitColor = Color.red;

    private void Awake()
    {
        // Singleton pattern
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    /// <summary>
    /// Sets the enemy plant data
    /// </summary>
    public void SetPlantData(
        PlantRecognitionSystem.PlantType type,
        PlantRecognitionSystem.ElementType elem,
        string name,
        int hp,
        int atk,
        int def,
        Color color)
    {
        plantType = type;
        element = elem;
        plantDisplayName = name;
        health = hp;
        attack = atk;
        defense = def;
        unitColor = color;

        Debug.Log($"Enemy plant data set: {plantDisplayName} (HP:{health}, ATK:{attack}, DEF:{defense})");
    }

    /// <summary>
    /// Checks if enemy data has been set
    /// </summary>
    public bool HasData()
    {
        return !string.IsNullOrEmpty(plantDisplayName);
    }

    /// <summary>
    /// Clears the enemy data
    /// </summary>
    public void Clear()
    {
        plantDisplayName = "Enemy Plant";
        health = 30;
        attack = 10;
        defense = 10;
        unitColor = Color.red;
    }
}
