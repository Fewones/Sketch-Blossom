using UnityEngine;
using System.Collections.Generic;

public class PlantInventory : MonoBehaviour
{
    public static PlantInventory Instance { get; private set; }

    [Header("All Plants")]
    public List<PlantData> plants = new List<PlantData>();

    [Header("Selection")]
    public PlantData selectedPlantForUpgrade;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        // TEMP: create some dummy plants if inventory is empty (for testing)
        if (plants.Count == 0)
        {
            plants.Add(new PlantData("Starter Plant", 50, 10, 5, "Grass"));
            plants.Add(new PlantData("Spiky Cactus", 40, 15, 3, "Fire"));
            plants.Add(new PlantData("Water Lily", 60, 8, 7, "Water"));
        }
    }
}
