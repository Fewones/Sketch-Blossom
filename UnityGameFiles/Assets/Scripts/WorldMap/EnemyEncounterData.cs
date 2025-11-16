using UnityEngine;

/// <summary>
/// Singleton that stores data about the selected enemy encounter on the world map
/// Persists between scene transitions to pass enemy data to the battle scene
/// </summary>
public class EnemyEncounterData : MonoBehaviour
{
    public static EnemyEncounterData Instance { get; private set; }

    [Header("Enemy Data")]
    public PlantRecognitionSystem.PlantType encounterPlantType;
    public PlantRecognitionSystem.ElementType encounterElement;
    public string encounterDisplayName;
    public int encounterDifficulty;
    public string encounterFlavorText;

    [Header("Battle Context")]
    public bool isWorldMapEncounter = false; // Flag to distinguish from random encounters

    private void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Store enemy data for the upcoming battle
    /// </summary>
    public void SetEncounterData(
        PlantRecognitionSystem.PlantType plantType,
        PlantRecognitionSystem.ElementType element,
        string displayName,
        int difficulty,
        string flavorText)
    {
        encounterPlantType = plantType;
        encounterElement = element;
        encounterDisplayName = displayName;
        encounterDifficulty = difficulty;
        encounterFlavorText = flavorText;
        isWorldMapEncounter = true;

        Debug.Log($"Enemy encounter set: {displayName} (Difficulty {difficulty})");
    }

    /// <summary>
    /// Clear encounter data after battle
    /// </summary>
    public void ClearEncounterData()
    {
        isWorldMapEncounter = false;
        encounterPlantType = PlantRecognitionSystem.PlantType.Sunflower;
        encounterElement = PlantRecognitionSystem.ElementType.Fire;
        encounterDisplayName = "";
        encounterDifficulty = 1;
        encounterFlavorText = "";
    }

    /// <summary>
    /// Get difficulty as a star string (★★★☆☆)
    /// </summary>
    public string GetDifficultyStars()
    {
        string stars = "";
        for (int i = 0; i < 5; i++)
        {
            stars += i < encounterDifficulty ? "★" : "☆";
        }
        return stars;
    }

    /// <summary>
    /// Get element as a colored string
    /// </summary>
    public string GetElementColorCode()
    {
        switch (encounterElement)
        {
            case PlantRecognitionSystem.ElementType.Fire:
                return "#FF5555"; // Red
            case PlantRecognitionSystem.ElementType.Water:
                return "#5599FF"; // Blue
            case PlantRecognitionSystem.ElementType.Grass:
                return "#55FF55"; // Green
            default:
                return "#FFFFFF"; // White
        }
    }
}
