using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

namespace SketchBlossom.UI
{
    /// <summary>
    /// Pause menu that can be added to any scene
    /// Provides options to resume, adjust settings, or return to main menu
    /// Activated by pressing ESC key
    /// </summary>
    public class PauseMenuManager : MonoBehaviour
    {
        [Header("Panels")]
        [SerializeField] private GameObject pauseMenuOverlay;
        [SerializeField] private GameObject pauseMenuWindow;
        [SerializeField] private SettingsPanel settingsPanel;
        [SerializeField] private ConfirmationDialog confirmationDialog;

        [Header("Buttons")]
        [SerializeField] private Button resumeButton;
        [SerializeField] private Button settingsButton;
        [SerializeField] private Button mainMenuButton;
        [SerializeField] private Button quitButton;

        [Header("Settings")]
        [SerializeField] private KeyCode pauseKey = KeyCode.Escape;
        [SerializeField] private bool pauseTimeWhenOpen = true;

        private bool isPaused = false;
        private bool settingsOpen = false;

        private void Start()
        {
            SetupButtons();
            Hide();
        }

        private void Update()
        {
            // Toggle pause menu with ESC key
            if (Input.GetKeyDown(pauseKey))
            {
                if (settingsOpen)
                {
                    // If settings is open, close it
                    CloseSettings();
                }
                else
                {
                    // Toggle pause menu
                    Toggle();
                }
            }
        }

        private void SetupButtons()
        {
            if (resumeButton != null)
                resumeButton.onClick.AddListener(Resume);

            if (settingsButton != null)
                settingsButton.onClick.AddListener(OpenSettings);

            if (mainMenuButton != null)
                mainMenuButton.onClick.AddListener(ReturnToMainMenu);

            if (quitButton != null)
                quitButton.onClick.AddListener(QuitGame);
        }

        /// <summary>
        /// Shows the pause menu
        /// </summary>
        public void Show()
        {
            isPaused = true;

            if (pauseMenuOverlay != null)
                pauseMenuOverlay.SetActive(true);

            if (pauseMenuWindow != null)
                pauseMenuWindow.SetActive(true);

            if (pauseTimeWhenOpen)
                Time.timeScale = 0f;

            Debug.Log("[PauseMenu] Pause menu shown");
        }

        /// <summary>
        /// Hides the pause menu
        /// </summary>
        public void Hide()
        {
            isPaused = false;
            settingsOpen = false;

            if (pauseMenuOverlay != null)
                pauseMenuOverlay.SetActive(false);

            if (pauseMenuWindow != null)
                pauseMenuWindow.SetActive(false);

            if (settingsPanel != null)
                settingsPanel.Hide();

            if (pauseTimeWhenOpen)
                Time.timeScale = 1f;

            Debug.Log("[PauseMenu] Pause menu hidden");
        }

        /// <summary>
        /// Toggles the pause menu
        /// </summary>
        public void Toggle()
        {
            if (isPaused)
                Hide();
            else
                Show();
        }

        /// <summary>
        /// Resumes the game
        /// </summary>
        public void Resume()
        {
            Hide();
        }

        /// <summary>
        /// Opens the settings panel
        /// </summary>
        public void OpenSettings()
        {
            if (settingsPanel != null)
            {
                settingsPanel.Show();
                settingsOpen = true;

                // Hide the pause menu window but keep overlay
                if (pauseMenuWindow != null)
                    pauseMenuWindow.SetActive(false);

                Debug.Log("[PauseMenu] Settings opened");
            }
            else
            {
                Debug.LogWarning("[PauseMenu] Settings panel reference not set");
            }
        }

        /// <summary>
        /// Closes the settings panel and returns to pause menu
        /// </summary>
        private void CloseSettings()
        {
            if (settingsPanel != null)
            {
                settingsPanel.Hide();
                settingsOpen = false;

                // Show the pause menu window again
                if (pauseMenuWindow != null)
                    pauseMenuWindow.SetActive(true);

                Debug.Log("[PauseMenu] Settings closed");
            }
        }

        /// <summary>
        /// Returns to the main menu
        /// Shows a confirmation to prevent accidental clicks
        /// </summary>
        public void ReturnToMainMenu()
        {
            if (confirmationDialog != null)
            {
                confirmationDialog.Show(
                    title: "Return to Main Menu?",
                    message: "Are you sure you want to return to the main menu? Any unsaved progress may be lost.",
                    confirmText: "Return to Menu",
                    cancelText: "Cancel",
                    onConfirmCallback: DoReturnToMainMenu
                );
            }
            else
            {
                // If no confirmation dialog, go directly
                DoReturnToMainMenu();
            }
        }

        /// <summary>
        /// Actually performs the return to main menu action
        /// </summary>
        private void DoReturnToMainMenu()
        {
            Debug.Log("[PauseMenu] Returning to main menu");

            // Reset time scale before changing scenes
            Time.timeScale = 1f;

            // Load main menu
            SceneManager.LoadScene("MainMenuScene");
        }

        /// <summary>
        /// Quits the game
        /// </summary>
        private void QuitGame()
        {
            Debug.Log("[PauseMenu] Quitting game");

            // Reset time scale before quitting
            Time.timeScale = 1f;

            #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
            #else
                Application.Quit();
            #endif
        }

        private void OnDestroy()
        {
            // Reset time scale when destroyed to avoid issues
            Time.timeScale = 1f;

            // Clean up button listeners
            if (resumeButton != null)
                resumeButton.onClick.RemoveAllListeners();

            if (settingsButton != null)
                settingsButton.onClick.RemoveAllListeners();

            if (mainMenuButton != null)
                mainMenuButton.onClick.RemoveAllListeners();

            if (quitButton != null)
                quitButton.onClick.RemoveAllListeners();
        }
    }
}
