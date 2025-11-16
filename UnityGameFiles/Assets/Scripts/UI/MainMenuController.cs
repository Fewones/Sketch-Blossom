using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using SketchBlossom.Progression;

namespace SketchBlossom.UI
{
    /// <summary>
    /// Controller for the Main Menu
    /// Handles navigation to different scenes based on game state
    /// </summary>
    public class MainMenuController : MonoBehaviour
    {
        [Header("Menu Buttons")]
        [SerializeField] private Button startButton;
        [SerializeField] private Button inventoryButton;
        [SerializeField] private Button settingsButton;
        [SerializeField] private Button quitButton;

        [Header("Status Display")]
        [SerializeField] private TextMeshProUGUI plantCountText;
        [SerializeField] private GameObject newGamePrompt;
        [SerializeField] private GameObject continuePrompt;

        [Header("Panels")]
        [SerializeField] private SettingsPanel settingsPanel;

        private void Start()
        {
            SetupButtons();
            UpdateUI();
        }

        private void SetupButtons()
        {
            if (startButton != null)
                startButton.onClick.AddListener(OnStartButtonClicked);

            if (inventoryButton != null)
                inventoryButton.onClick.AddListener(OnInventoryButtonClicked);

            if (settingsButton != null)
                settingsButton.onClick.AddListener(OnSettingsButtonClicked);

            if (quitButton != null)
                quitButton.onClick.AddListener(OnQuitButtonClicked);
        }

        /// <summary>
        /// Updates the UI based on current game state
        /// </summary>
        private void UpdateUI()
        {
            PlayerInventory inventory = PlayerInventory.Instance;
            int plantCount = 0;
            bool hasPlants = false;

            if (inventory != null)
            {
                plantCount = inventory.GetPlantCount();
                hasPlants = plantCount > 0;
            }

            // Update plant count display
            if (plantCountText != null)
            {
                if (hasPlants)
                {
                    plantCountText.text = $"Plants Collected: {plantCount}";
                    plantCountText.gameObject.SetActive(true);
                }
                else
                {
                    plantCountText.gameObject.SetActive(false);
                }
            }

            // Update prompts
            if (newGamePrompt != null)
                newGamePrompt.SetActive(!hasPlants);

            if (continuePrompt != null)
                continuePrompt.SetActive(hasPlants);

            // Update button states
            if (inventoryButton != null)
                inventoryButton.interactable = hasPlants;
        }

        /// <summary>
        /// Handles start button click
        /// Goes to DrawingScene if no plants, PlantSelectionScene if player has plants
        /// </summary>
        private void OnStartButtonClicked()
        {
            PlayerInventory inventory = PlayerInventory.Instance;
            bool hasPlants = inventory != null && inventory.GetPlantCount() > 0;

            if (hasPlants)
            {
                Debug.Log("Player has plants - going to PlantSelectionScene");
                SceneManager.LoadScene("PlantSelectionScene");
            }
            else
            {
                Debug.Log("New game - going to DrawingScene");
                SceneManager.LoadScene("DrawingScene");
            }
        }

        /// <summary>
        /// Opens the inventory scene
        /// </summary>
        private void OnInventoryButtonClicked()
        {
            Debug.Log("Opening inventory");
            SceneManager.LoadScene("InventoryScene");
        }

        /// <summary>
        /// Opens the settings panel
        /// </summary>
        private void OnSettingsButtonClicked()
        {
            Debug.Log("Settings clicked");

            if (settingsPanel != null)
            {
                settingsPanel.Show();
            }
            else
            {
                Debug.LogWarning("Settings panel reference not set in MainMenuController");
            }
        }

        /// <summary>
        /// Quits the game
        /// </summary>
        private void OnQuitButtonClicked()
        {
            Debug.Log("Quitting game");

            #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
            #else
                Application.Quit();
            #endif
        }

        /// <summary>
        /// Debug function to reset game progress
        /// Can be called from a hidden debug button or console command
        /// </summary>
        public void ResetGameProgress()
        {
            if (PlayerInventory.Instance != null)
            {
                PlayerInventory.Instance.ClearInventory();
                Debug.Log("Game progress reset - inventory cleared");
                UpdateUI();
            }
        }

        /// <summary>
        /// Debug function to view inventory data
        /// </summary>
        public void DebugLogInventory()
        {
            if (PlayerInventory.Instance != null)
            {
                var plants = PlayerInventory.Instance.GetAllPlants();
                Debug.Log($"=== INVENTORY DEBUG ===");
                Debug.Log($"Total Plants: {plants.Count}");

                foreach (var plant in plants)
                {
                    Debug.Log($"- {plant.plantName} (Lv.{plant.level}) | HP:{plant.currentHealth}/{plant.maxHealth} ATK:{plant.attack} DEF:{plant.defense}");
                }

                var selected = PlayerInventory.Instance.GetSelectedPlant();
                if (selected != null)
                {
                    Debug.Log($"Selected Plant: {selected.plantName}");
                }
            }
            else
            {
                Debug.Log("PlayerInventory instance not found");
            }
        }

        private void OnDestroy()
        {
            // Clean up button listeners
            if (startButton != null)
                startButton.onClick.RemoveAllListeners();

            if (inventoryButton != null)
                inventoryButton.onClick.RemoveAllListeners();

            if (settingsButton != null)
                settingsButton.onClick.RemoveAllListeners();

            if (quitButton != null)
                quitButton.onClick.RemoveAllListeners();
        }
    }
}
