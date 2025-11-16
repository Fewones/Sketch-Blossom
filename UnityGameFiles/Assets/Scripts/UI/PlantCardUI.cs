using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using SketchBlossom.Progression;

namespace SketchBlossom.UI
{
    /// <summary>
    /// UI component for a single plant card in the inventory or selection screen
    /// </summary>
    public class PlantCardUI : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private TextMeshProUGUI levelText;
        [SerializeField] private TextMeshProUGUI statsText;
        [SerializeField] private RawImage plantImage;
        [SerializeField] private Image backgroundImage;
        [SerializeField] private Image elementIcon;
        [SerializeField] private GameObject selectedIndicator;
        [SerializeField] private Button cardButton;

        [Header("Health Bar")]
        [SerializeField] private Slider healthBar;
        [SerializeField] private TextMeshProUGUI healthText;

        private PlantInventoryEntry plantData;
        private Action<PlantInventoryEntry> onClickCallback;

        private void Awake()
        {
            if (cardButton == null)
                cardButton = GetComponent<Button>();

            if (cardButton != null)
                cardButton.onClick.AddListener(OnCardClicked);
        }

        /// <summary>
        /// Sets up the card with plant data
        /// </summary>
        public void Setup(PlantInventoryEntry plant, Action<PlantInventoryEntry> onClick = null)
        {
            plantData = plant;
            onClickCallback = onClick;

            UpdateDisplay();
        }

        /// <summary>
        /// Updates the card display with current plant data
        /// </summary>
        public void UpdateDisplay()
        {
            if (plantData == null)
                return;

            // Set name
            if (nameText != null)
                nameText.text = plantData.plantName;

            // Set level
            if (levelText != null)
                levelText.text = $"Lv. {plantData.level}";

            // Set stats
            if (statsText != null)
            {
                statsText.text = $"ATK: {plantData.attack}  DEF: {plantData.defense}";
            }

            // Set health bar
            if (healthBar != null)
            {
                healthBar.maxValue = plantData.maxHealth;
                healthBar.value = plantData.currentHealth;
            }

            if (healthText != null)
            {
                healthText.text = $"{plantData.currentHealth}/{plantData.maxHealth} HP";
            }

            // Set plant image
            if (plantImage != null)
            {
                Texture2D texture = plantData.GetDrawingTexture();
                if (texture != null)
                {
                    plantImage.texture = texture;
                    plantImage.color = Color.white;
                }
                else
                {
                    // Show a colored square if no texture
                    plantImage.color = plantData.plantColor;
                }
            }

            // Set background color based on element
            if (backgroundImage != null)
            {
                backgroundImage.color = GetElementColor(plantData.elementType);
            }

            // Set element icon color (if you have element icons)
            if (elementIcon != null)
            {
                elementIcon.color = GetElementIconColor(plantData.elementType);
            }
        }

        /// <summary>
        /// Shows or hides the selected indicator
        /// </summary>
        public void SetSelected(bool isSelected)
        {
            if (selectedIndicator != null)
                selectedIndicator.SetActive(isSelected);
        }

        /// <summary>
        /// Called when the card is clicked
        /// </summary>
        private void OnCardClicked()
        {
            if (plantData != null)
            {
                Debug.Log($"Plant card clicked: {plantData.plantName}");
                onClickCallback?.Invoke(plantData);
            }
        }

        /// <summary>
        /// Gets a background color based on the element type
        /// </summary>
        private Color GetElementColor(PlantRecognitionSystem.ElementType element)
        {
            switch (element)
            {
                case PlantRecognitionSystem.ElementType.Fire:
                    return new Color(1f, 0.4f, 0.3f, 0.8f); // Red
                case PlantRecognitionSystem.ElementType.Water:
                    return new Color(0.3f, 0.6f, 1f, 0.8f); // Blue
                case PlantRecognitionSystem.ElementType.Grass:
                    return new Color(0.4f, 0.9f, 0.4f, 0.8f); // Green
                default:
                    return new Color(0.7f, 0.7f, 0.7f, 0.8f); // Gray
            }
        }

        /// <summary>
        /// Gets a more saturated color for element icons
        /// </summary>
        private Color GetElementIconColor(PlantRecognitionSystem.ElementType element)
        {
            switch (element)
            {
                case PlantRecognitionSystem.ElementType.Fire:
                    return new Color(1f, 0.2f, 0.1f, 1f); // Bright Red
                case PlantRecognitionSystem.ElementType.Water:
                    return new Color(0.1f, 0.5f, 1f, 1f); // Bright Blue
                case PlantRecognitionSystem.ElementType.Grass:
                    return new Color(0.2f, 1f, 0.2f, 1f); // Bright Green
                default:
                    return Color.white;
            }
        }

        private void OnDestroy()
        {
            if (cardButton != null)
                cardButton.onClick.RemoveAllListeners();
        }
    }
}
