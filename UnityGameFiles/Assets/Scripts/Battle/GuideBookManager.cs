using UnityEngine;
using UnityEngine.UI;

namespace SketchBlossom.Battle
{
    /// <summary>
    /// Manages the drawing guide book UI panel.
    /// Handles opening, closing, and button interactions for the guide book.
    /// </summary>
    public class GuideBookManager : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Button openGuideButton;
        [SerializeField] private GameObject guidePanel;
        [SerializeField] private Button closeGuideButton;

        [Header("Debug")]
        [SerializeField] private bool debugMode = true;

        private void Start()
        {
            AutoWireReferences();
            SetupListeners();

            // Initially hide the guide panel
            if (guidePanel != null)
            {
                guidePanel.SetActive(false);
                LogDebug("Guide panel initially hidden");
            }
        }

        /// <summary>
        /// Automatically find and wire up all necessary references
        /// </summary>
        private void AutoWireReferences()
        {
            LogDebug("=== Auto-wiring GuideBookManager references ===");

            // Find the open guide button by name if not assigned
            if (openGuideButton == null)
            {
                GameObject buttonObj = GameObject.Find("GuideBookButton");
                if (buttonObj != null)
                {
                    openGuideButton = buttonObj.GetComponent<Button>();
                    LogDebug($"✅ Found GuideBookButton: {openGuideButton != null}");
                }
                else
                {
                    LogDebug("❌ GuideBookButton not found in scene");
                }
            }

            // Find the guide panel by name if not assigned
            if (guidePanel == null)
            {
                guidePanel = GameObject.Find("GuidePanel");
                LogDebug($"✅ Found GuidePanel: {guidePanel != null}");
            }

            // Find the close button within the guide panel
            if (closeGuideButton == null && guidePanel != null)
            {
                // Look for close button in children
                Button[] buttons = guidePanel.GetComponentsInChildren<Button>(true);
                foreach (Button btn in buttons)
                {
                    if (btn.name == "CloseButton")
                    {
                        closeGuideButton = btn;
                        LogDebug("✅ Found CloseButton in GuidePanel");
                        break;
                    }
                }

                if (closeGuideButton == null)
                {
                    LogDebug("❌ CloseButton not found in GuidePanel");
                }
            }

            LogDebug("=== Auto-wire complete ===");
        }

        /// <summary>
        /// Setup button click listeners
        /// </summary>
        private void SetupListeners()
        {
            LogDebug("=== Setting up button listeners ===");

            // Setup open button
            if (openGuideButton != null)
            {
                openGuideButton.onClick.RemoveAllListeners();
                openGuideButton.onClick.AddListener(OpenGuide);
                LogDebug("✅ Open button listener added");
            }
            else
            {
                LogDebug("❌ Cannot setup open button - button is null");
            }

            // Setup close button
            if (closeGuideButton != null)
            {
                closeGuideButton.onClick.RemoveAllListeners();
                closeGuideButton.onClick.AddListener(CloseGuide);
                LogDebug("✅ Close button listener added");
            }
            else
            {
                LogDebug("❌ Cannot setup close button - button is null");
            }

            LogDebug("=== Listener setup complete ===");
        }

        /// <summary>
        /// Open the guide book panel
        /// </summary>
        public void OpenGuide()
        {
            LogDebug("Opening guide book...");

            if (guidePanel != null)
            {
                guidePanel.SetActive(true);
                LogDebug("✅ Guide book opened successfully");
            }
            else
            {
                LogDebug("❌ Cannot open guide - guidePanel is null");
            }
        }

        /// <summary>
        /// Close the guide book panel
        /// </summary>
        public void CloseGuide()
        {
            LogDebug("Closing guide book...");

            if (guidePanel != null)
            {
                guidePanel.SetActive(false);
                LogDebug("✅ Guide book closed successfully");
            }
            else
            {
                LogDebug("❌ Cannot close guide - guidePanel is null");
            }
        }

        /// <summary>
        /// Toggle guide visibility (useful for external scripts)
        /// </summary>
        public void ToggleGuide()
        {
            if (guidePanel != null)
            {
                if (guidePanel.activeSelf)
                {
                    CloseGuide();
                }
                else
                {
                    OpenGuide();
                }
            }
        }

        /// <summary>
        /// Check if guide is currently open
        /// </summary>
        public bool IsGuideOpen()
        {
            return guidePanel != null && guidePanel.activeSelf;
        }

        /// <summary>
        /// Manually assign references (useful for programmatic setup)
        /// </summary>
        public void AssignReferences(Button openButton, GameObject panel, Button closeButton)
        {
            openGuideButton = openButton;
            guidePanel = panel;
            closeGuideButton = closeButton;

            LogDebug("References manually assigned");
            SetupListeners();
        }

        /// <summary>
        /// Debug logging
        /// </summary>
        private void LogDebug(string message)
        {
            if (debugMode)
            {
                Debug.Log($"[GuideBookManager] {message}");
            }
        }

        /// <summary>
        /// Force re-wire and setup (useful for editor testing)
        /// </summary>
        [ContextMenu("Force Re-wire")]
        public void ForceReWire()
        {
            AutoWireReferences();
            SetupListeners();
            LogDebug("Force re-wire complete");
        }
    }
}
