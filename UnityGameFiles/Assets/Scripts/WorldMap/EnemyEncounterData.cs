using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Stores data about a single enemy plant within an encounter
/// </summary>
[System.Serializable]
public class EnemyPlantEntry
{
    public PlantRecognitionSystem.PlantType plantType;
    public PlantRecognitionSystem.ElementType element;
    public string displayName;
    public int artVariant; // 0, 1, or 2 - which of the 3 art variations to use

    public EnemyPlantEntry(PlantRecognitionSystem.PlantType type, PlantRecognitionSystem.ElementType elem, string name, int variant)
    {
        plantType = type;
        element = elem;
        displayName = name;
        artVariant = variant;
    }
}

/// <summary>
/// Singleton that stores data about the selected enemy encounter on the world map
/// Persists between scene transitions to pass enemy data to the battle scene
/// Supports multi-plant encounters (difficulty 1 = 1 plant, difficulty 2 = 2 plants, difficulty 3 = 3 plants)
/// </summary>
public class EnemyEncounterData : MonoBehaviour
{
    public static EnemyEncounterData Instance { get; private set; }

    [Header("Enemy Data (Legacy - first plant)")]
    public PlantRecognitionSystem.PlantType encounterPlantType;
    public PlantRecognitionSystem.ElementType encounterElement;
    public string encounterDisplayName;
    public int encounterDifficulty;
    public string encounterFlavorText;

    [Header("Multi-Plant Encounter")]
    public List<EnemyPlantEntry> encounterPlants = new List<EnemyPlantEntry>();
    public int currentPlantIndex = 0; // Which plant the player is currently fighting

    [Header("Battle Context")]
    public bool isWorldMapEncounter = false;
    public Vector2Int currentWorldMapIndex;

    private void Awake()
    {
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
    /// Store encounter data with multiple plants based on difficulty level
    /// </summary>
    public void SetEncounterData(
        List<EnemyPlantEntry> plants,
        int difficulty,
        string flavorText,
        Vector2Int worldMapIndex)
    {
        encounterPlants = new List<EnemyPlantEntry>(plants);
        encounterDifficulty = difficulty;
        encounterFlavorText = flavorText;
        currentPlantIndex = 0;
        isWorldMapEncounter = true;
        currentWorldMapIndex = worldMapIndex;

        // Set legacy fields to the first plant for backward compatibility
        if (plants.Count > 0)
        {
            encounterPlantType = plants[0].plantType;
            encounterElement = plants[0].element;
            encounterDisplayName = plants[0].displayName;
        }

        Debug.Log($"Enemy encounter set: Difficulty {difficulty} with {plants.Count} plant(s)");
        for (int i = 0; i < plants.Count; i++)
        {
            Debug.Log($"  Plant {i + 1}: {plants[i].displayName} ({plants[i].element}) [Art Variant {plants[i].artVariant + 1}]");
        }
    }

    /// <summary>
    /// Legacy single-plant setter for backward compatibility
    /// </summary>
    public void SetEncounterData(
        PlantRecognitionSystem.PlantType plantType,
        PlantRecognitionSystem.ElementType element,
        string displayName,
        int difficulty,
        string flavorText)
    {
        encounterPlants.Clear();
        int artVariant = Random.Range(0, 3);
        encounterPlants.Add(new EnemyPlantEntry(plantType, element, displayName, artVariant));

        encounterPlantType = plantType;
        encounterElement = element;
        encounterDisplayName = displayName;
        encounterDifficulty = difficulty;
        encounterFlavorText = flavorText;
        currentPlantIndex = 0;
        isWorldMapEncounter = true;

        Debug.Log($"Enemy encounter set: {displayName} (Difficulty {difficulty})");
    }

    /// <summary>
    /// Get the current enemy plant being fought
    /// </summary>
    public EnemyPlantEntry GetCurrentPlant()
    {
        if (encounterPlants.Count == 0 || currentPlantIndex >= encounterPlants.Count)
            return null;
        return encounterPlants[currentPlantIndex];
    }

    /// <summary>
    /// Advance to the next plant in the encounter. Returns true if there is a next plant.
    /// </summary>
    public bool AdvanceToNextPlant()
    {
        currentPlantIndex++;
        if (currentPlantIndex < encounterPlants.Count)
        {
            // Update legacy fields
            var plant = encounterPlants[currentPlantIndex];
            encounterPlantType = plant.plantType;
            encounterElement = plant.element;
            encounterDisplayName = plant.displayName;
            Debug.Log($"Advancing to next enemy plant: {plant.displayName} ({currentPlantIndex + 1}/{encounterPlants.Count})");
            return true;
        }
        return false;
    }

    /// <summary>
    /// Check if there are more plants to fight after the current one
    /// </summary>
    public bool HasMorePlants()
    {
        return currentPlantIndex < encounterPlants.Count - 1;
    }

    /// <summary>
    /// Get total number of plants in this encounter
    /// </summary>
    public int GetPlantCount()
    {
        return encounterPlants.Count;
    }

    /// <summary>
    /// Get remaining plants to fight (including current)
    /// </summary>
    public int GetRemainingPlantCount()
    {
        return encounterPlants.Count - currentPlantIndex;
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
        encounterPlants.Clear();
        currentPlantIndex = 0;
    }

    /// <summary>
    /// Get difficulty as a star string (★★★)
    /// </summary>
    public string GetDifficultyStars()
    {
        string stars = "";
        for (int i = 0; i < 3; i++)
        {
            stars += i < encounterDifficulty ? "★" : "☆";
        }
        return stars;
    }

    /// <summary>
    /// Get a display label for the difficulty level
    /// </summary>
    public string GetDifficultyLabel()
    {
        switch (encounterDifficulty)
        {
            case 1: return "Easy";
            case 2: return "Medium";
            case 3: return "Hard";
            default: return "Unknown";
        }
    }

    /// <summary>
    /// Get element as a colored string
    /// </summary>
    public string GetElementColorCode()
    {
        switch (encounterElement)
        {
            case PlantRecognitionSystem.ElementType.Fire:
                return "#FF5555";
            case PlantRecognitionSystem.ElementType.Water:
                return "#5599FF";
            case PlantRecognitionSystem.ElementType.Grass:
                return "#55FF55";
            default:
                return "#FFFFFF";
        }
    }
}
