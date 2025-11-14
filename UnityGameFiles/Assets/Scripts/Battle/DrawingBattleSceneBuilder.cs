using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace SketchBlossom.Battle
{
    /// <summary>
    /// Editor script to build the DrawingBattleScene programmatically.
    /// Attach this to an empty GameObject and click "Build Scene" in the Inspector.
    /// </summary>
    public class DrawingBattleSceneBuilder : MonoBehaviour
    {
        [Header("Build Settings")]
        [Tooltip("Click to build the entire battle scene")]
        [SerializeField] private bool buildScene = false;

        [Header("References (Auto-Assigned)")]
        [SerializeField] private Canvas mainCanvas;
        [SerializeField] private DrawingBattleSceneManager battleManager;

        // Created components (for wiring up references)
        private BattleDrawingCanvas createdDrawingCanvas;
        private BattleHPBar createdPlayerHPBar;
        private BattleHPBar createdEnemyHPBar;
        private Button createdFinishButton;
        private Button createdClearButton;
        private Button createdGuideButton;
        private GameObject createdGuidePanel;
        private TextMeshProUGUI createdTurnIndicator;
        private TextMeshProUGUI createdActionText;
        private TextMeshProUGUI createdAvailableMovesText;
        private Image createdPlayerSprite;
        private Image createdEnemySprite;
        private TextMeshProUGUI createdPlayerName;
        private TextMeshProUGUI createdEnemyName;

        private void OnValidate()
        {
            if (buildScene)
            {
                buildScene = false;
                BuildBattleScene();
            }
        }

        /// <summary>
        /// Build the complete battle scene
        /// </summary>
        [ContextMenu("Build Battle Scene")]
        public void BuildBattleScene()
        {
            Debug.Log("=== Building Drawing Battle Scene ===");

            // Create main canvas
            CreateMainCanvas();

            // Create battle manager
            CreateBattleManager();

            // Create combat field
            CreateCombatField();

            // Create drawing area
            CreateDrawingArea();

            // Create UI elements
            CreateUIElements();

            // Wire up all references to BattleManager
            WireUpReferences();

            Debug.Log("=== Battle Scene Built Successfully! ===");
            Debug.Log("All references connected to BattleManager");
        }

        /// <summary>
        /// Create the main canvas
        /// </summary>
        private void CreateMainCanvas()
        {
            GameObject canvasObj = GameObject.Find("BattleCanvas");
            if (canvasObj == null)
            {
                canvasObj = new GameObject("BattleCanvas");
                mainCanvas = canvasObj.AddComponent<Canvas>();
                mainCanvas.renderMode = RenderMode.ScreenSpaceOverlay;

                // Configure CanvasScaler for proper screen scaling
                CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920, 1080);
                scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
                scaler.matchWidthOrHeight = 0.5f; // Balance between width and height

                canvasObj.AddComponent<GraphicRaycaster>();

                Debug.Log("Created BattleCanvas with proper scaling");
            }
            else
            {
                mainCanvas = canvasObj.GetComponent<Canvas>();

                // Configure existing canvas scaler if needed
                CanvasScaler scaler = canvasObj.GetComponent<CanvasScaler>();
                if (scaler != null)
                {
                    scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                    scaler.referenceResolution = new Vector2(1920, 1080);
                    scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
                    scaler.matchWidthOrHeight = 0.5f;
                }

                Debug.Log("Using existing BattleCanvas");
            }
        }

        /// <summary>
        /// Create battle manager GameObject
        /// </summary>
        private void CreateBattleManager()
        {
            GameObject managerObj = GameObject.Find("BattleManager");
            if (managerObj == null)
            {
                managerObj = new GameObject("BattleManager");
                battleManager = managerObj.AddComponent<DrawingBattleSceneManager>();

                // Add required components
                var moveDetector = managerObj.AddComponent<MovesetDetector>();
                var moveRecognition = managerObj.AddComponent<MoveRecognitionSystem>();
                moveDetector.recognitionSystem = moveRecognition;

                Debug.Log("Created BattleManager with MovesetDetector and MoveRecognitionSystem");
            }
            else
            {
                battleManager = managerObj.GetComponent<DrawingBattleSceneManager>();
                Debug.Log("Using existing BattleManager");
            }
        }

        /// <summary>
        /// Create the combat field with player and enemy units
        /// </summary>
        private void CreateCombatField()
        {
            GameObject combatField = new GameObject("CombatField");
            combatField.transform.SetParent(mainCanvas.transform);
            RectTransform rt = combatField.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0, 0.4f);
            rt.anchorMax = new Vector2(1, 0.9f);
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            // Create player area (left side)
            CreatePlayerArea(combatField.transform);

            // Create enemy area (right side)
            CreateEnemyArea(combatField.transform);

            Debug.Log("Created CombatField with Player and Enemy areas");
        }

        /// <summary>
        /// Create player unit area
        /// </summary>
        private void CreatePlayerArea(Transform parent)
        {
            GameObject playerArea = new GameObject("PlayerArea");
            playerArea.transform.SetParent(parent);
            RectTransform rt = playerArea.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0, 0);
            rt.anchorMax = new Vector2(0.4f, 1);
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            // Player sprite placeholder
            GameObject playerSprite = new GameObject("PlayerSprite");
            playerSprite.transform.SetParent(playerArea.transform);
            RectTransform spriteRT = playerSprite.AddComponent<RectTransform>();
            spriteRT.anchorMin = new Vector2(0.5f, 0.3f);
            spriteRT.anchorMax = new Vector2(0.5f, 0.3f);
            spriteRT.sizeDelta = new Vector2(150, 150);
            spriteRT.anchoredPosition = Vector2.zero;

            Image spriteImage = playerSprite.AddComponent<Image>();
            spriteImage.color = new Color(0.3f, 1f, 0.3f); // Green for grass-type placeholder

            // Player name
            GameObject playerName = CreateTextElement("PlayerName", playerArea.transform, "Your Plant", 24);
            RectTransform nameRT = playerName.GetComponent<RectTransform>();
            nameRT.anchorMin = new Vector2(0.5f, 0.8f);
            nameRT.anchorMax = new Vector2(0.5f, 0.8f);
            nameRT.sizeDelta = new Vector2(200, 30);

            // Player HP Bar
            GameObject playerHPObj = CreateHPBar("PlayerHPBar", playerArea.transform);
            RectTransform hpRT = playerHPObj.GetComponent<RectTransform>();
            hpRT.anchorMin = new Vector2(0.5f, 0.1f);
            hpRT.anchorMax = new Vector2(0.5f, 0.1f);
            hpRT.anchoredPosition = Vector2.zero;
            createdPlayerHPBar = playerHPObj.GetComponent<BattleHPBar>(); // Store reference

            // Store player visuals
            createdPlayerSprite = playerSprite.GetComponent<Image>();
            createdPlayerName = playerName.GetComponent<TextMeshProUGUI>();
        }

        /// <summary>
        /// Create enemy unit area
        /// </summary>
        private void CreateEnemyArea(Transform parent)
        {
            GameObject enemyArea = new GameObject("EnemyArea");
            enemyArea.transform.SetParent(parent);
            RectTransform rt = enemyArea.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.6f, 0);
            rt.anchorMax = new Vector2(1, 1);
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            // Enemy sprite placeholder
            GameObject enemySprite = new GameObject("EnemySprite");
            enemySprite.transform.SetParent(enemyArea.transform);
            RectTransform spriteRT = enemySprite.AddComponent<RectTransform>();
            spriteRT.anchorMin = new Vector2(0.5f, 0.3f);
            spriteRT.anchorMax = new Vector2(0.5f, 0.3f);
            spriteRT.sizeDelta = new Vector2(150, 150);
            spriteRT.anchoredPosition = Vector2.zero;

            Image spriteImage = enemySprite.AddComponent<Image>();
            spriteImage.color = new Color(1f, 0.3f, 0.3f); // Red for fire-type placeholder

            // Enemy name
            GameObject enemyName = CreateTextElement("EnemyName", enemyArea.transform, "Enemy Plant", 24);
            RectTransform nameRT = enemyName.GetComponent<RectTransform>();
            nameRT.anchorMin = new Vector2(0.5f, 0.8f);
            nameRT.anchorMax = new Vector2(0.5f, 0.8f);
            nameRT.sizeDelta = new Vector2(200, 30);

            // Enemy HP Bar
            GameObject enemyHPObj = CreateHPBar("EnemyHPBar", enemyArea.transform);
            RectTransform hpRT = enemyHPObj.GetComponent<RectTransform>();
            hpRT.anchorMin = new Vector2(0.5f, 0.1f);
            hpRT.anchorMax = new Vector2(0.5f, 0.1f);
            hpRT.anchoredPosition = Vector2.zero;
            createdEnemyHPBar = enemyHPObj.GetComponent<BattleHPBar>(); // Store reference

            // Store enemy visuals
            createdEnemySprite = enemySprite.GetComponent<Image>();
            createdEnemyName = enemyName.GetComponent<TextMeshProUGUI>();
        }

        /// <summary>
        /// Create HP bar UI element
        /// </summary>
        private GameObject CreateHPBar(string name, Transform parent)
        {
            GameObject hpBarObj = new GameObject(name);
            hpBarObj.transform.SetParent(parent);
            RectTransform rt = hpBarObj.AddComponent<RectTransform>();
            rt.sizeDelta = new Vector2(200, 40);
            BattleHPBar hpBar = hpBarObj.AddComponent<BattleHPBar>();

            // Background
            GameObject bg = new GameObject("Background");
            bg.transform.SetParent(hpBarObj.transform);
            RectTransform bgRT = bg.AddComponent<RectTransform>();
            bgRT.anchorMin = Vector2.zero;
            bgRT.anchorMax = Vector2.one;
            bgRT.offsetMin = Vector2.zero;
            bgRT.offsetMax = Vector2.zero;
            Image bgImage = bg.AddComponent<Image>();
            bgImage.color = new Color(0.2f, 0.2f, 0.2f);

            // Fill (HP)
            GameObject fill = new GameObject("Fill");
            fill.transform.SetParent(hpBarObj.transform);
            RectTransform fillRT = fill.AddComponent<RectTransform>();
            fillRT.anchorMin = Vector2.zero;
            fillRT.anchorMax = Vector2.one;
            fillRT.offsetMin = new Vector2(2, 2);
            fillRT.offsetMax = new Vector2(-2, -20);
            Image fillImage = fill.AddComponent<Image>();
            fillImage.color = Color.green;
            fillImage.type = Image.Type.Filled;
            fillImage.fillMethod = Image.FillMethod.Horizontal;

            // HP Text
            GameObject hpText = CreateTextElement("HPText", hpBarObj.transform, "100 / 100", 14);
            RectTransform textRT = hpText.GetComponent<RectTransform>();
            textRT.anchorMin = new Vector2(0, 0);
            textRT.anchorMax = new Vector2(1, 0.5f);
            textRT.offsetMin = Vector2.zero;
            textRT.offsetMax = Vector2.zero;
            hpText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;

            Debug.Log($"Created {name}");
            return hpBarObj;
        }

        /// <summary>
        /// Create the drawing area
        /// </summary>
        private void CreateDrawingArea()
        {
            GameObject drawingArea = new GameObject("DrawingArea");
            drawingArea.transform.SetParent(mainCanvas.transform);
            RectTransform rt = drawingArea.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.1f, 0.05f);
            rt.anchorMax = new Vector2(0.9f, 0.35f);
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            // Background - MUST be raycast target for drawing to work!
            Image bg = drawingArea.AddComponent<Image>();
            bg.color = new Color(0.95f, 0.95f, 0.95f);
            bg.raycastTarget = true; // Enable raycasting for mouse/touch input

            // Add BattleDrawingCanvas component
            BattleDrawingCanvas canvas = drawingArea.AddComponent<BattleDrawingCanvas>();
            createdDrawingCanvas = canvas; // Store reference

            // Border - needs Image component for Outline to work
            GameObject border = new GameObject("Border");
            border.transform.SetParent(drawingArea.transform);
            RectTransform borderRT = border.AddComponent<RectTransform>();
            borderRT.anchorMin = Vector2.zero;
            borderRT.anchorMax = Vector2.one;
            borderRT.offsetMin = Vector2.zero;
            borderRT.offsetMax = Vector2.zero;

            // Add Image for the outline to attach to
            Image borderImage = border.AddComponent<Image>();
            borderImage.color = new Color(1, 1, 1, 0); // Transparent
            borderImage.raycastTarget = false; // Don't block raycasts

            Outline outline = border.AddComponent<Outline>();
            outline.effectColor = Color.black;
            outline.effectDistance = new Vector2(3, 3);

            Debug.Log("Created DrawingArea with BattleDrawingCanvas (raycast enabled)");
        }

        /// <summary>
        /// Create UI elements (buttons, text, etc.)
        /// </summary>
        private void CreateUIElements()
        {
            // Turn Indicator
            GameObject turnIndicator = CreateTextElement("TurnIndicator", mainCanvas.transform, "YOUR TURN", 32);
            RectTransform turnRT = turnIndicator.GetComponent<RectTransform>();
            turnRT.anchorMin = new Vector2(0.5f, 0.95f);
            turnRT.anchorMax = new Vector2(0.5f, 0.95f);
            turnRT.sizeDelta = new Vector2(300, 50);
            turnIndicator.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
            turnIndicator.GetComponent<TextMeshProUGUI>().color = Color.yellow;
            createdTurnIndicator = turnIndicator.GetComponent<TextMeshProUGUI>(); // Store reference

            // Action Text
            GameObject actionText = CreateTextElement("ActionText", mainCanvas.transform, "Draw your move!", 20);
            RectTransform actionRT = actionText.GetComponent<RectTransform>();
            actionRT.anchorMin = new Vector2(0.5f, 0.38f);
            actionRT.anchorMax = new Vector2(0.5f, 0.38f);
            actionRT.sizeDelta = new Vector2(400, 30);
            actionText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
            actionText.GetComponent<TextMeshProUGUI>().color = Color.white; // Make sure it's visible
            createdActionText = actionText.GetComponent<TextMeshProUGUI>(); // Store reference

            // Available Moves Text
            GameObject movesText = CreateTextElement("AvailableMovesText", mainCanvas.transform, "Available Moves:\n- Move 1\n- Move 2", 16);
            RectTransform movesRT = movesText.GetComponent<RectTransform>();
            movesRT.anchorMin = new Vector2(0, 0.4f);
            movesRT.anchorMax = new Vector2(0, 0.4f);
            movesRT.sizeDelta = new Vector2(200, 150);
            movesRT.anchoredPosition = new Vector2(100, 0);
            createdAvailableMovesText = movesText.GetComponent<TextMeshProUGUI>(); // Store reference

            // Finish Drawing Button
            GameObject finishButton = CreateButton("FinishDrawingButton", mainCanvas.transform, "Finish Drawing");
            RectTransform finishRT = finishButton.GetComponent<RectTransform>();
            finishRT.anchorMin = new Vector2(0.7f, 0.02f);
            finishRT.anchorMax = new Vector2(0.7f, 0.02f);
            finishRT.sizeDelta = new Vector2(180, 50);
            createdFinishButton = finishButton.GetComponent<Button>(); // Store reference

            // Clear Drawing Button
            GameObject clearButton = CreateButton("ClearDrawingButton", mainCanvas.transform, "Clear");
            RectTransform clearRT = clearButton.GetComponent<RectTransform>();
            clearRT.anchorMin = new Vector2(0.3f, 0.02f);
            clearRT.anchorMax = new Vector2(0.3f, 0.02f);
            clearRT.sizeDelta = new Vector2(120, 50);
            createdClearButton = clearButton.GetComponent<Button>(); // Store reference

            // Guide Book Button (below available moves)
            GameObject guideButton = CreateButton("GuideBookButton", mainCanvas.transform, "How to Draw");
            RectTransform guideRT = guideButton.GetComponent<RectTransform>();
            guideRT.anchorMin = new Vector2(0, 0.4f);
            guideRT.anchorMax = new Vector2(0, 0.4f);
            guideRT.sizeDelta = new Vector2(160, 40);
            guideRT.anchoredPosition = new Vector2(100, -100);
            createdGuideButton = guideButton.GetComponent<Button>(); // Store reference

            // Create the guide panel
            CreateGuidePanel();

            Debug.Log("Created UI elements (buttons and text)");
        }

        /// <summary>
        /// Helper: Create a text element
        /// </summary>
        private GameObject CreateTextElement(string name, Transform parent, string text, int fontSize)
        {
            GameObject textObj = new GameObject(name);
            textObj.transform.SetParent(parent);
            RectTransform rt = textObj.AddComponent<RectTransform>();

            TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.color = Color.black;
            tmp.alignment = TextAlignmentOptions.Left;

            return textObj;
        }

        /// <summary>
        /// Helper: Create a button
        /// </summary>
        private GameObject CreateButton(string name, Transform parent, string buttonText)
        {
            GameObject buttonObj = new GameObject(name);
            buttonObj.transform.SetParent(parent);
            RectTransform rt = buttonObj.AddComponent<RectTransform>();

            Image image = buttonObj.AddComponent<Image>();
            image.color = new Color(0.2f, 0.7f, 1f);

            Button button = buttonObj.AddComponent<Button>();

            // Button text
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(buttonObj.transform);
            RectTransform textRT = textObj.AddComponent<RectTransform>();
            textRT.anchorMin = Vector2.zero;
            textRT.anchorMax = Vector2.one;
            textRT.offsetMin = Vector2.zero;
            textRT.offsetMax = Vector2.zero;

            TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
            tmp.text = buttonText;
            tmp.fontSize = 18;
            tmp.color = Color.white;
            tmp.alignment = TextAlignmentOptions.Center;

            return buttonObj;
        }

        /// <summary>
        /// Create the drawing guide panel
        /// </summary>
        private void CreateGuidePanel()
        {
            // Main panel (full screen overlay)
            GameObject guidePanel = new GameObject("GuidePanel");
            guidePanel.transform.SetParent(mainCanvas.transform);
            RectTransform panelRT = guidePanel.AddComponent<RectTransform>();
            panelRT.anchorMin = Vector2.zero;
            panelRT.anchorMax = Vector2.one;
            panelRT.offsetMin = Vector2.zero;
            panelRT.offsetMax = Vector2.zero;

            // Background overlay (semi-transparent)
            Image panelBg = guidePanel.AddComponent<Image>();
            panelBg.color = new Color(0, 0, 0, 0.8f);

            // Content panel
            GameObject contentPanel = new GameObject("ContentPanel");
            contentPanel.transform.SetParent(guidePanel.transform);
            RectTransform contentRT = contentPanel.AddComponent<RectTransform>();
            contentRT.anchorMin = new Vector2(0.15f, 0.1f);
            contentRT.anchorMax = new Vector2(0.85f, 0.9f);
            contentRT.offsetMin = Vector2.zero;
            contentRT.offsetMax = Vector2.zero;

            Image contentBg = contentPanel.AddComponent<Image>();
            contentBg.color = new Color(0.9f, 0.85f, 0.7f); // Parchment color

            // Title
            GameObject titleObj = CreateTextElement("Title", contentPanel.transform, "Drawing Guide", 32);
            RectTransform titleRT = titleObj.GetComponent<RectTransform>();
            titleRT.anchorMin = new Vector2(0.5f, 0.92f);
            titleRT.anchorMax = new Vector2(0.5f, 0.92f);
            titleRT.sizeDelta = new Vector2(400, 50);
            titleObj.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
            titleObj.GetComponent<TextMeshProUGUI>().fontStyle = FontStyles.Bold;

            // Instructions text
            string instructions = "Draw the following patterns to execute moves:\n";
            GameObject instructionsObj = CreateTextElement("Instructions", contentPanel.transform, instructions, 18);
            RectTransform instructionsRT = instructionsObj.GetComponent<RectTransform>();
            instructionsRT.anchorMin = new Vector2(0.5f, 0.85f);
            instructionsRT.anchorMax = new Vector2(0.5f, 0.85f);
            instructionsRT.sizeDelta = new Vector2(700, 40);
            instructionsObj.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;

            // Move guides
            CreateMoveGuides(contentPanel.transform);

            // Close button
            GameObject closeButton = CreateButton("CloseButton", contentPanel.transform, "Close");
            RectTransform closeRT = closeButton.GetComponent<RectTransform>();
            closeRT.anchorMin = new Vector2(0.5f, 0.05f);
            closeRT.anchorMax = new Vector2(0.5f, 0.05f);
            closeRT.sizeDelta = new Vector2(150, 45);

            createdGuidePanel = guidePanel;
            guidePanel.SetActive(false); // Hidden by default

            Debug.Log("Created GuidePanel");
        }

        /// <summary>
        /// Create visual guides for each move type
        /// </summary>
        private void CreateMoveGuides(Transform parent)
        {
            // Create a grid layout for move guides
            string[,] moveGuides = new string[,]
            {
                { "Block", "Draw any simple shape\n(Square, Circle, etc.)" },
                { "Fireball", "Draw a circle or spiral" },
                { "Burn", "Draw zigzag or spiky lines" },
                { "Flame Strike", "Draw vertical wavy line" },
                { "Vine Whip", "Draw a curved S-shape" },
                { "Leaf Storm", "Draw many small scattered marks" },
                { "Root Attack", "Draw vertical lines downward" },
                { "Water Splash", "Draw wavy horizontal lines" },
                { "Bubble", "Draw small circles (2-3)" },
                { "Healing Wave", "Draw smooth horizontal wave" }
            };

            int rows = 5;
            int cols = 2;
            float startY = 0.75f;
            float spacing = 0.15f;

            for (int i = 0; i < Mathf.Min(moveGuides.GetLength(0), rows * cols); i++)
            {
                int row = i / cols;
                int col = i % cols;

                float xPos = col == 0 ? 0.25f : 0.75f;
                float yPos = startY - (row * spacing);

                string moveName = moveGuides[i, 0];
                string description = moveGuides[i, 1];

                CreateMoveGuideEntry(parent, moveName, description, xPos, yPos);
            }
        }

        /// <summary>
        /// Create a single move guide entry
        /// </summary>
        private void CreateMoveGuideEntry(Transform parent, string moveName, string description, float xPos, float yPos)
        {
            GameObject entry = new GameObject($"Guide_{moveName}");
            entry.transform.SetParent(parent);
            RectTransform entryRT = entry.AddComponent<RectTransform>();
            entryRT.anchorMin = new Vector2(xPos, yPos);
            entryRT.anchorMax = new Vector2(xPos, yPos);
            entryRT.sizeDelta = new Vector2(300, 100);

            // Move name (bold)
            GameObject nameObj = CreateTextElement("Name", entry.transform, moveName, 16);
            RectTransform nameRT = nameObj.GetComponent<RectTransform>();
            nameRT.anchorMin = new Vector2(0, 0.6f);
            nameRT.anchorMax = new Vector2(1, 1f);
            nameRT.offsetMin = Vector2.zero;
            nameRT.offsetMax = Vector2.zero;
            nameObj.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.TopLeft;
            nameObj.GetComponent<TextMeshProUGUI>().fontStyle = FontStyles.Bold;
            nameObj.GetComponent<TextMeshProUGUI>().color = new Color(0.2f, 0.1f, 0.05f);

            // Description
            GameObject descObj = CreateTextElement("Description", entry.transform, description, 12);
            RectTransform descRT = descObj.GetComponent<RectTransform>();
            descRT.anchorMin = new Vector2(0, 0f);
            descRT.anchorMax = new Vector2(1, 0.6f);
            descRT.offsetMin = Vector2.zero;
            descRT.offsetMax = Vector2.zero;
            descObj.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.TopLeft;
            descObj.GetComponent<TextMeshProUGUI>().color = new Color(0.3f, 0.2f, 0.1f);
        }

        /// <summary>
        /// Wire up all references to the BattleManager using reflection
        /// </summary>
        private void WireUpReferences()
        {
            if (battleManager == null)
            {
                Debug.LogError("BattleManager not found! Cannot wire up references.");
                return;
            }

            // Use reflection to assign private serialized fields
            var managerType = typeof(DrawingBattleSceneManager);

            // Drawing System
            SetPrivateField(managerType, "drawingCanvas", createdDrawingCanvas);
            SetPrivateField(managerType, "finishDrawingButton", createdFinishButton);
            SetPrivateField(managerType, "clearDrawingButton", createdClearButton);
            SetPrivateField(managerType, "guideBookButton", createdGuideButton);
            SetPrivateField(managerType, "guidePanel", createdGuidePanel);

            // HP Bars
            SetPrivateField(managerType, "playerHPBar", createdPlayerHPBar);
            SetPrivateField(managerType, "enemyHPBar", createdEnemyHPBar);

            // UI Elements
            SetPrivateField(managerType, "turnIndicatorText", createdTurnIndicator);
            SetPrivateField(managerType, "actionText", createdActionText);
            SetPrivateField(managerType, "availableMovesText", createdAvailableMovesText);

            // Move Detection (already wired in CreateBattleManager)
            var moveDetector = battleManager.GetComponent<MovesetDetector>();
            var moveRecognition = battleManager.GetComponent<MoveRecognitionSystem>();
            SetPrivateField(managerType, "movesetDetector", moveDetector);
            SetPrivateField(managerType, "moveRecognitionSystem", moveRecognition);

            Debug.Log("✅ All references wired to BattleManager!");
            Debug.Log($"  - Drawing Canvas: {createdDrawingCanvas != null}");
            Debug.Log($"  - Action Text: {createdActionText != null}");
            Debug.Log($"  - HP Bars: {createdPlayerHPBar != null} / {createdEnemyHPBar != null}");
            Debug.Log($"  - Buttons: {createdFinishButton != null} / {createdClearButton != null}");
        }

        /// <summary>
        /// Helper to set private serialized fields using reflection
        /// </summary>
        private void SetPrivateField(System.Type type, string fieldName, object value)
        {
            var field = type.GetField(fieldName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (field != null)
            {
                field.SetValue(battleManager, value);
            }
            else
            {
                Debug.LogWarning($"Field '{fieldName}' not found in {type.Name}");
            }
        }

        /// <summary>
        /// Fix existing scene by finding and wiring up all references
        /// Use this instead of rebuilding the entire scene
        /// </summary>
        [ContextMenu("Fix Existing Scene (No Rebuild)")]
        public void FixExistingScene()
        {
            Debug.Log("=== FIXING EXISTING SCENE ===");

            // Find BattleManager
            battleManager = FindObjectOfType<DrawingBattleSceneManager>();
            if (battleManager == null)
            {
                Debug.LogError("❌ BattleManager not found in scene! Add DrawingBattleSceneManager component first.");
                return;
            }

            // Find all existing components by name
            createdDrawingCanvas = FindComponentByName<BattleDrawingCanvas>("DrawingArea");
            createdPlayerHPBar = FindComponentByName<BattleHPBar>("PlayerHPBar");
            createdEnemyHPBar = FindComponentByName<BattleHPBar>("EnemyHPBar");
            createdFinishButton = FindComponentByName<Button>("FinishDrawingButton");
            createdClearButton = FindComponentByName<Button>("ClearDrawingButton");
            createdGuideButton = FindComponentByName<Button>("GuideBookButton");
            createdTurnIndicator = FindComponentByName<TextMeshProUGUI>("TurnIndicator");
            createdActionText = FindComponentByName<TextMeshProUGUI>("ActionText");
            createdAvailableMovesText = FindComponentByName<TextMeshProUGUI>("AvailableMovesText");

            // Find guide panel
            GameObject guidePanelObj = GameObject.Find("GuidePanel");
            if (guidePanelObj != null)
            {
                createdGuidePanel = guidePanelObj;
                Debug.Log("✅ Found GuidePanel");
            }

            // Wire them all up
            WireUpReferences();

            Debug.Log("✅ EXISTING SCENE FIXED!");
            Debug.Log("All references connected. Drawing and action text should now work!");
        }

        /// <summary>
        /// Helper to find a component by GameObject name
        /// </summary>
        private T FindComponentByName<T>(string gameObjectName) where T : Component
        {
            GameObject obj = GameObject.Find(gameObjectName);
            if (obj != null)
            {
                T component = obj.GetComponent<T>();
                if (component != null)
                {
                    Debug.Log($"✅ Found {typeof(T).Name} in '{gameObjectName}'");
                    return component;
                }
                else
                {
                    Debug.LogWarning($"⚠ Found '{gameObjectName}' but no {typeof(T).Name} component");
                }
            }
            else
            {
                Debug.LogWarning($"⚠ GameObject '{gameObjectName}' not found");
            }
            return null;
        }

        /// <summary>
        /// Create EventSystem if it doesn't exist
        /// </summary>
        [ContextMenu("Create EventSystem")]
        public void CreateEventSystem()
        {
            if (GameObject.Find("EventSystem") == null)
            {
                GameObject eventSystem = new GameObject("EventSystem");
                eventSystem.AddComponent<UnityEngine.EventSystems.EventSystem>();
                eventSystem.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
                Debug.Log("Created EventSystem");
            }
        }

        /// <summary>
        /// Create Main Camera if it doesn't exist
        /// </summary>
        [ContextMenu("Create Main Camera")]
        public void CreateMainCamera()
        {
            if (Camera.main == null)
            {
                GameObject cameraObj = new GameObject("Main Camera");
                Camera cam = cameraObj.AddComponent<Camera>();
                cam.tag = "MainCamera";
                cam.clearFlags = CameraClearFlags.SolidColor;
                cam.backgroundColor = new Color(0.8f, 0.9f, 1f); // Light blue sky
                Debug.Log("Created Main Camera");
            }
        }
    }
}
