using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using SketchBlossom.Battle;

namespace SketchBlossom.Drawing
{
    /// <summary>
    /// Main manager for the Drawing Scene - handles game flow and coordination
    /// Coordinates between drawing canvas, recognition system, and UI
    /// </summary>
    public class DrawingSceneManager : MonoBehaviour
    {
        [Header("Core Systems")]
        [SerializeField] private SimpleDrawingCanvas drawingCanvas;
        [SerializeField] private PlantRecognitionSystem recognitionSystem;
        [SerializeField] private DrawnUnitData unitData;

        [Header("UI References")]
        [SerializeField] private DrawingSceneUIController uiController;
        [SerializeField] private PlantResultPanel resultPanel;
        [SerializeField] private GuideBookManager guideBook;

        [Header("Drawing Capture")]
        [SerializeField] private DrawingCaptureHandler captureHandler;

        [Header("Scene Settings")]
        [SerializeField] private string battleSceneName = "DrawingBattleScene";
        [SerializeField] private bool enableBattleTransition = true;

        private PlantRecognitionSystem.RecognitionResult lastResult;

        #region Unity Lifecycle

        private void Awake()
        {
            AutoDiscoverComponents();
            InitializeUnitData();
        }

        private void Start()
        {
            InitializeSystems();
            SetupEventListeners();
            ShowInstructions();
        }

        #endregion

        #region Initialization

        private void AutoDiscoverComponents()
        {
            // Auto-find drawing canvas
            if (drawingCanvas == null)
            {
                drawingCanvas = FindFirstObjectByType<SimpleDrawingCanvas>();
                if (drawingCanvas == null)
                {
                    Debug.LogError("DrawingSceneManager: SimpleDrawingCanvas not found!");
                }
            }

            // Auto-find or create recognition system
            if (recognitionSystem == null)
            {
                recognitionSystem = FindFirstObjectByType<PlantRecognitionSystem>();
                if (recognitionSystem == null)
                {
                    GameObject recognitionObj = new GameObject("PlantRecognitionSystem");
                    recognitionSystem = recognitionObj.AddComponent<PlantRecognitionSystem>();
                    Debug.Log("DrawingSceneManager: Created PlantRecognitionSystem");
                }
            }

            // Auto-find UI controller
            if (uiController == null)
            {
                uiController = FindFirstObjectByType<DrawingSceneUIController>();
                if (uiController == null)
                {
                    Debug.LogError("DrawingSceneManager: DrawingSceneUIController not found!");
                }
            }

            // Auto-find result panel
            if (resultPanel == null)
            {
                resultPanel = FindFirstObjectByType<PlantResultPanel>(FindObjectsInactive.Include);
                if (resultPanel == null)
                {
                    Debug.LogWarning("DrawingSceneManager: PlantResultPanel not found");
                }
            }

            // Auto-find guidebook (optional)
            if (guideBook == null)
            {
                guideBook = FindFirstObjectByType<GuideBookManager>();
            }

            // Auto-find or create capture handler
            if (captureHandler == null)
            {
                captureHandler = FindFirstObjectByType<DrawingCaptureHandler>();
                if (captureHandler == null)
                {
                    GameObject captureObj = new GameObject("DrawingCaptureHandler");
                    captureHandler = captureObj.AddComponent<DrawingCaptureHandler>();
                    Debug.Log("DrawingSceneManager: Created DrawingCaptureHandler");
                }
            }
        }

        private void InitializeUnitData()
        {
            // Get or create DrawnUnitData singleton
            if (DrawnUnitData.Instance == null)
            {
                GameObject dataObj = new GameObject("DrawnUnitData");
                unitData = dataObj.AddComponent<DrawnUnitData>();
                Debug.Log("DrawingSceneManager: Created DrawnUnitData singleton");
            }
            else
            {
                unitData = DrawnUnitData.Instance;
                unitData.ClearData();
                Debug.Log("DrawingSceneManager: Using existing DrawnUnitData");
            }
        }

        private void InitializeSystems()
        {
            Debug.Log("========== DRAWING SCENE INITIALIZED ==========");
            Debug.Log($"Drawing Canvas: {(drawingCanvas != null ? "✓" : "✗")}");
            Debug.Log($"Recognition System: {(recognitionSystem != null ? "✓" : "✗")}");
            Debug.Log($"UI Controller: {(uiController != null ? "✓" : "✗")}");
            Debug.Log($"Result Panel: {(resultPanel != null ? "✓" : "✗")}");
            Debug.Log($"Capture Handler: {(captureHandler != null ? "✓" : "✗")}");
            Debug.Log("===============================================");
        }

        private void SetupEventListeners()
        {
            if (uiController != null)
            {
                uiController.OnFinishDrawing += HandleFinishDrawing;
                uiController.OnRedrawRequested += HandleRedraw;
                uiController.OnContinueRequested += HandleContinue;
            }
        }

        #endregion

        #region Public Methods

        public void ShowInstructions()
        {
            if (uiController != null)
            {
                uiController.ShowInstructionsPanel();
            }
        }

        public void StartDrawing()
        {
            if (uiController != null)
            {
                uiController.ShowDrawingPanel();
            }
        }

        #endregion

        #region Event Handlers

        private void HandleFinishDrawing()
        {
            Debug.Log("DrawingSceneManager: Finish button pressed");

            if (!ValidateDrawing())
            {
                return;
            }

            // Force end any in-progress stroke
            drawingCanvas.ForceEndStroke();

            // Analyze the drawing
            AnalyzeDrawing();

            // Show results
            ShowResults();
        }

        private void HandleRedraw()
        {
            Debug.Log("DrawingSceneManager: Redraw requested - clearing strokes and resetting");

            // Clear the drawing (this removes all strokes from the canvas)
            if (drawingCanvas != null)
            {
                drawingCanvas.ClearAll();
                Debug.Log("✓ All strokes cleared for redraw");
            }

            // Clear stored results
            lastResult = null;
            unitData.ClearData();

            // Return to drawing panel to draw again
            if (uiController != null)
            {
                uiController.ShowDrawingPanel();
            }
        }

        private void HandleContinue()
        {
            Debug.Log("DrawingSceneManager: Continue to battle - capturing drawing and loading battle scene");

            // Capture the drawing and save it to DrawnUnitData
            CaptureAndSaveDrawing();

            if (enableBattleTransition)
            {
                LoadBattleScene();
            }
            else
            {
                Debug.LogWarning("Battle transition disabled. Enable 'enableBattleTransition' in DrawingSceneManager inspector.");
            }
        }

        /// <summary>
        /// Capture the current drawing and save it to DrawnUnitData for use in battle
        /// </summary>
        private void CaptureAndSaveDrawing()
        {
            if (captureHandler == null)
            {
                Debug.LogError("DrawingCaptureHandler is null! Cannot capture drawing.");
                return;
            }

            if (drawingCanvas == null || drawingCanvas.allStrokes.Count == 0)
            {
                Debug.LogWarning("No drawing to capture!");
                return;
            }

            // Get the main camera for rendering
            Camera mainCamera = drawingCanvas.mainCamera;
            if (mainCamera == null)
            {
                mainCamera = Camera.main;
            }

            if (mainCamera == null)
            {
                Debug.LogError("No camera available for capturing drawing!");
                return;
            }

            Debug.Log("========== CAPTURING PLANT DRAWING ==========");

            // Capture the drawing as a texture (pass drawing area for screen capture option)
            Texture2D drawingTexture = captureHandler.CaptureDrawing(
                drawingCanvas.allStrokes,
                mainCamera,
                drawingCanvas.drawingArea
            );

            if (drawingTexture != null)
            {
                // Save to DrawnUnitData
                unitData.drawingTexture = drawingTexture;
                Debug.Log($"✓ Drawing captured and saved! Texture size: {drawingTexture.width}x{drawingTexture.height}");
                Debug.Log("   This texture will be used as your plant's sprite in battle!");
            }
            else
            {
                Debug.LogError("Failed to capture drawing texture!");
            }

            Debug.Log("=============================================");
        }

        #endregion

        #region Drawing Analysis

        private bool ValidateDrawing()
        {
            if (drawingCanvas == null)
            {
                Debug.LogError("DrawingCanvas is null!");
                uiController?.ShowError("Drawing system error!");
                return false;
            }

            if (drawingCanvas.allStrokes.Count == 0)
            {
                Debug.LogWarning("No strokes drawn!");
                uiController?.ShowError("Please draw something first!");
                return false;
            }

            return true;
        }

        private void AnalyzeDrawing()
        {
            if (recognitionSystem == null)
            {
                Debug.LogError("PlantRecognitionSystem is null!");
                return;
            }

            Debug.Log("========== ANALYZING DRAWING ==========");

            // Get drawing data
            List<LineRenderer> strokes = drawingCanvas.allStrokes;
            Color dominantColor = drawingCanvas.GetDominantColor();

            Debug.Log($"Strokes: {strokes.Count}, Dominant Color: {dominantColor}");

            // Analyze with recognition system
            lastResult = recognitionSystem.AnalyzeDrawing(strokes, dominantColor);

            if (lastResult != null)
            {
                // Check if the plant is valid
                if (!lastResult.isValidPlant)
                {
                    Debug.LogWarning("⚠️ Drawing does not meet plant requirements!");
                    Debug.LogWarning("   User must redraw to create a valid plant.");

                    // DO NOT store invalid plant data
                    // The result panel will show the error and offer Redraw option
                }
                else
                {
                    // Store valid plant data in DrawnUnitData
                    unitData.SetPlantData(lastResult.plantData);
                    unitData.detectionConfidence = lastResult.confidence;

                    Debug.Log($"✓ VALID PLANT Detected: {lastResult.plantData.displayName}");
                    Debug.Log($"Element: {lastResult.element}");
                    Debug.Log($"Confidence: {lastResult.confidence:P0}");
                    Debug.Log("========================================");
                }
            }
            else
            {
                Debug.LogError("Analysis failed - no result returned!");
            }
        }

        private void ShowResults()
        {
            if (lastResult == null)
            {
                Debug.LogError("No analysis result to show!");
                return;
            }

            // IMPORTANT: Keep strokes visible in the background behind results panel
            // Strokes should remain visible until user clicks "Redraw"
            if (drawingCanvas != null && drawingCanvas.strokeContainer != null)
            {
                drawingCanvas.strokeContainer.gameObject.SetActive(true);
                Debug.Log("✓ Strokes kept visible for results screen");
            }

            // Hide drawing UI controls (buttons, hints, etc.)
            if (uiController != null)
            {
                uiController.HideDrawingPanel();
            }

            // Show result panel with Continue (→ Battle) and Redraw options
            if (resultPanel != null)
            {
                resultPanel.ShowResults(lastResult, unitData, HandleContinue, HandleRedraw);
            }
            else
            {
                Debug.LogError("PlantResultPanel is null! Cannot show results.");
            }
        }

        #endregion

        #region Scene Management

        private void LoadBattleScene()
        {
            if (string.IsNullOrEmpty(battleSceneName))
            {
                Debug.LogError("Battle scene name not set!");
                return;
            }

            Debug.Log($"Loading battle scene: {battleSceneName}");
            SceneManager.LoadScene(battleSceneName);
        }

        #endregion

        #region Cleanup

        private void OnDestroy()
        {
            // Unsubscribe from events
            if (uiController != null)
            {
                uiController.OnFinishDrawing -= HandleFinishDrawing;
                uiController.OnRedrawRequested -= HandleRedraw;
                uiController.OnContinueRequested -= HandleContinue;
            }
        }

        #endregion
    }
}
