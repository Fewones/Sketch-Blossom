using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

namespace SketchBlossom.Battle
{
    /// <summary>
    /// Manages the drawing guide book UI panel.
    /// Handles opening, closing, page navigation, and button interactions for the guide book.
    /// </summary>
    public class GuideBookManager : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Button openGuideButton;
        [SerializeField] private GameObject guidePanel;
        [SerializeField] private Button closeGuideButton;
        [SerializeField] private Button previousPageButton;
        [SerializeField] private Button nextPageButton;
        [SerializeField] private TextMeshProUGUI pageIndicatorText;

        [Header("Page Content")]
        [SerializeField] private GameObject[] pages;

        [Header("Page Settings")]
        [SerializeField] private int currentPageIndex = 0;

        [Header("Debug")]
        [SerializeField] private bool debugMode = true;

        private void Start()
        {
            AutoWireReferences();
            SetupListeners();
            SetupPages();

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

            // Find page navigation buttons
            if (guidePanel != null)
            {
                Button[] buttons = guidePanel.GetComponentsInChildren<Button>(true);
                foreach (Button btn in buttons)
                {
                    if (btn.name == "PreviousPageButton" && previousPageButton == null)
                    {
                        previousPageButton = btn;
                        LogDebug("✅ Found PreviousPageButton");
                    }
                    else if (btn.name == "NextPageButton" && nextPageButton == null)
                    {
                        nextPageButton = btn;
                        LogDebug("✅ Found NextPageButton");
                    }
                }
            }

            // Find page indicator text
            if (pageIndicatorText == null && guidePanel != null)
            {
                TextMeshProUGUI[] texts = guidePanel.GetComponentsInChildren<TextMeshProUGUI>(true);
                foreach (TextMeshProUGUI text in texts)
                {
                    if (text.name == "PageIndicator")
                    {
                        pageIndicatorText = text;
                        LogDebug("✅ Found PageIndicator");
                        break;
                    }
                }
            }

            // Find all pages
            if (guidePanel != null)
            {
                List<GameObject> foundPages = new List<GameObject>();
                Transform contentPanel = guidePanel.transform.Find("ContentPanel");
                if (contentPanel != null)
                {
                    for (int i = 0; i < contentPanel.childCount; i++)
                    {
                        Transform child = contentPanel.GetChild(i);
                        if (child.name.StartsWith("Page"))
                        {
                            foundPages.Add(child.gameObject);
                        }
                    }
                }

                if (foundPages.Count > 0)
                {
                    // Sort pages by name to ensure correct order (Page0, Page1, Page2, etc.)
                    foundPages.Sort((a, b) => string.Compare(a.name, b.name));
                    pages = foundPages.ToArray();
                    LogDebug($"✅ Found {pages.Length} pages (sorted by name)");
                    for (int i = 0; i < pages.Length; i++)
                    {
                        LogDebug($"    Page {i}: {pages[i].name}");
                    }
                }
                else
                {
                    LogDebug("❌ No pages found");
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

            // Setup previous page button
            if (previousPageButton != null)
            {
                previousPageButton.onClick.RemoveAllListeners();
                previousPageButton.onClick.AddListener(PreviousPage);
                LogDebug("✅ Previous page button listener added");
            }

            // Setup next page button
            if (nextPageButton != null)
            {
                nextPageButton.onClick.RemoveAllListeners();
                nextPageButton.onClick.AddListener(NextPage);
                LogDebug("✅ Next page button listener added");
            }

            LogDebug("=== Listener setup complete ===");
        }

        /// <summary>
        /// Setup pages and show the first page
        /// </summary>
        private void SetupPages()
        {
            LogDebug("=== Setting up pages ===");

            if (pages == null || pages.Length == 0)
            {
                LogDebug("❌ No pages to setup");
                return;
            }

            // Hide all pages
            for (int i = 0; i < pages.Length; i++)
            {
                if (pages[i] != null)
                {
                    pages[i].SetActive(false);
                }
            }

            // Show first page
            currentPageIndex = 0;
            ShowCurrentPage();

            LogDebug($"✅ Pages setup complete. Total pages: {pages.Length}");
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
                ShowCurrentPage();
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
        /// Go to the previous page
        /// </summary>
        public void PreviousPage()
        {
            if (pages == null || pages.Length == 0) return;

            currentPageIndex--;
            if (currentPageIndex < 0)
            {
                currentPageIndex = pages.Length - 1; // Wrap to last page
            }

            ShowCurrentPage();
            LogDebug($"Navigated to page {currentPageIndex + 1}/{pages.Length}");
        }

        /// <summary>
        /// Go to the next page
        /// </summary>
        public void NextPage()
        {
            if (pages == null || pages.Length == 0) return;

            currentPageIndex++;
            if (currentPageIndex >= pages.Length)
            {
                currentPageIndex = 0; // Wrap to first page
            }

            ShowCurrentPage();
            LogDebug($"Navigated to page {currentPageIndex + 1}/{pages.Length}");
        }

        /// <summary>
        /// Show the current page and hide others
        /// </summary>
        private void ShowCurrentPage()
        {
            if (pages == null || pages.Length == 0)
            {
                LogDebug("No pages to show");
                return;
            }

            // Hide all pages
            for (int i = 0; i < pages.Length; i++)
            {
                if (pages[i] != null)
                {
                    pages[i].SetActive(i == currentPageIndex);
                }
            }

            // Update page indicator
            UpdatePageIndicator();

            // Update navigation button states
            UpdateNavigationButtons();
        }

        /// <summary>
        /// Update page indicator text
        /// </summary>
        private void UpdatePageIndicator()
        {
            if (pageIndicatorText != null && pages != null && pages.Length > 0)
            {
                pageIndicatorText.text = $"Page {currentPageIndex + 1} / {pages.Length}";
            }
        }

        /// <summary>
        /// Update navigation button interactability
        /// </summary>
        private void UpdateNavigationButtons()
        {
            if (pages == null || pages.Length <= 1)
            {
                // Disable navigation if only one or no pages
                if (previousPageButton != null)
                    previousPageButton.interactable = false;
                if (nextPageButton != null)
                    nextPageButton.interactable = false;
            }
            else
            {
                // Enable navigation buttons
                if (previousPageButton != null)
                    previousPageButton.interactable = true;
                if (nextPageButton != null)
                    nextPageButton.interactable = true;
            }
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
            SetupPages();
            LogDebug("Force re-wire complete");
        }

        /// <summary>
        /// Manually trigger page setup (useful for editor debugging)
        /// </summary>
        [ContextMenu("Setup Pages Manually")]
        public void ManualSetupPages()
        {
            LogDebug("=== Manual Page Setup ===");
            SetupPages();

            if (pages != null && pages.Length > 0)
            {
                LogDebug($"Pages setup complete. Current page: {currentPageIndex + 1}/{pages.Length}");
                ShowCurrentPage();
            }
        }

        /// <summary>
        /// Debug current state
        /// </summary>
        [ContextMenu("Debug Current State")]
        public void DebugCurrentState()
        {
            LogDebug("=== Current State ===");
            LogDebug($"Guide Panel: {guidePanel != null}");
            LogDebug($"Open Button: {openGuideButton != null}");
            LogDebug($"Close Button: {closeGuideButton != null}");
            LogDebug($"Previous Button: {previousPageButton != null}");
            LogDebug($"Next Button: {nextPageButton != null}");
            LogDebug($"Page Indicator: {pageIndicatorText != null}");
            LogDebug($"Pages Array: {pages != null} (Length: {pages?.Length ?? 0})");
            LogDebug($"Current Page Index: {currentPageIndex}");

            if (pages != null && pages.Length > 0)
            {
                for (int i = 0; i < pages.Length; i++)
                {
                    LogDebug($"  Page {i}: {pages[i]?.name ?? "null"} - Active: {pages[i]?.activeSelf ?? false}");
                }
            }
        }
    }
}
