using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SketchBlossom.Progression
{
    /// <summary>
    /// Singleton manager for the player's plant inventory
    /// Handles storing, loading, and managing all tamed plants
    /// Persists across game sessions using PlayerPrefs
    /// </summary>
    public class PlayerInventory : MonoBehaviour
    {
        public static PlayerInventory Instance { get; private set; }

        [Header("Inventory Data")]
        [SerializeField] private List<PlantInventoryEntry> plants = new List<PlantInventoryEntry>();

        [Header("Current Selection")]
        [SerializeField] private string selectedPlantId; // ID of the plant selected for next battle

        // Events for UI updates
        public event Action<PlantInventoryEntry> OnPlantAdded;
        public event Action<PlantInventoryEntry> OnPlantUpdated;
        public event Action<PlantInventoryEntry> OnPlantSelected;

        private const string INVENTORY_SAVE_KEY = "PlayerInventory_v1";
        private const string SELECTED_PLANT_KEY = "SelectedPlantId";

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

            LoadInventory();
        }

        #region Adding Plants

        /// <summary>
        /// Adds a new plant to the inventory from DrawnUnitData
        /// Called when player draws their first plant or tames a new plant
        /// </summary>
        public PlantInventoryEntry AddPlant(DrawnUnitData unitData)
        {
            // Create plant data from the recognition system
            PlantRecognitionSystem.PlantData plantData = new PlantRecognitionSystem.PlantData(
                unitData.plantType,
                unitData.element,
                unitData.plantDisplayName,
                unitData.health,
                unitData.attack,
                unitData.defense,
                ""
            );

            PlantInventoryEntry newEntry = new PlantInventoryEntry(
                plantData,
                unitData.unitColor,
                unitData.drawingTexture,
                unitData.detectionConfidence
            );

            plants.Add(newEntry);
            Debug.Log($"Added {newEntry.plantName} to inventory. Total plants: {plants.Count}");

            OnPlantAdded?.Invoke(newEntry);
            SaveInventory();

            return newEntry;
        }

        #endregion

        #region Getting Plants

        /// <summary>
        /// Gets all plants in the inventory
        /// </summary>
        public List<PlantInventoryEntry> GetAllPlants()
        {
            return new List<PlantInventoryEntry>(plants);
        }

        /// <summary>
        /// Gets a plant by its unique ID
        /// </summary>
        public PlantInventoryEntry GetPlantById(string plantId)
        {
            return plants.FirstOrDefault(p => p.plantId == plantId);
        }

        /// <summary>
        /// Gets the currently selected plant for battle
        /// </summary>
        public PlantInventoryEntry GetSelectedPlant()
        {
            if (string.IsNullOrEmpty(selectedPlantId))
                return plants.FirstOrDefault(); // Return first plant if none selected

            return GetPlantById(selectedPlantId);
        }

        /// <summary>
        /// Gets the total number of plants in inventory
        /// </summary>
        public int GetPlantCount()
        {
            return plants.Count;
        }

        /// <summary>
        /// Checks if the inventory is empty
        /// </summary>
        public bool IsEmpty()
        {
            return plants.Count == 0;
        }

        #endregion

        #region Updating Plants

        /// <summary>
        /// Selects a plant for the next battle
        /// </summary>
        public void SelectPlant(string plantId)
        {
            PlantInventoryEntry plant = GetPlantById(plantId);
            if (plant == null)
            {
                Debug.LogError($"Cannot select plant with ID {plantId} - not found in inventory");
                return;
            }

            selectedPlantId = plantId;
            PlayerPrefs.SetString(SELECTED_PLANT_KEY, selectedPlantId);
            PlayerPrefs.Save();

            Debug.Log($"Selected {plant.plantName} for battle");
            OnPlantSelected?.Invoke(plant);
        }

        /// <summary>
        /// Applies Wild Growth to a plant, upgrading its stats
        /// </summary>
        public void ApplyWildGrowth(string plantId)
        {
            PlantInventoryEntry plant = GetPlantById(plantId);
            if (plant == null)
            {
                Debug.LogError($"Cannot apply Wild Growth to plant with ID {plantId} - not found");
                return;
            }

            plant.ApplyWildGrowth();
            OnPlantUpdated?.Invoke(plant);
            SaveInventory();
        }

        /// <summary>
        /// Records a victory for a plant
        /// </summary>
        public void RecordVictory(string plantId)
        {
            PlantInventoryEntry plant = GetPlantById(plantId);
            if (plant == null)
            {
                Debug.LogError($"Cannot record victory for plant with ID {plantId} - not found");
                return;
            }

            plant.RecordVictory();
            OnPlantUpdated?.Invoke(plant);
            SaveInventory();
        }

        /// <summary>
        /// Heals a plant to full health
        /// </summary>
        public void HealPlant(string plantId)
        {
            PlantInventoryEntry plant = GetPlantById(plantId);
            if (plant == null)
            {
                Debug.LogError($"Cannot heal plant with ID {plantId} - not found");
                return;
            }

            plant.HealToFull();
            OnPlantUpdated?.Invoke(plant);
            SaveInventory();
        }

        /// <summary>
        /// Heals all plants to full health
        /// </summary>
        public void HealAllPlants()
        {
            foreach (var plant in plants)
            {
                plant.HealToFull();
            }
            SaveInventory();
            Debug.Log("All plants healed to full health");
        }

        /// <summary>
        /// Updates plant stats after battle
        /// </summary>
        public void UpdatePlantStats(string plantId, int currentHealth)
        {
            PlantInventoryEntry plant = GetPlantById(plantId);
            if (plant == null)
            {
                Debug.LogError($"Cannot update stats for plant with ID {plantId} - not found");
                return;
            }

            plant.currentHealth = Mathf.Max(0, currentHealth);
            OnPlantUpdated?.Invoke(plant);
            SaveInventory();
        }

        /// <summary>
        /// Removes a plant from the inventory (permanent death in rogue-like)
        /// </summary>
        public void RemovePlant(string plantId)
        {
            PlantInventoryEntry plant = GetPlantById(plantId);
            if (plant == null)
            {
                Debug.LogError($"Cannot remove plant with ID {plantId} - not found");
                return;
            }

            plants.Remove(plant);

            // Clear selection if this was the selected plant
            if (selectedPlantId == plantId)
            {
                selectedPlantId = "";
            }

            Debug.Log($"Removed {plant.plantName} from inventory (permanent death). Remaining plants: {plants.Count}");
            SaveInventory();
        }

        #endregion

        #region Applying to Battle

        /// <summary>
        /// Loads the selected plant into DrawnUnitData for battle
        /// Call this before entering battle to use the selected plant
        /// </summary>
        public void LoadSelectedPlantForBattle()
        {
            PlantInventoryEntry selectedPlant = GetSelectedPlant();
            if (selectedPlant == null)
            {
                Debug.LogError("No plant selected for battle!");
                return;
            }

            if (DrawnUnitData.Instance == null)
            {
                Debug.LogError("DrawnUnitData.Instance is null! Cannot load plant for battle.");
                return;
            }

            // Apply stats/visuals to battle unit
            selectedPlant.ApplyToDrawnUnitData(DrawnUnitData.Instance);

            // 🔗 Remember which inventory entry this battle unit came from
            DrawnUnitData.Instance.inventoryPlantId = selectedPlant.plantId;

            Debug.Log($"Loaded {selectedPlant.plantName} for battle (ATK:{selectedPlant.attack} HP:{selectedPlant.maxHealth} DEF:{selectedPlant.defense})");
        }

        /// <summary>
        /// Loads a specific plant into DrawnUnitData for battle
        /// </summary>
        public void LoadPlantForBattle(string plantId)
        {
            SelectPlant(plantId);
            LoadSelectedPlantForBattle();
        }

        #endregion

        #region Save/Load

        /// <summary>
        /// Saves the inventory to PlayerPrefs as JSON
        /// </summary>
        public void SaveInventory()
        {
            try
            {
                InventorySaveData saveData = new InventorySaveData
                {
                    plants = plants,
                    selectedPlantId = selectedPlantId
                };

                string json = JsonUtility.ToJson(saveData, true);
                PlayerPrefs.SetString(INVENTORY_SAVE_KEY, json);
                PlayerPrefs.Save();

                Debug.Log($"Inventory saved: {plants.Count} plants");
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to save inventory: {e.Message}");
            }
        }

        /// <summary>
        /// Loads the inventory from PlayerPrefs
        /// </summary>
        public void LoadInventory()
        {
            try
            {
                if (!PlayerPrefs.HasKey(INVENTORY_SAVE_KEY))
                {
                    Debug.Log("No saved inventory found. Starting with empty inventory.");
                    plants = new List<PlantInventoryEntry>();
                    selectedPlantId = "";
                    return;
                }

                string json = PlayerPrefs.GetString(INVENTORY_SAVE_KEY);
                InventorySaveData saveData = JsonUtility.FromJson<InventorySaveData>(json);

                plants = saveData.plants ?? new List<PlantInventoryEntry>();
                selectedPlantId = saveData.selectedPlantId ?? "";

                // Also load selected plant ID from separate key (backward compatibility)
                if (string.IsNullOrEmpty(selectedPlantId) && PlayerPrefs.HasKey(SELECTED_PLANT_KEY))
                {
                    selectedPlantId = PlayerPrefs.GetString(SELECTED_PLANT_KEY);
                }

                Debug.Log($"Inventory loaded: {plants.Count} plants");
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to load inventory: {e.Message}");
                plants = new List<PlantInventoryEntry>();
                selectedPlantId = "";
            }
        }

        /// <summary>
        /// Clears all plants from the inventory (for testing/reset)
        /// </summary>
        public void ClearInventory()
        {
            plants.Clear();
            selectedPlantId = "";
            SaveInventory();
            Debug.Log("Inventory cleared");
        }

        #endregion

        #region Helper Classes

        [System.Serializable]
        private class InventorySaveData
        {
            public List<PlantInventoryEntry> plants;
            public string selectedPlantId;
        }

        #endregion

        void OnApplicationQuit()
        {
            ClearInventory();
        }
    }
}
