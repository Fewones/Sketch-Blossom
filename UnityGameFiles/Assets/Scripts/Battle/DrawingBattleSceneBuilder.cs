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

            Debug.Log("=== Battle Scene Built Successfully! ===");
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

                canvasObj.AddComponent<CanvasScaler>();
                canvasObj.AddComponent<GraphicRaycaster>();

                Debug.Log("Created BattleCanvas");
            }
            else
            {
                mainCanvas = canvasObj.GetComponent<Canvas>();
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

            // Background
            Image bg = drawingArea.AddComponent<Image>();
            bg.color = new Color(0.95f, 0.95f, 0.95f);

            // Add BattleDrawingCanvas component
            BattleDrawingCanvas canvas = drawingArea.AddComponent<BattleDrawingCanvas>();

            // Border
            GameObject border = new GameObject("Border");
            border.transform.SetParent(drawingArea.transform);
            RectTransform borderRT = border.AddComponent<RectTransform>();
            borderRT.anchorMin = Vector2.zero;
            borderRT.anchorMax = Vector2.one;
            borderRT.offsetMin = Vector2.zero;
            borderRT.offsetMax = Vector2.zero;
            Outline outline = border.AddComponent<Outline>();
            outline.effectColor = Color.black;
            outline.effectDistance = new Vector2(2, 2);

            Debug.Log("Created DrawingArea with BattleDrawingCanvas");
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

            // Action Text
            GameObject actionText = CreateTextElement("ActionText", mainCanvas.transform, "Draw your move!", 20);
            RectTransform actionRT = actionText.GetComponent<RectTransform>();
            actionRT.anchorMin = new Vector2(0.5f, 0.38f);
            actionRT.anchorMax = new Vector2(0.5f, 0.38f);
            actionRT.sizeDelta = new Vector2(400, 30);
            actionText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;

            // Available Moves Text
            GameObject movesText = CreateTextElement("AvailableMovesText", mainCanvas.transform, "Available Moves:\n- Move 1\n- Move 2", 16);
            RectTransform movesRT = movesText.GetComponent<RectTransform>();
            movesRT.anchorMin = new Vector2(0, 0.4f);
            movesRT.anchorMax = new Vector2(0, 0.4f);
            movesRT.sizeDelta = new Vector2(200, 150);
            movesRT.anchoredPosition = new Vector2(100, 0);

            // Finish Drawing Button
            GameObject finishButton = CreateButton("FinishDrawingButton", mainCanvas.transform, "Finish Drawing");
            RectTransform finishRT = finishButton.GetComponent<RectTransform>();
            finishRT.anchorMin = new Vector2(0.7f, 0.02f);
            finishRT.anchorMax = new Vector2(0.7f, 0.02f);
            finishRT.sizeDelta = new Vector2(180, 50);

            // Clear Drawing Button
            GameObject clearButton = CreateButton("ClearDrawingButton", mainCanvas.transform, "Clear");
            RectTransform clearRT = clearButton.GetComponent<RectTransform>();
            clearRT.anchorMin = new Vector2(0.3f, 0.02f);
            clearRT.anchorMax = new Vector2(0.3f, 0.02f);
            clearRT.sizeDelta = new Vector2(120, 50);

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
