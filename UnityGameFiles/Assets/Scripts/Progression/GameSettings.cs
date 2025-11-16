using UnityEngine;
using System;

namespace SketchBlossom.Progression
{
    /// <summary>
    /// Singleton that manages game settings and persists them across sessions
    /// </summary>
    public class GameSettings : MonoBehaviour
    {
        public static GameSettings Instance { get; private set; }

        // Setting Keys for PlayerPrefs
        private const string MASTER_VOLUME_KEY = "Settings_MasterVolume";
        private const string MUSIC_VOLUME_KEY = "Settings_MusicVolume";
        private const string SFX_VOLUME_KEY = "Settings_SFXVolume";
        private const string FULLSCREEN_KEY = "Settings_Fullscreen";
        private const string RESOLUTION_INDEX_KEY = "Settings_ResolutionIndex";

        // Default values
        private const float DEFAULT_VOLUME = 0.7f;

        // Current settings
        [Header("Audio Settings")]
        [SerializeField] private float masterVolume = DEFAULT_VOLUME;
        [SerializeField] private float musicVolume = DEFAULT_VOLUME;
        [SerializeField] private float sfxVolume = DEFAULT_VOLUME;

        [Header("Display Settings")]
        [SerializeField] private bool isFullscreen = true;
        [SerializeField] private int resolutionIndex = 0;

        // Events
        public event Action<float> OnMasterVolumeChanged;
        public event Action<float> OnMusicVolumeChanged;
        public event Action<float> OnSFXVolumeChanged;
        public event Action<bool> OnFullscreenChanged;
        public event Action<int> OnResolutionChanged;

        // Properties
        public float MasterVolume
        {
            get => masterVolume;
            set
            {
                masterVolume = Mathf.Clamp01(value);
                SaveSetting(MASTER_VOLUME_KEY, masterVolume);
                OnMasterVolumeChanged?.Invoke(masterVolume);
                ApplyAudioSettings();
            }
        }

        public float MusicVolume
        {
            get => musicVolume;
            set
            {
                musicVolume = Mathf.Clamp01(value);
                SaveSetting(MUSIC_VOLUME_KEY, musicVolume);
                OnMusicVolumeChanged?.Invoke(musicVolume);
                ApplyAudioSettings();
            }
        }

        public float SFXVolume
        {
            get => sfxVolume;
            set
            {
                sfxVolume = Mathf.Clamp01(value);
                SaveSetting(SFX_VOLUME_KEY, sfxVolume);
                OnSFXVolumeChanged?.Invoke(sfxVolume);
                ApplyAudioSettings();
            }
        }

        public bool IsFullscreen
        {
            get => isFullscreen;
            set
            {
                isFullscreen = value;
                SaveSetting(FULLSCREEN_KEY, isFullscreen ? 1 : 0);
                OnFullscreenChanged?.Invoke(isFullscreen);
                ApplyDisplaySettings();
            }
        }

        public int ResolutionIndex
        {
            get => resolutionIndex;
            set
            {
                resolutionIndex = value;
                SaveSetting(RESOLUTION_INDEX_KEY, resolutionIndex);
                OnResolutionChanged?.Invoke(resolutionIndex);
                ApplyDisplaySettings();
            }
        }

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

            LoadSettings();
            ApplySettings();
        }

        /// <summary>
        /// Loads all settings from PlayerPrefs
        /// </summary>
        private void LoadSettings()
        {
            masterVolume = PlayerPrefs.GetFloat(MASTER_VOLUME_KEY, DEFAULT_VOLUME);
            musicVolume = PlayerPrefs.GetFloat(MUSIC_VOLUME_KEY, DEFAULT_VOLUME);
            sfxVolume = PlayerPrefs.GetFloat(SFX_VOLUME_KEY, DEFAULT_VOLUME);
            isFullscreen = PlayerPrefs.GetInt(FULLSCREEN_KEY, 1) == 1;
            resolutionIndex = PlayerPrefs.GetInt(RESOLUTION_INDEX_KEY, 0);

            Debug.Log($"[GameSettings] Settings loaded - Master: {masterVolume}, Music: {musicVolume}, SFX: {sfxVolume}");
        }

        /// <summary>
        /// Applies all settings to the game
        /// </summary>
        private void ApplySettings()
        {
            ApplyAudioSettings();
            ApplyDisplaySettings();
        }

        /// <summary>
        /// Applies audio settings
        /// </summary>
        private void ApplyAudioSettings()
        {
            AudioListener.volume = masterVolume;
            Debug.Log($"[GameSettings] Audio applied - Master: {masterVolume}, Music: {musicVolume}, SFX: {sfxVolume}");
        }

        /// <summary>
        /// Applies display settings
        /// </summary>
        private void ApplyDisplaySettings()
        {
            Screen.fullScreen = isFullscreen;

            if (resolutionIndex >= 0 && resolutionIndex < Screen.resolutions.Length)
            {
                Resolution resolution = Screen.resolutions[resolutionIndex];
                Screen.SetResolution(resolution.width, resolution.height, isFullscreen);
                Debug.Log($"[GameSettings] Display applied - Fullscreen: {isFullscreen}, Resolution: {resolution.width}x{resolution.height}");
            }
        }

        /// <summary>
        /// Saves a float setting to PlayerPrefs
        /// </summary>
        private void SaveSetting(string key, float value)
        {
            PlayerPrefs.SetFloat(key, value);
            PlayerPrefs.Save();
        }

        /// <summary>
        /// Saves an int setting to PlayerPrefs
        /// </summary>
        private void SaveSetting(string key, int value)
        {
            PlayerPrefs.SetInt(key, value);
            PlayerPrefs.Save();
        }

        /// <summary>
        /// Resets all settings to default values
        /// </summary>
        public void ResetToDefaults()
        {
            MasterVolume = DEFAULT_VOLUME;
            MusicVolume = DEFAULT_VOLUME;
            SFXVolume = DEFAULT_VOLUME;
            IsFullscreen = true;
            ResolutionIndex = 0;

            Debug.Log("[GameSettings] Settings reset to defaults");
        }

        /// <summary>
        /// Gets the effective volume for music (master * music)
        /// </summary>
        public float GetEffectiveMusicVolume()
        {
            return masterVolume * musicVolume;
        }

        /// <summary>
        /// Gets the effective volume for SFX (master * sfx)
        /// </summary>
        public float GetEffectiveSFXVolume()
        {
            return masterVolume * sfxVolume;
        }
    }
}
