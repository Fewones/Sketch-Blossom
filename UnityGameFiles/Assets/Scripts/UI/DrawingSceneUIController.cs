using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using SketchBlossom.Battle;

namespace SketchBlossom.Drawing
{
    /// <summary>
    /// Controls all UI elements in the Drawing Scene
    /// Handles panels, buttons, and visual feedback
    /// </summary>
    public class DrawingSceneUIController : MonoBehaviour
    {
        [Header("Main Panels")]
        [SerializeField] private GameObject instructionsPanel;
        [SerializeField] private GameObject drawingOverlay;
        [SerializeField] private GameObject drawingPanel;

        [Header("Instruction Elements")]
        [SerializeField] private TextMeshProUGUI instructionTitle;
        [SerializeField] private TextMeshProUGUI instructionText;

        [Header("Drawing UI")]
        [SerializeField] private TextMeshProUGUI strokeCountText;
        [SerializeField] private TextMeshProUGUI hintText;
        [SerializeField] private Button finishButton;
        [SerializeField] private Button clearButton;
        [SerializeField] private Button guideBookButton;

        [Header("Error Display")]
        [SerializeField] private GameObject errorPanel;
        [SerializeField] private TextMeshProUGUI errorText;
        [SerializeField] private float errorDisplayDuration = 2f;

        [Header("References")]
        [SerializeField] private SimpleDrawingCanvas drawingCanvas;

        // Events
        public event Action OnFinishDrawing;
        public event Action OnRedrawRequested;
        public event Action OnContinueRequested;

        private int currentStrokeCount = 0;
        private int maxStrokes = 20;

        #region Unity Lifecycle

        private void Awake()
        {
            AutoDiscoverComponents();
        }

        private void Start()
        {
            SetupButtons();
            SetupInstructions();
            ShowInstructionsPanel();
        }

        private void Update()
        {
            UpdateStrokeCounter();
        }

        #endregion

        #region Initialization

        private void AutoDiscoverComponents()
        {
            // Find drawing canvas
            if (drawingCanvas == null)
            {
                drawingCanvas = FindFirstObjectByType<SimpleDrawingCanvas>();
            }

            if (drawingCanvas != null)
            {
                maxStrokes = drawingCanvas.maxStrokes;
            }

            // Auto-find panels if not assigned
            Canvas canvas = FindFirstObjectByType<Canvas>();
            if (canvas != null)
            {
                if (instructionsPanel == null)
                {
                    Transform t = canvas.transform.Find("InstructionsPanel");
                    if (t != null) instructionsPanel = t.gameObject;
                }

                if (drawingOverlay == null)
                {
                    Transform t = canvas.transform.Find("DrawingOverlay");
                    if (t != null) drawingOverlay = t.gameObject;
                }

                if (drawingPanel == null)
                {
                    Transform t = canvas.transform.Find("DrawingPanel");
                    if (t != null) drawingPanel = t.gameObject;
                }
            }
        }

        private void SetupButtons()
        {
            if (finishButton != null)
            {
                finishButton.onClick.AddListener(OnFinishButtonClicked);
            }

            if (clearButton != null)
            {
                clearButton.onClick.AddListener(OnClearButtonClicked);
            }

            if (guideBookButton != null)
            {
                GuideBookManager guideBook = FindFirstObjectByType<GuideBookManager>();
                if (guideBook != null)
                {
                    guideBookButton.onClick.AddListener(() => guideBook.OpenGuide());
                }
            }
        }

        private void SetupInstructions()
        {
            if (instructionTitle != null)
            {
                instructionTitle.text = "Draw Your Plant Companion";
            }

            if (instructionText != null)
            {
                instructionText.text =
                    "Welcome to Sketch Blossom!\n\n" +
                    "Draw a plant shape to create your battle companion.\n\n" +
                    "<color=#FF8C42><b>Red</b></color> strokes → Fire plants (high attack)\n" +
                    "<color=#4ECDC4><b>Green</b></color> strokes → Grass plants (balanced)\n" +
                    "<color=#95E1D3><b>Blue</b></color> strokes → Water plants (high HP)\n\n" +
                    "Use the guidebook (book icon) for drawing tips!";
            }
        }

        #endregion

        #region Panel Management

        public void ShowInstructionsPanel()
        {
            SetPanelActive(instructionsPanel, true);
            SetPanelActive(drawingOverlay, false);
            SetPanelActive(drawingPanel, false);
        }

        public void ShowDrawingPanel()
        {
            SetPanelActive(instructionsPanel, false);
            SetPanelActive(drawingOverlay, true);
            SetPanelActive(drawingPanel, true);

            // Re-enable all drawing UI controls
            if (finishButton != null) finishButton.gameObject.SetActive(true);
            if (clearButton != null) clearButton.gameObject.SetActive(true);
            if (guideBookButton != null) guideBookButton.gameObject.SetActive(true);
            if (strokeCountText != null) strokeCountText.gameObject.SetActive(true);
            if (hintText != null) hintText.gameObject.SetActive(true);

            // Re-enable any title elements that were hidden
            if (drawingPanel != null)
            {
                foreach (Transform child in drawingPanel.transform)
                {
                    if (child.name.ToLower().Contains("title"))
                    {
                        child.gameObject.SetActive(true);
                    }
                }
            }

            UpdateHint("Draw your plant! Choose a color and start drawing.");
            drawingCanvas.visible = true;
        }

        public void HideDrawingPanel()
        {
            // Hide specific UI controls but keep panels active
            // This allows the drawing strokes to remain visible
            if (finishButton != null) finishButton.gameObject.SetActive(false);
            if (clearButton != null) clearButton.gameObject.SetActive(false);
            if (guideBookButton != null) guideBookButton.gameObject.SetActive(false);
            if (strokeCountText != null) strokeCountText.gameObject.SetActive(false);
            if (hintText != null) hintText.gameObject.SetActive(false);

            // If there's a title element in the drawing panel, find and hide it
            if (drawingPanel != null)
            {
                // Look for any TextMeshProUGUI with "title" in the name and hide it
                foreach (Transform child in drawingPanel.transform)
                {
                    if (child.name.ToLower().Contains("title"))
                    {
                        child.gameObject.SetActive(false);
                        Debug.Log($"✓ Hidden title element: {child.name}");
                    }
                }
            }

            // Keep panels active - only controls are hidden
            Debug.Log("✓ Drawing UI controls hidden, panels and strokes remain visible");

            // Ensure strokes are explicitly kept visible
            if (drawingCanvas != null && drawingCanvas.strokeContainer != null)
            {
                drawingCanvas.strokeContainer.gameObject.SetActive(true);
            }
            drawingCanvas.visible = false;
        }

        private void SetPanelActive(GameObject panel, bool active)
        {
            if (panel != null)
            {
                panel.SetActive(active);
            }
        }

        #endregion

        #region Button Handlers

        private void OnFinishButtonClicked()
        {
            Debug.Log("Finish button clicked");
            OnFinishDrawing?.Invoke();
        }

        private void OnClearButtonClicked()
        {
            Debug.Log("Clear button clicked");

            if (drawingCanvas != null)
            {
                drawingCanvas.ClearAll();
                UpdateHint("Canvas cleared! Start fresh.");
            }
        }

        #endregion

        #region Stroke Counter

        private void UpdateStrokeCounter()
        {
            if (drawingCanvas == null) return;

            int newStrokeCount = drawingCanvas.GetStrokeCount();
            if (newStrokeCount != currentStrokeCount)
            {
                currentStrokeCount = newStrokeCount;
                DisplayStrokeCount();
                UpdateHintBasedOnStrokes();
            }
        }

        private void DisplayStrokeCount()
        {
            if (strokeCountText != null)
            {
                strokeCountText.text = $"Strokes: {currentStrokeCount} / {maxStrokes}";

                // Color-code based on usage
                if (currentStrokeCount >= maxStrokes)
                {
                    strokeCountText.color = Color.red;
                }
                else if (currentStrokeCount >= maxStrokes - 3)
                {
                    strokeCountText.color = Color.yellow;
                }
                else
                {
                    strokeCountText.color = Color.white;
                }
            }
        }

        private void UpdateHintBasedOnStrokes()
        {
            if (currentStrokeCount == 0)
            {
                UpdateHint("Draw your plant! Choose a color and start drawing.");
            }
            else if (currentStrokeCount == 1)
            {
                UpdateHint("Great start! Keep drawing to define the shape.");
            }
            else if (currentStrokeCount >= 3 && currentStrokeCount < maxStrokes - 3)
            {
                UpdateHint("Looking good! Add more details or click Finish when ready.");
            }
            else if (currentStrokeCount >= maxStrokes - 3)
            {
                UpdateHint("Almost at the limit! Finish your drawing soon.");
            }
            else if (currentStrokeCount >= maxStrokes)
            {
                UpdateHint("Maximum strokes reached! Click Finish to see your plant.");
            }
        }

        #endregion

        #region Hint & Error Display

        public void UpdateHint(string hint)
        {
            if (hintText != null)
            {
                hintText.text = hint;
            }
        }

        public void ShowError(string message)
        {
            Debug.LogWarning($"UI Error: {message}");

            if (errorPanel != null && errorText != null)
            {
                errorText.text = message;
                errorPanel.SetActive(true);
                Invoke(nameof(HideError), errorDisplayDuration);
            }
            else
            {
                // Fallback to hint text
                UpdateHint($"<color=red>{message}</color>");
            }
        }

        private void HideError()
        {
            if (errorPanel != null)
            {
                errorPanel.SetActive(false);
            }
        }

        #endregion

        #region Cleanup

        private void OnDestroy()
        {
            // Clean up button listeners
            if (finishButton != null)
                finishButton.onClick.RemoveListener(OnFinishButtonClicked);

            if (clearButton != null)
                clearButton.onClick.RemoveListener(OnClearButtonClicked);
        }

        #endregion
    }
}
