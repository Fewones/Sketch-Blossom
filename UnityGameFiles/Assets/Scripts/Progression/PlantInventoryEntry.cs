using System;
using UnityEngine;

namespace SketchBlossom.Progression
{
    /// <summary>
    /// Represents a single plant in the player's inventory
    /// Stores all data needed to recreate the plant in battle or display it in the inventory
    /// </summary>
    [System.Serializable]
    public class PlantInventoryEntry
    {
        // Unique identifier for this plant instance
        public string plantId;

        // Plant type and element
        public PlantRecognitionSystem.PlantType plantType;
        public PlantRecognitionSystem.ElementType elementType;

        // Display name
        public string plantName;

        // Current stats (can be upgraded via Wild Growth)
        public int maxHealth;
        public int attack;
        public int defense;
        public int currentHealth;

        // Plant level (increases with Wild Growth)
        public int level;

        // Visual data
        public Color plantColor;
        public string drawingTextureBase64; // Store the drawing texture as base64 string

        // Metadata
        public DateTime acquiredDate;
        public float originalConfidence; // Original detection confidence
        public int battlesWon;
        public int wildGrowthCount; // How many times this plant has been upgraded

        // Constructor for new plants
        public PlantInventoryEntry(
            PlantRecognitionSystem.PlantData plantData,
            Color color,
            Texture2D drawingTexture = null,
            float confidence = 1f)
        {
            plantId = System.Guid.NewGuid().ToString();
            plantType = plantData.type;
            elementType = plantData.element;
            plantName = plantData.displayName;

            maxHealth = plantData.baseHP;
            attack = plantData.baseAttack;
            defense = plantData.baseDefense;
            currentHealth = maxHealth;

            level = 1;
            plantColor = color;

            if (drawingTexture != null)
            {
                drawingTextureBase64 = TextureToBase64(drawingTexture);
            }

            acquiredDate = DateTime.Now;
            originalConfidence = confidence;
            battlesWon = 0;
            wildGrowthCount = 0;
        }

        // Constructor for loading from saved data
        public PlantInventoryEntry()
        {
            plantId = System.Guid.NewGuid().ToString();
            acquiredDate = DateTime.Now;
        }

        /// <summary>
        /// Applies Wild Growth upgrade to this plant
        /// Increases stats and level
        /// </summary>
        public void ApplyWildGrowth()
        {
            wildGrowthCount++;
            level++;

            // Increase stats based on element type
            switch (elementType)
            {
                case PlantRecognitionSystem.ElementType.Fire:
                    // Fire plants get more attack
                    attack += 5;
                    maxHealth += 3;
                    defense += 2;
                    break;

                case PlantRecognitionSystem.ElementType.Grass:
                    // Grass plants get balanced growth
                    attack += 3;
                    maxHealth += 4;
                    defense += 3;
                    break;

                case PlantRecognitionSystem.ElementType.Water:
                    // Water plants get more defense and health
                    attack += 2;
                    maxHealth += 5;
                    defense += 4;
                    break;
            }

            // Heal to full health after upgrade
            currentHealth = maxHealth;

            Debug.Log($"Wild Growth applied to {plantName}! Now level {level} with ATK:{attack} HP:{maxHealth} DEF:{defense}");
        }

        /// <summary>
        /// Heals the plant to full health
        /// </summary>
        public void HealToFull()
        {
            currentHealth = maxHealth;
        }

        /// <summary>
        /// Records a battle victory for this plant
        /// </summary>
        public void RecordVictory()
        {
            battlesWon++;
        }

        /// <summary>
        /// Converts a Texture2D to a base64 string for serialization
        /// </summary>
        private string TextureToBase64(Texture2D texture)
        {
            try
            {
                byte[] imageBytes = texture.EncodeToPNG();
                return Convert.ToBase64String(imageBytes);
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to encode texture to base64: {e.Message}");
                return "";
            }
        }

        /// <summary>
        /// Converts a base64 string back to a Texture2D
        /// </summary>
        public Texture2D GetDrawingTexture()
        {
            if (string.IsNullOrEmpty(drawingTextureBase64))
                return null;

            try
            {
                byte[] imageBytes = Convert.FromBase64String(drawingTextureBase64);
                Texture2D texture = new Texture2D(2, 2);
                texture.LoadImage(imageBytes);
                return texture;
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to decode base64 to texture: {e.Message}");
                return null;
            }
        }

        /// <summary>
        /// Creates a DrawnUnitData instance from this inventory entry
        /// Used when loading a plant for battle
        /// </summary>
        public void ApplyToDrawnUnitData(DrawnUnitData unitData)
        {
            unitData.attack = attack;
            unitData.defense = defense;
            unitData.health = maxHealth;
            unitData.plantType = plantType;
            unitData.element = elementType;
            unitData.plantDisplayName = plantName;
            unitData.unitColor = plantColor;
            unitData.detectionConfidence = originalConfidence;

            // Load the drawing texture if available
            Texture2D texture = GetDrawingTexture();
            if (texture != null)
            {
                unitData.drawingTexture = texture;
            }
        }

        /// <summary>
        /// Gets a formatted summary of the plant's stats
        /// </summary>
        public string GetStatsSummary()
        {
            return $"Level {level} {plantName}\n" +
                   $"HP: {currentHealth}/{maxHealth}\n" +
                   $"ATK: {attack} | DEF: {defense}\n" +
                   $"Element: {elementType}\n" +
                   $"Battles Won: {battlesWon}";
        }
    }
}
