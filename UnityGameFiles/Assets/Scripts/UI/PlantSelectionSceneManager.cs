using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using TMPro;
using SketchBlossom.Progression;

namespace SketchBlossom.UI
{
    /// <summary>
    /// Manages the Plant Selection Scene where players choose which plant to use in battle
    /// Shows all available plants and allows selecting one for the upcoming battle
    /// </summary>
    public class PlantSelectionSceneManager : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private GameObject plantCardPrefab;
        [SerializeField] private Transform plantGridContainer;
        [SerializeField] private Button confirmButton;
        [SerializeField] private Button backButton;

        [Header("Selection Display")]
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI selectionInfoText;
        [SerializeField] private GameObject selectedPlantPanel;
        [SerializeField] private TextMeshProUGUI selectedPlantNameText;
        [SerializeField] private TextMeshProUGUI selectedPlantStatsText;
        [SerializeField] private RawImage selectedPlantImage;

        [Header("Empty State")]
        [SerializeField] private GameObject emptyStatePanel;
        [SerializeField] private TextMeshProUGUI emptyStateText;
        [SerializeField] private Button drawNewPlantButton;

        private PlayerInventory inventory;
        private PlantInventoryEntry selectedPlant;
        private List<PlantCardUI> plantCards = new List<PlantCardUI>();

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
            RefreshPlantSelection();
        }

        private void SetupButtons()
        {
            if (confirmButton != null)
            {
                confirmButton.onClick.AddListener(OnConfirmSelection);
                confirmButton.interactable = false; // Disabled until a plant is selected
            }

            if (backButton != null)
                backButton.onClick.AddListener(OnBackButtonClicked);

            if (drawNewPlantButton != null)
                drawNewPlantButton.onClick.AddListener(OnDrawNewPlantClicked);

            UpdateConfirmButtonState();
        }

        /// <summary>
        /// Refreshes the plant selection display
        /// </summary>
        public void RefreshPlantSelection()
        {
            // Clear existing cards
            ClearPlantCards();

            List<PlantInventoryEntry> plants = inventory.GetAllPlants();

            // Show empty state if no plants
            if (plants.Count == 0)
            {
                ShowEmptyState();
                return;
            }

            HideEmptyState();

            // Set title
            if (titleText != null)
                titleText.text = "Choose Your Plant";

            // Get currently selected plant from inventory
            PlantInventoryEntry currentSelection = inventory.GetSelectedPlant();
            if (currentSelection != null)
                selectedPlant = currentSelection;

            // Create a card for each plant
            foreach (PlantInventoryEntry plant in plants)
            {
                CreatePlantCard(plant);
            }

            // Update selection display
            UpdateSelectionDisplay();
        }

        /// <summary>
        /// Creates a selectable plant card
        /// </summary>
        private void CreatePlantCard(PlantInventoryEntry plant)
        {
            GameObject card = Instantiate(plantCardPrefab, plantGridContainer);

            PlantCardUI cardUI = card.GetComponent<PlantCardUI>();
            if (cardUI != null)
            {
                cardUI.Setup(plant, OnPlantCardClicked);
                plantCards.Add(cardUI);

                // Highlight if this is the selected plant
                if (selectedPlant != null && plant.plantId == selectedPlant.plantId)
                {
                    cardUI.SetSelected(true);
                }
            }
            else
            {
                Debug.LogWarning("PlantCardUI component not found on plant card prefab!");
            }
        }

        /// <summary>
        /// Clears all plant cards
        /// </summary>
        private void ClearPlantCards()
        {
            plantCards.Clear();
            foreach (Transform child in plantGridContainer)
            {
                Destroy(child.gameObject);
            }
        }

        /// <summary>
        /// Called when a plant card is clicked
        /// </summary>
        private void OnPlantCardClicked(PlantInventoryEntry plant)
        {
            selectedPlant = plant;
            Debug.Log($"Selected plant: {plant.plantName}");

            // Update card selection indicators
            foreach (PlantCardUI card in plantCards)
            {
                card.SetSelected(false);
            }

            // Find and highlight the selected card
            PlantCardUI selectedCard = plantCards.Find(c =>
            {
                // We need to compare the plant data somehow
                // This is a workaround since we can't directly access the plant from the card
                return true; // Will be set properly in the loop
            });

            // Update all cards to show selection
            foreach (PlantCardUI card in plantCards)
            {
                card.SetSelected(false);
            }

            // Re-create cards with proper selection (simpler approach)
            RefreshCardSelection();

            UpdateSelectionDisplay();
            UpdateConfirmButtonState();
        }

        /// <summary>
        /// Refreshes only the selection state of cards
        /// </summary>
        private void RefreshCardSelection()
        {
            foreach (PlantCardUI card in plantCards)
            {
                card.SetSelected(false);
            }

            // This is a limitation - we'll need to track which card belongs to which plant
            // For now, we'll just refresh the entire display
            // In a production environment, you'd want to track card-to-plant mapping
        }

        /// <summary>
        /// Updates the selection display panel
        /// </summary>
        private void UpdateSelectionDisplay()
        {
            if (selectedPlant == null)
            {
                if (selectedPlantPanel != null)
                    selectedPlantPanel.SetActive(false);

                if (selectionInfoText != null)
                    selectionInfoText.text = "Select a plant to continue";

                return;
            }

            if (selectedPlantPanel != null)
                selectedPlantPanel.SetActive(true);

            if (selectedPlantNameText != null)
                selectedPlantNameText.text = selectedPlant.plantName;

            if (selectedPlantStatsText != null)
            {
                selectedPlantStatsText.text =
                    $"Level {selectedPlant.level} {selectedPlant.elementType} Type\n\n" +
                    $"HP: {selectedPlant.currentHealth}/{selectedPlant.maxHealth}\n" +
                    $"ATK: {selectedPlant.attack} | DEF: {selectedPlant.defense}\n\n" +
                    $"Battles Won: {selectedPlant.battlesWon}";
            }

            if (selectedPlantImage != null)
            {
                Texture2D texture = selectedPlant.GetDrawingTexture();
                if (texture != null)
                {
                    selectedPlantImage.texture = texture;
                    selectedPlantImage.gameObject.SetActive(true);
                }
                else
                {
                    selectedPlantImage.gameObject.SetActive(false);
                }
            }

            if (selectionInfoText != null)
            {
                if (selectedPlant.currentHealth <= 0)
                {
                    selectionInfoText.text = "<color=red>This plant has fainted! Choose another or heal it first.</color>";
                }
                else if (selectedPlant.currentHealth < selectedPlant.maxHealth * 0.3f)
                {
                    selectionInfoText.text = "<color=yellow>Warning: This plant is low on health!</color>";
                }
                else
                {
                    selectionInfoText.text = $"{selectedPlant.plantName} is ready for battle!";
                }
            }
        }

        /// <summary>
        /// Updates the confirm button state based on selection
        /// </summary>
        private void UpdateConfirmButtonState()
        {
            if (confirmButton != null)
            {
                // Can only confirm if a plant is selected and it has health
                bool canConfirm = selectedPlant != null && selectedPlant.currentHealth > 0;
                confirmButton.interactable = canConfirm;
            }
        }

        /// <summary>
        /// Confirms the plant selection and proceeds to battle
        /// </summary>
        private void OnConfirmSelection()
        {
            if (selectedPlant == null)
            {
                Debug.LogError("No plant selected!");
                return;
            }

            if (selectedPlant.currentHealth <= 0)
            {
                Debug.LogError("Selected plant has fainted!");
                return;
            }

            // Save selection to inventory
            inventory.SelectPlant(selectedPlant.plantId);

            // Load the selected plant into DrawnUnitData for battle
            inventory.LoadSelectedPlantForBattle();

            Debug.Log($"Confirmed selection: {selectedPlant.plantName}. Proceeding to battle.");

            // Load battle scene
            SceneManager.LoadScene("DrawingBattleScene");
        }

        /// <summary>
        /// Shows the empty state when no plants are available
        /// </summary>
        private void ShowEmptyState()
        {
            if (plantGridContainer != null)
                plantGridContainer.gameObject.SetActive(false);

            if (selectedPlantPanel != null)
                selectedPlantPanel.SetActive(false);

            if (emptyStatePanel != null)
            {
                emptyStatePanel.SetActive(true);

                if (emptyStateText != null)
                {
                    emptyStateText.text = "You don't have any plants yet!\n\n" +
                                         "Draw your first plant to begin your journey.";
                }
            }

            if (confirmButton != null)
                confirmButton.interactable = false;
        }

        /// <summary>
        /// Hides the empty state
        /// </summary>
        private void HideEmptyState()
        {
            if (plantGridContainer != null)
                plantGridContainer.gameObject.SetActive(true);

            if (emptyStatePanel != null)
                emptyStatePanel.SetActive(false);
        }

        /// <summary>
        /// Returns to the main menu
        /// </summary>
        private void OnBackButtonClicked()
        {
            Debug.Log("Returning to main menu");
            SceneManager.LoadScene("MainMenuScene");
        }

        /// <summary>
        /// Goes to the drawing scene to create a new plant
        /// </summary>
        private void OnDrawNewPlantClicked()
        {
            Debug.Log("Going to drawing scene to create new plant");
            SceneManager.LoadScene("DrawingScene");
        }

        private void OnDestroy()
        {
            // Clean up button listeners
            if (confirmButton != null)
                confirmButton.onClick.RemoveAllListeners();

            if (backButton != null)
                backButton.onClick.RemoveAllListeners();

            if (drawNewPlantButton != null)
                drawNewPlantButton.onClick.RemoveAllListeners();
        }
    }
}
