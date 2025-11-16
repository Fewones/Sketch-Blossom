using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using TMPro;
using SketchBlossom.Progression;

namespace SketchBlossom.UI
{
    /// <summary>
    /// Manages the Inventory Scene where players can view all their plants
    /// Displays plant cards in a grid and shows detailed info when a plant is clicked
    /// </summary>
    public class InventorySceneManager : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private GameObject plantCardPrefab;
        [SerializeField] private Transform plantGridContainer;
        [SerializeField] private GameObject detailPanel;
        [SerializeField] private Button backButton;

        [Header("Detail Panel Elements")]
        [SerializeField] private TextMeshProUGUI detailNameText;
        [SerializeField] private TextMeshProUGUI detailStatsText;
        [SerializeField] private TextMeshProUGUI detailInfoText;
        [SerializeField] private RawImage detailPlantImage;
        [SerializeField] private Image detailBackgroundImage;
        [SerializeField] private Button closeDetailButton;

        [Header("Empty State")]
        [SerializeField] private GameObject emptyStatePanel;
        [SerializeField] private TextMeshProUGUI emptyStateText;

        private PlayerInventory inventory;
        private PlantInventoryEntry currentlyViewedPlant;

        private void Start()
        {
            // Get inventory instance
            inventory = PlayerInventory.Instance;
            if (inventory == null)
            {
                Debug.LogError("PlayerInventory instance not found!");
                return;
            }

            // Setup UI
            SetupButtons();
            HideDetailPanel();
            RefreshInventoryDisplay();
        }

        private void SetupButtons()
        {
            if (backButton != null)
                backButton.onClick.AddListener(OnBackButtonClicked);

            if (closeDetailButton != null)
                closeDetailButton.onClick.AddListener(HideDetailPanel);
        }

        /// <summary>
        /// Refreshes the inventory display with all plants
        /// </summary>
        public void RefreshInventoryDisplay()
        {
            // Clear existing cards
            foreach (Transform child in plantGridContainer)
            {
                Destroy(child.gameObject);
            }

            List<PlantInventoryEntry> plants = inventory.GetAllPlants();

            // Show empty state if no plants
            if (plants.Count == 0)
            {
                ShowEmptyState();
                return;
            }

            HideEmptyState();

            // Create a card for each plant
            foreach (PlantInventoryEntry plant in plants)
            {
                CreatePlantCard(plant);
            }
        }

        /// <summary>
        /// Creates a UI card for a plant in the grid
        /// </summary>
        private void CreatePlantCard(PlantInventoryEntry plant)
        {
            GameObject card = Instantiate(plantCardPrefab, plantGridContainer);

            // Get card components
            PlantCardUI cardUI = card.GetComponent<PlantCardUI>();
            if (cardUI != null)
            {
                cardUI.Setup(plant, OnPlantCardClicked);
            }
            else
            {
                // Fallback if PlantCardUI component doesn't exist
                SetupBasicCard(card, plant);
            }
        }

        /// <summary>
        /// Fallback method to setup a basic card without the PlantCardUI component
        /// </summary>
        private void SetupBasicCard(GameObject card, PlantInventoryEntry plant)
        {
            // Find child elements (assuming basic structure)
            TextMeshProUGUI nameText = card.transform.Find("NameText")?.GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI levelText = card.transform.Find("LevelText")?.GetComponent<TextMeshProUGUI>();
            RawImage plantImage = card.transform.Find("PlantImage")?.GetComponent<RawImage>();
            Image background = card.GetComponent<Image>();
            Button cardButton = card.GetComponent<Button>();

            if (nameText != null)
                nameText.text = plant.plantName;

            if (levelText != null)
                levelText.text = $"Lv. {plant.level}";

            if (plantImage != null && plant.GetDrawingTexture() != null)
                plantImage.texture = plant.GetDrawingTexture();

            if (background != null)
                background.color = plant.plantColor * 0.5f;

            if (cardButton != null)
                cardButton.onClick.AddListener(() => OnPlantCardClicked(plant));
        }

        /// <summary>
        /// Called when a plant card is clicked
        /// </summary>
        private void OnPlantCardClicked(PlantInventoryEntry plant)
        {
            ShowDetailPanel(plant);
        }

        /// <summary>
        /// Shows the detail panel with plant information
        /// </summary>
        private void ShowDetailPanel(PlantInventoryEntry plant)
        {
            currentlyViewedPlant = plant;

            if (detailPanel != null)
                detailPanel.SetActive(true);

            // Set name
            if (detailNameText != null)
                detailNameText.text = plant.plantName;

            // Set stats
            if (detailStatsText != null)
            {
                detailStatsText.text = $"Level {plant.level}\n\n" +
                                      $"<b>HP:</b> {plant.currentHealth}/{plant.maxHealth}\n" +
                                      $"<b>Attack:</b> {plant.attack}\n" +
                                      $"<b>Defense:</b> {plant.defense}";
            }

            // Set additional info
            if (detailInfoText != null)
            {
                detailInfoText.text = $"<b>Element:</b> {plant.elementType}\n" +
                                     $"<b>Type:</b> {plant.plantType}\n\n" +
                                     $"<b>Battles Won:</b> {plant.battlesWon}\n" +
                                     $"<b>Wild Growth:</b> {plant.wildGrowthCount}x\n\n" +
                                     $"<b>Acquired:</b> {plant.acquiredDate:MM/dd/yyyy}";
            }

            // Set plant image
            if (detailPlantImage != null)
            {
                Texture2D texture = plant.GetDrawingTexture();
                if (texture != null)
                {
                    detailPlantImage.texture = texture;
                    detailPlantImage.gameObject.SetActive(true);
                }
                else
                {
                    detailPlantImage.gameObject.SetActive(false);
                }
            }

            // Set background color based on element
            if (detailBackgroundImage != null)
            {
                detailBackgroundImage.color = GetElementColor(plant.elementType);
            }

            Debug.Log($"Viewing details for {plant.plantName}");
        }

        /// <summary>
        /// Hides the detail panel
        /// </summary>
        private void HideDetailPanel()
        {
            if (detailPanel != null)
                detailPanel.SetActive(false);

            currentlyViewedPlant = null;
        }

        /// <summary>
        /// Shows the empty state panel when inventory is empty
        /// </summary>
        private void ShowEmptyState()
        {
            if (emptyStatePanel != null)
            {
                emptyStatePanel.SetActive(true);

                if (emptyStateText != null)
                {
                    emptyStateText.text = "Your garden is empty!\n\n" +
                                         "Draw your first plant to begin your journey.";
                }
            }
        }

        /// <summary>
        /// Hides the empty state panel
        /// </summary>
        private void HideEmptyState()
        {
            if (emptyStatePanel != null)
                emptyStatePanel.SetActive(false);
        }

        /// <summary>
        /// Gets a color based on the element type
        /// </summary>
        private Color GetElementColor(PlantRecognitionSystem.ElementType element)
        {
            switch (element)
            {
                case PlantRecognitionSystem.ElementType.Fire:
                    return new Color(1f, 0.4f, 0.3f, 0.3f); // Red tint
                case PlantRecognitionSystem.ElementType.Water:
                    return new Color(0.3f, 0.6f, 1f, 0.3f); // Blue tint
                case PlantRecognitionSystem.ElementType.Grass:
                    return new Color(0.4f, 0.9f, 0.4f, 0.3f); // Green tint
                default:
                    return new Color(0.8f, 0.8f, 0.8f, 0.3f); // Gray
            }
        }

        /// <summary>
        /// Returns to the main menu
        /// </summary>
        private void OnBackButtonClicked()
        {
            Debug.Log("Returning to main menu from inventory");
            SceneManager.LoadScene("MainMenuScene");
        }

        private void OnDestroy()
        {
            // Clean up button listeners
            if (backButton != null)
                backButton.onClick.RemoveAllListeners();

            if (closeDetailButton != null)
                closeDetailButton.onClick.RemoveAllListeners();
        }
    }
}
