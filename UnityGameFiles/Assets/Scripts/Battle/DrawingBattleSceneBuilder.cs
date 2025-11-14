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
        private GuideBookManager createdGuideBookManager;
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

            // Create GuideBookManager
            CreateGuideBookManager();
        }

        /// <summary>
        /// Create the GuideBookManager GameObject and component
        /// </summary>
        private void CreateGuideBookManager()
        {
            GameObject guideManagerObj = GameObject.Find("GuideBookManager");
            if (guideManagerObj == null)
            {
                guideManagerObj = new GameObject("GuideBookManager");
                createdGuideBookManager = guideManagerObj.AddComponent<GuideBookManager>();
                Debug.Log("Created GuideBookManager");
            }
            else
            {
                createdGuideBookManager = guideManagerObj.GetComponent<GuideBookManager>();
                if (createdGuideBookManager == null)
                {
                    createdGuideBookManager = guideManagerObj.AddComponent<GuideBookManager>();
                }
                Debug.Log("Using existing GuideBookManager");
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

            // Create pages
            CreateGuidePage0_Introduction(contentPanel.transform); // Page 0: Introduction
            CreateGuidePage1_FireMoves(contentPanel.transform);     // Page 1: Fire Moves
            CreateGuidePage2_GrassMoves(contentPanel.transform);    // Page 2: Grass Moves
            CreateGuidePage3_WaterMoves(contentPanel.transform);    // Page 3: Water Moves

            // Page indicator
            GameObject pageIndicator = CreateTextElement("PageIndicator", contentPanel.transform, "Page 1 / 4", 16);
            RectTransform pageRT = pageIndicator.GetComponent<RectTransform>();
            pageRT.anchorMin = new Vector2(0.5f, 0.12f);
            pageRT.anchorMax = new Vector2(0.5f, 0.12f);
            pageRT.sizeDelta = new Vector2(200, 30);
            pageIndicator.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;

            // Previous Page button
            GameObject prevButton = CreateButton("PreviousPageButton", contentPanel.transform, "< Prev");
            RectTransform prevRT = prevButton.GetComponent<RectTransform>();
            prevRT.anchorMin = new Vector2(0.15f, 0.05f);
            prevRT.anchorMax = new Vector2(0.15f, 0.05f);
            prevRT.sizeDelta = new Vector2(120, 45);

            // Next Page button
            GameObject nextButton = CreateButton("NextPageButton", contentPanel.transform, "Next >");
            RectTransform nextRT = nextButton.GetComponent<RectTransform>();
            nextRT.anchorMin = new Vector2(0.85f, 0.05f);
            nextRT.anchorMax = new Vector2(0.85f, 0.05f);
            nextRT.sizeDelta = new Vector2(120, 45);

            // Close button
            GameObject closeButton = CreateButton("CloseButton", contentPanel.transform, "Close");
            RectTransform closeRT = closeButton.GetComponent<RectTransform>();
            closeRT.anchorMin = new Vector2(0.5f, 0.05f);
            closeRT.anchorMax = new Vector2(0.5f, 0.05f);
            closeRT.sizeDelta = new Vector2(120, 45);

            createdGuidePanel = guidePanel;
            guidePanel.SetActive(false); // Hidden by default

            Debug.Log("Created GuidePanel with 4 pages");

            // Wire up the GuideBookManager if it exists
            WireUpGuideBookManager();
        }

        /// <summary>
        /// Create Page 0: Introduction
        /// </summary>
        private void CreateGuidePage0_Introduction(Transform parent)
        {
            GameObject page = new GameObject("Page0_Introduction");
            page.transform.SetParent(parent);
            RectTransform pageRT = page.AddComponent<RectTransform>();
            pageRT.anchorMin = new Vector2(0, 0.15f);
            pageRT.anchorMax = new Vector2(1, 0.85f);
            pageRT.offsetMin = Vector2.zero;
            pageRT.offsetMax = Vector2.zero;

            // Intro text
            string introText = "Welcome to the Drawing Guide!\n\n" +
                "Draw patterns to execute battle moves.\n\n" +
                "The quality of your drawing affects damage:\n" +
                "• Perfect drawing = 1.5x damage\n" +
                "• Good drawing = 1.0x damage\n" +
                "• Poor drawing = 0.5x damage\n\n" +
                "Use Next to see move guides organized by element type.";

            GameObject textObj = CreateTextElement("IntroText", page.transform, introText, 18);
            RectTransform textRT = textObj.GetComponent<RectTransform>();
            textRT.anchorMin = Vector2.zero;
            textRT.anchorMax = Vector2.one;
            textRT.offsetMin = new Vector2(50, 0);
            textRT.offsetMax = new Vector2(-50, 0);
            textObj.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
            textObj.GetComponent<TextMeshProUGUI>().color = new Color(0.2f, 0.1f, 0.05f);
        }

        /// <summary>
        /// Create Page 1: Fire Moves
        /// </summary>
        private void CreateGuidePage1_FireMoves(Transform parent)
        {
            GameObject page = new GameObject("Page1_FireMoves");
            page.transform.SetParent(parent);
            RectTransform pageRT = page.AddComponent<RectTransform>();
            pageRT.anchorMin = new Vector2(0, 0.15f);
            pageRT.anchorMax = new Vector2(1, 0.85f);
            pageRT.offsetMin = Vector2.zero;
            pageRT.offsetMax = Vector2.zero;

            // Page title
            GameObject titleObj = CreateTextElement("PageTitle", page.transform, "🔥 Fire Moves", 24);
            RectTransform titleRT = titleObj.GetComponent<RectTransform>();
            titleRT.anchorMin = new Vector2(0.5f, 0.9f);
            titleRT.anchorMax = new Vector2(0.5f, 0.9f);
            titleRT.sizeDelta = new Vector2(400, 40);
            titleObj.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
            titleObj.GetComponent<TextMeshProUGUI>().fontStyle = FontStyles.Bold;
            titleObj.GetComponent<TextMeshProUGUI>().color = new Color(0.8f, 0.2f, 0.0f);

            // Fire moves
            CreateMoveGuideEntry(page.transform, "Block", "Draw any simple shape\n(Square, Circle, etc.)", 0.5f, 0.7f);
            CreateMoveGuideEntry(page.transform, "Fireball", "Draw a circle or spiral\nClosed loop shape", 0.5f, 0.5f);
            CreateMoveGuideEntry(page.transform, "Burn", "Draw zigzag or spiky lines\nSharp angles", 0.5f, 0.3f);
            CreateMoveGuideEntry(page.transform, "Flame Wave", "Draw wavy horizontal lines\nSmooth curves", 0.5f, 0.1f);
        }

        /// <summary>
        /// Create Page 2: Grass Moves
        /// </summary>
        private void CreateGuidePage2_GrassMoves(Transform parent)
        {
            GameObject page = new GameObject("Page2_GrassMoves");
            page.transform.SetParent(parent);
            RectTransform pageRT = page.AddComponent<RectTransform>();
            pageRT.anchorMin = new Vector2(0, 0.15f);
            pageRT.anchorMax = new Vector2(1, 0.85f);
            pageRT.offsetMin = Vector2.zero;
            pageRT.offsetMax = Vector2.zero;

            // Page title
            GameObject titleObj = CreateTextElement("PageTitle", page.transform, "🌿 Grass Moves", 24);
            RectTransform titleRT = titleObj.GetComponent<RectTransform>();
            titleRT.anchorMin = new Vector2(0.5f, 0.9f);
            titleRT.anchorMax = new Vector2(0.5f, 0.9f);
            titleRT.sizeDelta = new Vector2(400, 40);
            titleObj.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
            titleObj.GetComponent<TextMeshProUGUI>().fontStyle = FontStyles.Bold;
            titleObj.GetComponent<TextMeshProUGUI>().color = new Color(0.2f, 0.7f, 0.1f);

            // Grass moves
            CreateMoveGuideEntry(page.transform, "Block", "Draw any simple shape\n(Square, Circle, etc.)", 0.5f, 0.7f);
            CreateMoveGuideEntry(page.transform, "Vine Whip", "Draw a curved S-shape\nSmooth flowing line", 0.5f, 0.5f);
            CreateMoveGuideEntry(page.transform, "Leaf Storm", "Draw many small scattered marks\n5+ short strokes", 0.5f, 0.3f);
            CreateMoveGuideEntry(page.transform, "Root Attack", "Draw vertical lines downward\nTall aspect ratio", 0.5f, 0.1f);
        }

        /// <summary>
        /// Create Page 3: Water Moves
        /// </summary>
        private void CreateGuidePage3_WaterMoves(Transform parent)
        {
            GameObject page = new GameObject("Page3_WaterMoves");
            page.transform.SetParent(parent);
            RectTransform pageRT = page.AddComponent<RectTransform>();
            pageRT.anchorMin = new Vector2(0, 0.15f);
            pageRT.anchorMax = new Vector2(1, 0.85f);
            pageRT.offsetMin = Vector2.zero;
            pageRT.offsetMax = Vector2.zero;

            // Page title
            GameObject titleObj = CreateTextElement("PageTitle", page.transform, "💧 Water Moves", 24);
            RectTransform titleRT = titleObj.GetComponent<RectTransform>();
            titleRT.anchorMin = new Vector2(0.5f, 0.9f);
            titleRT.anchorMax = new Vector2(0.5f, 0.9f);
            titleRT.sizeDelta = new Vector2(400, 40);
            titleObj.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
            titleObj.GetComponent<TextMeshProUGUI>().fontStyle = FontStyles.Bold;
            titleObj.GetComponent<TextMeshProUGUI>().color = new Color(0.1f, 0.5f, 0.9f);

            // Water moves
            CreateMoveGuideEntry(page.transform, "Block", "Draw any simple shape\n(Square, Circle, etc.)", 0.5f, 0.7f);
            CreateMoveGuideEntry(page.transform, "Water Splash", "Draw wavy horizontal lines\n2-5 curved strokes", 0.5f, 0.5f);
            CreateMoveGuideEntry(page.transform, "Bubble", "Draw small circles (2-3)\nCompact circular shapes", 0.5f, 0.3f);
            CreateMoveGuideEntry(page.transform, "Healing Wave", "Draw smooth horizontal wave\nGentle flowing curve", 0.5f, 0.1f);
        }

        /// <summary>
        /// Wire up the GuideBookManager with all necessary references
        /// </summary>
        private void WireUpGuideBookManager()
        {
            if (createdGuideBookManager == null)
            {
                Debug.LogWarning("GuideBookManager not found, cannot wire up references");
                return;
            }

            // Use reflection to set the private fields
            var managerType = typeof(GuideBookManager);

            SetPrivateFieldForManager(managerType, createdGuideBookManager, "openGuideButton", createdGuideButton);
            SetPrivateFieldForManager(managerType, createdGuideBookManager, "guidePanel", createdGuidePanel);

            // Find the close button
            if (createdGuidePanel != null)
            {
                Button closeButton = null;
                Button[] buttons = createdGuidePanel.GetComponentsInChildren<Button>(true);
                foreach (Button btn in buttons)
                {
                    if (btn.name == "CloseButton")
                    {
                        closeButton = btn;
                        break;
                    }
                }

                if (closeButton != null)
                {
                    SetPrivateFieldForManager(managerType, createdGuideBookManager, "closeGuideButton", closeButton);
                }
            }

            Debug.Log("✅ GuideBookManager wired up successfully!");
        }

        /// <summary>
        /// Helper to set private serialized fields for GuideBookManager using reflection
        /// </summary>
        private void SetPrivateFieldForManager(System.Type type, object instance, string fieldName, object value)
        {
            var field = type.GetField(fieldName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (field != null)
            {
                field.SetValue(instance, value);
                Debug.Log($"  ✅ Set {fieldName} = {value != null}");
            }
            else
            {
                Debug.LogWarning($"Field '{fieldName}' not found in {type.Name}");
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
            entryRT.sizeDelta = new Vector2(600, 80);

            // Background
            Image entryBg = entry.AddComponent<Image>();
            entryBg.color = new Color(0.85f, 0.8f, 0.65f, 0.3f);

            // Move name (bold)
            GameObject nameObj = CreateTextElement("Name", entry.transform, moveName, 18);
            RectTransform nameRT = nameObj.GetComponent<RectTransform>();
            nameRT.anchorMin = new Vector2(0, 0.5f);
            nameRT.anchorMax = new Vector2(0.4f, 1f);
            nameRT.offsetMin = new Vector2(10, 0);
            nameRT.offsetMax = new Vector2(0, -5);
            nameObj.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.MidlineLeft;
            nameObj.GetComponent<TextMeshProUGUI>().fontStyle = FontStyles.Bold;
            nameObj.GetComponent<TextMeshProUGUI>().color = new Color(0.2f, 0.1f, 0.05f);

            // Description
            GameObject descObj = CreateTextElement("Description", entry.transform, description, 14);
            RectTransform descRT = descObj.GetComponent<RectTransform>();
            descRT.anchorMin = new Vector2(0.4f, 0f);
            descRT.anchorMax = new Vector2(1, 1f);
            descRT.offsetMin = new Vector2(10, 5);
            descRT.offsetMax = new Vector2(-10, -5);
            descObj.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.MidlineLeft;
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

            // Find or create GuideBookManager
            createdGuideBookManager = FindObjectOfType<GuideBookManager>();
            if (createdGuideBookManager == null)
            {
                GameObject guideManagerObj = new GameObject("GuideBookManager");
                createdGuideBookManager = guideManagerObj.AddComponent<GuideBookManager>();
                Debug.Log("✅ Created new GuideBookManager");
            }
            else
            {
                Debug.Log("✅ Found existing GuideBookManager");
            }

            // Wire them all up
            WireUpReferences();

            // Wire up GuideBookManager
            WireUpGuideBookManager();

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

        /// <summary>
        /// Fix guide book setup and wire up GuideBookManager
        /// </summary>
        [ContextMenu("Fix Guide Book Setup")]
        public void FixGuideBookSetup()
        {
            Debug.Log("=== FIXING GUIDE BOOK SETUP ===");

            // Find guide button
            createdGuideButton = FindComponentByName<Button>("GuideBookButton");

            // Find guide panel
            GameObject guidePanelObj = GameObject.Find("GuidePanel");
            if (guidePanelObj != null)
            {
                createdGuidePanel = guidePanelObj;
                Debug.Log("✅ Found GuidePanel");
            }

            // Find or create GuideBookManager
            createdGuideBookManager = FindObjectOfType<GuideBookManager>();
            if (createdGuideBookManager == null)
            {
                GameObject guideManagerObj = new GameObject("GuideBookManager");
                createdGuideBookManager = guideManagerObj.AddComponent<GuideBookManager>();
                Debug.Log("✅ Created new GuideBookManager");
            }
            else
            {
                Debug.Log("✅ Found existing GuideBookManager");
            }

            // Wire up GuideBookManager
            WireUpGuideBookManager();

            Debug.Log("✅ GUIDE BOOK SETUP FIXED!");
        }
    }
}
