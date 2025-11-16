using UnityEngine;
using UnityEngine.UI;
using TMPro;
using SketchBlossom.Progression;

namespace SketchBlossom.UI
{
    /// <summary>
    /// UI Panel for adjusting game settings
    /// Can be used in main menu or as a pause menu overlay
    /// </summary>
    public class SettingsPanel : MonoBehaviour
    {
        [Header("Panel")]
        [SerializeField] private GameObject panelOverlay;
        [SerializeField] private GameObject panelWindow;

        [Header("Audio Controls")]
        [SerializeField] private Slider masterVolumeSlider;
        [SerializeField] private TextMeshProUGUI masterVolumeText;
        [SerializeField] private Slider musicVolumeSlider;
        [SerializeField] private TextMeshProUGUI musicVolumeText;
        [SerializeField] private Slider sfxVolumeSlider;
        [SerializeField] private TextMeshProUGUI sfxVolumeText;

        [Header("Display Controls")]
        [SerializeField] private Toggle fullscreenToggle;
        [SerializeField] private TMP_Dropdown resolutionDropdown;

        [Header("Buttons")]
        [SerializeField] private Button resetButton;
        [SerializeField] private Button closeButton;

        private void Start()
        {
            SetupControls();
            LoadCurrentSettings();
            Hide(); // Start hidden
        }

        private void SetupControls()
        {
            // Audio sliders
            if (masterVolumeSlider != null)
                masterVolumeSlider.onValueChanged.AddListener(OnMasterVolumeChanged);

            if (musicVolumeSlider != null)
                musicVolumeSlider.onValueChanged.AddListener(OnMusicVolumeChanged);

            if (sfxVolumeSlider != null)
                sfxVolumeSlider.onValueChanged.AddListener(OnSFXVolumeChanged);

            // Display controls
            if (fullscreenToggle != null)
                fullscreenToggle.onValueChanged.AddListener(OnFullscreenChanged);

            if (resolutionDropdown != null)
            {
                PopulateResolutionDropdown();
                resolutionDropdown.onValueChanged.AddListener(OnResolutionChanged);
            }

            // Buttons
            if (resetButton != null)
                resetButton.onClick.AddListener(OnResetClicked);

            if (closeButton != null)
                closeButton.onClick.AddListener(Hide);
        }

        /// <summary>
        /// Populates the resolution dropdown with available screen resolutions
        /// </summary>
        private void PopulateResolutionDropdown()
        {
            if (resolutionDropdown == null) return;

            resolutionDropdown.ClearOptions();
            Resolution[] resolutions = Screen.resolutions;
            var options = new System.Collections.Generic.List<string>();

            for (int i = 0; i < resolutions.Length; i++)
            {
                string option = $"{resolutions[i].width} x {resolutions[i].height}";
                options.Add(option);
            }

            resolutionDropdown.AddOptions(options);
        }

        /// <summary>
        /// Loads current settings from GameSettings
        /// </summary>
        private void LoadCurrentSettings()
        {
            if (GameSettings.Instance == null)
            {
                Debug.LogWarning("[SettingsPanel] GameSettings instance not found");
                return;
            }

            // Load audio settings
            if (masterVolumeSlider != null)
            {
                masterVolumeSlider.SetValueWithoutNotify(GameSettings.Instance.MasterVolume);
                UpdateVolumeText(masterVolumeText, GameSettings.Instance.MasterVolume);
            }

            if (musicVolumeSlider != null)
            {
                musicVolumeSlider.SetValueWithoutNotify(GameSettings.Instance.MusicVolume);
                UpdateVolumeText(musicVolumeText, GameSettings.Instance.MusicVolume);
            }

            if (sfxVolumeSlider != null)
            {
                sfxVolumeSlider.SetValueWithoutNotify(GameSettings.Instance.SFXVolume);
                UpdateVolumeText(sfxVolumeText, GameSettings.Instance.SFXVolume);
            }

            // Load display settings
            if (fullscreenToggle != null)
                fullscreenToggle.SetIsOnWithoutNotify(GameSettings.Instance.IsFullscreen);

            if (resolutionDropdown != null)
                resolutionDropdown.SetValueWithoutNotify(GameSettings.Instance.ResolutionIndex);
        }

        /// <summary>
        /// Shows the settings panel
        /// </summary>
        public void Show()
        {
            LoadCurrentSettings(); // Refresh settings when showing

            if (panelOverlay != null)
                panelOverlay.SetActive(true);

            if (panelWindow != null)
                panelWindow.SetActive(true);

            Debug.Log("[SettingsPanel] Settings panel shown");
        }

        /// <summary>
        /// Hides the settings panel
        /// </summary>
        public void Hide()
        {
            if (panelOverlay != null)
                panelOverlay.SetActive(false);

            if (panelWindow != null)
                panelWindow.SetActive(false);

            Debug.Log("[SettingsPanel] Settings panel hidden");
        }

        /// <summary>
        /// Toggles the visibility of the settings panel
        /// </summary>
        public void Toggle()
        {
            if (panelWindow != null && panelWindow.activeSelf)
                Hide();
            else
                Show();
        }

        // Event Handlers

        private void OnMasterVolumeChanged(float value)
        {
            if (GameSettings.Instance != null)
            {
                GameSettings.Instance.MasterVolume = value;
                UpdateVolumeText(masterVolumeText, value);
            }
        }

        private void OnMusicVolumeChanged(float value)
        {
            if (GameSettings.Instance != null)
            {
                GameSettings.Instance.MusicVolume = value;
                UpdateVolumeText(musicVolumeText, value);
            }
        }

        private void OnSFXVolumeChanged(float value)
        {
            if (GameSettings.Instance != null)
            {
                GameSettings.Instance.SFXVolume = value;
                UpdateVolumeText(sfxVolumeText, value);
            }
        }

        private void OnFullscreenChanged(bool isFullscreen)
        {
            if (GameSettings.Instance != null)
            {
                GameSettings.Instance.IsFullscreen = isFullscreen;
            }
        }

        private void OnResolutionChanged(int index)
        {
            if (GameSettings.Instance != null)
            {
                GameSettings.Instance.ResolutionIndex = index;
            }
        }

        private void OnResetClicked()
        {
            if (GameSettings.Instance != null)
            {
                GameSettings.Instance.ResetToDefaults();
                LoadCurrentSettings();
                Debug.Log("[SettingsPanel] Settings reset to defaults");
            }
        }

        /// <summary>
        /// Updates a volume text display with percentage
        /// </summary>
        private void UpdateVolumeText(TextMeshProUGUI textElement, float value)
        {
            if (textElement != null)
            {
                textElement.text = $"{Mathf.RoundToInt(value * 100)}%";
            }
        }

        private void OnDestroy()
        {
            // Clean up listeners
            if (masterVolumeSlider != null)
                masterVolumeSlider.onValueChanged.RemoveAllListeners();

            if (musicVolumeSlider != null)
                musicVolumeSlider.onValueChanged.RemoveAllListeners();

            if (sfxVolumeSlider != null)
                sfxVolumeSlider.onValueChanged.RemoveAllListeners();

            if (fullscreenToggle != null)
                fullscreenToggle.onValueChanged.RemoveAllListeners();

            if (resolutionDropdown != null)
                resolutionDropdown.onValueChanged.RemoveAllListeners();

            if (resetButton != null)
                resetButton.onClick.RemoveAllListeners();

            if (closeButton != null)
                closeButton.onClick.RemoveAllListeners();
        }
    }
}
