using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

namespace SketchBlossom.Battle
{
    /// <summary>
    /// Main manager for the Drawing Battle Scene.
    /// Handles turn-based combat where players draw their moves.
    /// </summary>
    public class DrawingBattleSceneManager : MonoBehaviour
    {
        [Header("Unit Setup")]
        [SerializeField] private BattleUnitDisplay playerUnit;
        [SerializeField] private BattleUnitDisplay enemyUnit;

        [Header("HP Bars")]
        [SerializeField] private BattleHPBar playerHPBar;
        [SerializeField] private BattleHPBar enemyHPBar;

        [Header("Drawing System")]
        [SerializeField] private BattleDrawingCanvas drawingCanvas;
        [SerializeField] private Button finishDrawingButton;
        [SerializeField] private Button clearDrawingButton;

        [Header("Move Detection")]
        [SerializeField] private MovesetDetector movesetDetector;
        [SerializeField] private MoveRecognitionSystem moveRecognitionSystem;

        [Header("UI Elements")]
        [SerializeField] private TextMeshProUGUI turnIndicatorText;
        [SerializeField] private TextMeshProUGUI actionText;
        [SerializeField] private TextMeshProUGUI availableMovesText;
        [SerializeField] private GameObject playerTurnPanel;
        [SerializeField] private GameObject enemyTurnPanel;

        [Header("Battle Settings")]
        [SerializeField] private float turnDelay = 1.5f;
        [SerializeField] private float actionTextDelay = 2f;

        // Battle state
        private enum BattleState
        {
            Start,
            PlayerTurn,
            PlayerDrawing,
            PlayerExecuting,
            EnemyTurn,
            Victory,
            Defeat
        }

        private BattleState currentState = BattleState.Start;

        // Unit data
        private PlantRecognitionSystem.PlantType playerPlantType;
        private PlantRecognitionSystem.PlantType enemyPlantType;
        private PlantRecognitionSystem.ElementType playerElement;
        private PlantRecognitionSystem.ElementType enemyElement;
        private string playerPlantName;
        private string enemyPlantName;

        // Stats
        private int playerMaxHP;
        private int playerAttack;
        private int playerDefense;
        private int enemyMaxHP;
        private int enemyAttack;
        private int enemyDefense;

        // Combat state
        private bool playerIsBlocking = false;
        private bool enemyIsBlocking = false;

        private void Start()
        {
            InitializeBattle();
        }

        /// <summary>
        /// Initialize the battle with player and enemy units
        /// </summary>
        private void InitializeBattle()
        {
            // Try to load player unit from DrawnUnitData
            LoadPlayerUnit();

            // Create enemy unit
            LoadEnemyUnit();

            // Initialize HP bars
            playerHPBar.Initialize(playerPlantName, playerMaxHP);
            enemyHPBar.Initialize(enemyPlantName, enemyMaxHP);

            // Setup UI
            SetupUI();

            // Setup drawing canvas
            SetupDrawingCanvas();

            // Start battle
            StartCoroutine(BattleSequence());
        }

        /// <summary>
        /// Load player unit from previous drawing scene
        /// </summary>
        private void LoadPlayerUnit()
        {
            Texture2D playerDrawingTexture = null;

            if (DrawnUnitData.Instance != null && DrawnUnitData.Instance.HasData())
            {
                playerPlantType = DrawnUnitData.Instance.plantType;
                playerElement = DrawnUnitData.Instance.element;
                playerPlantName = DrawnUnitData.Instance.plantDisplayName;
                playerDrawingTexture = DrawnUnitData.Instance.drawingTexture;

                var plantData = PlantRecognitionSystem.GetPlantData(playerPlantType);
                playerMaxHP = plantData.baseHP;
                playerAttack = plantData.baseAttack;
                playerDefense = plantData.baseDefense;

                Debug.Log($"Loaded player unit: {playerPlantName} (HP:{playerMaxHP}, ATK:{playerAttack}, DEF:{playerDefense})");

                if (playerDrawingTexture != null)
                {
                    Debug.Log($"✓ Player drawing texture loaded! Size: {playerDrawingTexture.width}x{playerDrawingTexture.height}");
                }
            }
            else
            {
                // Default player unit for testing
                Debug.LogWarning("No player unit data found! Using default Sunflower.");
                playerPlantType = PlantRecognitionSystem.PlantType.Sunflower;
                playerElement = PlantRecognitionSystem.ElementType.Fire;

                var plantData = PlantRecognitionSystem.GetPlantData(playerPlantType);
                playerPlantName = plantData.displayName;
                playerMaxHP = plantData.baseHP;
                playerAttack = plantData.baseAttack;
                playerDefense = plantData.baseDefense;
            }

            // Initialize player unit display with drawing texture
            if (playerUnit != null)
            {
                playerUnit.Initialize(playerPlantType, playerElement, playerPlantName, playerDrawingTexture, true);
            }
        }

        /// <summary>
        /// Load enemy unit (random for now)
        /// </summary>
        private void LoadEnemyUnit()
        {
            // Pick a random enemy
            PlantRecognitionSystem.PlantType[] allPlants = (PlantRecognitionSystem.PlantType[])System.Enum.GetValues(typeof(PlantRecognitionSystem.PlantType));
            enemyPlantType = allPlants[Random.Range(0, allPlants.Length)];

            var plantData = PlantRecognitionSystem.GetPlantData(enemyPlantType);
            enemyPlantName = plantData.displayName;
            enemyElement = plantData.element;
            enemyMaxHP = plantData.baseHP;
            enemyAttack = plantData.baseAttack;
            enemyDefense = plantData.baseDefense;

            Debug.Log($"Enemy unit: {enemyPlantName} (HP:{enemyMaxHP}, ATK:{enemyAttack}, DEF:{enemyDefense})");

            // Initialize enemy unit display (no drawing texture for enemy)
            if (enemyUnit != null)
            {
                enemyUnit.Initialize(enemyPlantType, enemyElement, enemyPlantName, null, false);
            }
        }

        /// <summary>
        /// Setup UI elements
        /// </summary>
        private void SetupUI()
        {
            if (finishDrawingButton != null)
            {
                finishDrawingButton.onClick.AddListener(OnFinishDrawingClicked);
            }

            if (clearDrawingButton != null)
            {
                clearDrawingButton.onClick.AddListener(OnClearDrawingClicked);
            }

            // Display available moves
            UpdateAvailableMovesText();
        }

        /// <summary>
        /// Setup drawing canvas events
        /// </summary>
        private void SetupDrawingCanvas()
        {
            if (drawingCanvas != null)
            {
                drawingCanvas.OnDrawingCompleted += OnDrawingCompleted;
                drawingCanvas.DisableDrawing();
            }
        }

        /// <summary>
        /// Main battle sequence coroutine
        /// </summary>
        private IEnumerator BattleSequence()
        {
            currentState = BattleState.Start;
            UpdateActionText("Battle Start!");
            yield return new WaitForSeconds(turnDelay);

            while (true)
            {
                // Check for victory/defeat
                if (enemyHPBar.GetCurrentHP() <= 0)
                {
                    currentState = BattleState.Victory;
                    yield return StartCoroutine(HandleVictory());
                    yield break;
                }

                if (playerHPBar.GetCurrentHP() <= 0)
                {
                    currentState = BattleState.Defeat;
                    yield return StartCoroutine(HandleDefeat());
                    yield break;
                }

                // Player turn
                currentState = BattleState.PlayerTurn;
                yield return StartCoroutine(PlayerTurn());

                // Check for victory after player turn
                if (enemyHPBar.GetCurrentHP() <= 0)
                {
                    currentState = BattleState.Victory;
                    yield return StartCoroutine(HandleVictory());
                    yield break;
                }

                // Enemy turn
                currentState = BattleState.EnemyTurn;
                yield return StartCoroutine(EnemyTurn());

                // Check for defeat after enemy turn
                if (playerHPBar.GetCurrentHP() <= 0)
                {
                    currentState = BattleState.Defeat;
                    yield return StartCoroutine(HandleDefeat());
                    yield break;
                }
            }
        }

        /// <summary>
        /// Handle player's turn - enable drawing
        /// </summary>
        private IEnumerator PlayerTurn()
        {
            UpdateTurnIndicator("YOUR TURN");
            UpdateActionText("Draw your move!");
            ShowPlayerTurnUI(true);

            // Enable drawing
            currentState = BattleState.PlayerDrawing;
            drawingCanvas.EnableDrawing();
            drawingCanvas.ClearCanvas();

            // Wait for player to finish drawing
            while (currentState == BattleState.PlayerDrawing)
            {
                yield return null;
            }

            // Execute move was already called by OnDrawingCompleted
            while (currentState == BattleState.PlayerExecuting)
            {
                yield return null;
            }

            ShowPlayerTurnUI(false);
            yield return new WaitForSeconds(turnDelay);
        }

        /// <summary>
        /// Handle enemy's turn - AI selects and executes move
        /// </summary>
        private IEnumerator EnemyTurn()
        {
            UpdateTurnIndicator("ENEMY TURN");
            ShowEnemyTurnUI(true);
            yield return new WaitForSeconds(turnDelay);

            // Get enemy moves
            MoveData[] enemyMoves = MoveData.GetMovesForPlant(enemyPlantType);
            if (enemyMoves.Length == 0)
            {
                Debug.LogError("Enemy has no moves!");
                yield break;
            }

            // Simple AI: Pick a random non-defensive move
            MoveData selectedMove = null;
            List<MoveData> offensiveMoves = new List<MoveData>();
            foreach (var move in enemyMoves)
            {
                if (!move.isDefensiveMove && !move.isHealingMove)
                {
                    offensiveMoves.Add(move);
                }
            }

            if (offensiveMoves.Count > 0)
            {
                selectedMove = offensiveMoves[Random.Range(0, offensiveMoves.Count)];
            }
            else
            {
                selectedMove = enemyMoves[0]; // Fallback to first move
            }

            UpdateActionText($"{enemyPlantName} used {selectedMove.moveName}!");

            // Execute enemy move
            ExecuteEnemyMove(selectedMove);

            yield return new WaitForSeconds(actionTextDelay);
            ShowEnemyTurnUI(false);
        }

        /// <summary>
        /// Called when player finishes drawing
        /// </summary>
        private void OnDrawingCompleted(List<List<Vector2>> strokes, Color dominantColor)
        {
            // This is handled by the finish button instead
        }

        /// <summary>
        /// Called when finish drawing button is clicked
        /// </summary>
        private void OnFinishDrawingClicked()
        {
            if (currentState != BattleState.PlayerDrawing) return;

            currentState = BattleState.PlayerExecuting;

            // Get all line renderers from canvas
            List<LineRenderer> lineRenderers = drawingCanvas.GetAllLineRenderers();

            if (lineRenderers.Count == 0)
            {
                UpdateActionText("Draw something first!");
                currentState = BattleState.PlayerDrawing;
                return;
            }

            // Detect the move
            var result = movesetDetector.DetectMove(lineRenderers, playerPlantType);

            if (result.wasRecognized)
            {
                // Execute the move
                StartCoroutine(ExecutePlayerMove(result));
            }
            else
            {
                UpdateActionText("Move not recognized! Try again.");
                drawingCanvas.ClearCanvas();
                currentState = BattleState.PlayerDrawing;
            }
        }

        /// <summary>
        /// Called when clear drawing button is clicked
        /// </summary>
        private void OnClearDrawingClicked()
        {
            if (currentState == BattleState.PlayerDrawing)
            {
                drawingCanvas.ClearCanvas();
            }
        }

        /// <summary>
        /// Execute player's detected move
        /// </summary>
        private IEnumerator ExecutePlayerMove(MovesetDetector.MoveDetectionResult result)
        {
            drawingCanvas.DisableDrawing();

            // Get move data
            MoveData[] playerMoves = MoveData.GetMovesForPlant(playerPlantType);
            MoveData moveData = System.Array.Find(playerMoves, m => m.moveType == result.detectedMove);

            if (moveData == null)
            {
                Debug.LogError($"Move data not found for {result.detectedMove}");
                currentState = BattleState.PlayerTurn;
                yield break;
            }

            UpdateActionText($"You used {moveData.moveName}! (Quality: {result.qualityRating})");
            yield return new WaitForSeconds(1f);

            // Calculate damage
            if (moveData.isDefensiveMove)
            {
                playerIsBlocking = true;
                UpdateActionText("You're defending!");
            }
            else if (moveData.isHealingMove)
            {
                int healAmount = Mathf.RoundToInt(moveData.basePower * result.damageMultiplier);
                playerHPBar.ModifyHP(healAmount);
                UpdateActionText($"Restored {healAmount} HP!");
            }
            else
            {
                // Calculate damage
                int damage = CalculateDamage(
                    moveData.basePower,
                    playerAttack,
                    enemyDefense,
                    GetElementType(playerElement),
                    GetElementType(enemyElement),
                    result.damageMultiplier,
                    enemyIsBlocking
                );

                enemyHPBar.ModifyHP(-damage);
                enemyHPBar.PlayDamageAnimation();
                UpdateActionText($"Dealt {damage} damage!");

                if (enemyUnit != null)
                {
                    enemyUnit.PlayHitAnimation();
                }
            }

            yield return new WaitForSeconds(actionTextDelay);

            // Reset blocking state
            enemyIsBlocking = false;

            currentState = BattleState.EnemyTurn;
        }

        /// <summary>
        /// Execute enemy's move
        /// </summary>
        private void ExecuteEnemyMove(MoveData moveData)
        {
            if (moveData.isDefensiveMove)
            {
                enemyIsBlocking = true;
                UpdateActionText($"{enemyPlantName} is defending!");
            }
            else if (moveData.isHealingMove)
            {
                int healAmount = moveData.basePower;
                enemyHPBar.ModifyHP(healAmount);
                UpdateActionText($"{enemyPlantName} restored {healAmount} HP!");
            }
            else
            {
                // Calculate damage (enemy always has 1.0 quality)
                int damage = CalculateDamage(
                    moveData.basePower,
                    enemyAttack,
                    playerDefense,
                    GetElementType(enemyElement),
                    GetElementType(playerElement),
                    1.0f,
                    playerIsBlocking
                );

                playerHPBar.ModifyHP(-damage);
                playerHPBar.PlayDamageAnimation();
                UpdateActionText($"{enemyPlantName} dealt {damage} damage!");

                if (playerUnit != null)
                {
                    playerUnit.PlayHitAnimation();
                }
            }

            // Reset blocking state
            playerIsBlocking = false;
        }

        /// <summary>
        /// Calculate damage for an attack
        /// </summary>
        private int CalculateDamage(int basePower, int attackStat, int defenseStat,
            MoveData.ElementType attackElement, MoveData.ElementType defenseElement,
            float qualityMultiplier, bool isBlocking)
        {
            // Base damage
            float damage = (basePower + attackStat) * qualityMultiplier;

            // Type advantage
            float typeMultiplier = MoveData.GetTypeAdvantage(attackElement, defenseElement);
            damage *= typeMultiplier;

            // Defense reduction
            float defenseReduction = 1.0f - (defenseStat / 100f);
            defenseReduction = Mathf.Clamp(defenseReduction, 0.5f, 1.0f);
            damage *= defenseReduction;

            // Blocking reduction
            if (isBlocking)
            {
                damage *= 0.5f;
            }

            return Mathf.Max(1, Mathf.RoundToInt(damage));
        }

        /// <summary>
        /// Convert PlantRecognitionSystem.ElementType to MoveData.ElementType
        /// </summary>
        private MoveData.ElementType GetElementType(PlantRecognitionSystem.ElementType element)
        {
            switch (element)
            {
                case PlantRecognitionSystem.ElementType.Fire:
                    return MoveData.ElementType.Fire;
                case PlantRecognitionSystem.ElementType.Grass:
                    return MoveData.ElementType.Grass;
                case PlantRecognitionSystem.ElementType.Water:
                    return MoveData.ElementType.Water;
                default:
                    return MoveData.ElementType.Fire;
            }
        }

        /// <summary>
        /// Handle victory
        /// </summary>
        private IEnumerator HandleVictory()
        {
            UpdateTurnIndicator("VICTORY!");
            UpdateActionText("You win!");
            yield return new WaitForSeconds(3f);

            // Return to main menu or next scene
            SceneManager.LoadScene("MainMenu");
        }

        /// <summary>
        /// Handle defeat
        /// </summary>
        private IEnumerator HandleDefeat()
        {
            UpdateTurnIndicator("DEFEAT");
            UpdateActionText("You lost...");
            yield return new WaitForSeconds(3f);

            // Return to main menu
            SceneManager.LoadScene("MainMenu");
        }

        /// <summary>
        /// Update turn indicator text
        /// </summary>
        private void UpdateTurnIndicator(string text)
        {
            if (turnIndicatorText != null)
            {
                turnIndicatorText.text = text;
            }
        }

        /// <summary>
        /// Update action text
        /// </summary>
        private void UpdateActionText(string text)
        {
            if (actionText != null)
            {
                actionText.text = text;
            }
            Debug.Log($"[Battle] {text}");
        }

        /// <summary>
        /// Update available moves text
        /// </summary>
        private void UpdateAvailableMovesText()
        {
            if (availableMovesText == null) return;

            MoveData[] moves = MoveData.GetMovesForPlant(playerPlantType);
            string movesText = "Available Moves:\n";
            foreach (var move in moves)
            {
                movesText += $"- {move.moveName}\n";
            }

            availableMovesText.text = movesText;
        }

        /// <summary>
        /// Show/hide player turn UI
        /// </summary>
        private void ShowPlayerTurnUI(bool show)
        {
            if (playerTurnPanel != null)
                playerTurnPanel.SetActive(show);

            if (finishDrawingButton != null)
                finishDrawingButton.gameObject.SetActive(show);

            if (clearDrawingButton != null)
                clearDrawingButton.gameObject.SetActive(show);
        }

        /// <summary>
        /// Show/hide enemy turn UI
        /// </summary>
        private void ShowEnemyTurnUI(bool show)
        {
            if (enemyTurnPanel != null)
                enemyTurnPanel.SetActive(show);
        }

        /// <summary>
        /// Simple class to display battle units (placeholder for actual sprite rendering)
        /// </summary>
        [System.Serializable]
        public class BattleUnitDisplay
        {
            [SerializeField] private Image unitImage;
            [SerializeField] private TextMeshProUGUI unitNameText;
            [SerializeField] private Animator animator;

            public void Initialize(PlantRecognitionSystem.PlantType plantType, PlantRecognitionSystem.ElementType element, string displayName, Texture2D drawingTexture = null, bool isPlayerUnit = false)
            {
                Debug.Log($"BattleUnitDisplay.Initialize() called - IsPlayer:{isPlayerUnit}, HasTexture:{drawingTexture != null}");

                if (unitNameText != null)
                {
                    unitNameText.text = displayName;
                }

                if (unitImage == null)
                {
                    Debug.LogError("BattleUnitDisplay: unitImage is NULL! Cannot display sprite.");
                    return;
                }

                // If we have a drawing texture (player's drawn plant), use it as the sprite
                if (drawingTexture != null && isPlayerUnit)
                {
                    Debug.Log($"BattleUnitDisplay: Using player's drawing texture as sprite! Texture size: {drawingTexture.width}x{drawingTexture.height}");

                    // Convert Texture2D to Sprite
                    Sprite drawingSprite = Texture2DToSprite(drawingTexture);

                    if (drawingSprite != null)
                    {
                        unitImage.sprite = drawingSprite;
                        unitImage.color = Color.white; // Reset color to show texture properly
                        unitImage.preserveAspect = true; // Keep aspect ratio
                        Debug.Log($"✓ Drawing sprite applied to player unit! Sprite bounds: {drawingSprite.bounds}");
                    }
                    else
                    {
                        Debug.LogError("Failed to convert drawing texture to sprite!");
                        ApplyElementColor(element);
                    }
                }
                else
                {
                    // No drawing texture - use element color as fallback (for enemy or if texture missing)
                    if (drawingTexture == null && isPlayerUnit)
                    {
                        Debug.LogWarning("BattleUnitDisplay: Player unit has no drawing texture! Using fallback color.");
                    }
                    ApplyElementColor(element);
                }
            }

            /// <summary>
            /// Apply element-based color to the unit image (fallback when no sprite available)
            /// </summary>
            private void ApplyElementColor(PlantRecognitionSystem.ElementType element)
            {
                if (unitImage == null) return;

                Color elementColor = Color.white;
                switch (element)
                {
                    case PlantRecognitionSystem.ElementType.Fire:
                        elementColor = new Color(1f, 0.3f, 0.3f);
                        break;
                    case PlantRecognitionSystem.ElementType.Grass:
                        elementColor = new Color(0.3f, 1f, 0.3f);
                        break;
                    case PlantRecognitionSystem.ElementType.Water:
                        elementColor = new Color(0.3f, 0.3f, 1f);
                        break;
                }
                unitImage.color = elementColor;
            }

            /// <summary>
            /// Convert a Texture2D to a Sprite
            /// </summary>
            private Sprite Texture2DToSprite(Texture2D texture)
            {
                if (texture == null) return null;

                // Create sprite from texture
                Sprite sprite = Sprite.Create(
                    texture,
                    new Rect(0, 0, texture.width, texture.height),
                    new Vector2(0.5f, 0.5f), // Pivot at center
                    100f // Pixels per unit
                );

                return sprite;
            }

            public void PlayHitAnimation()
            {
                if (animator != null)
                {
                    animator.SetTrigger("Hit");
                }
                else if (unitImage != null)
                {
                    // Simple shake effect
                    StartShake();
                }
            }

            private void StartShake()
            {
                // Simple implementation - could be enhanced
                if (unitImage != null)
                {
                    // Would need MonoBehaviour context for coroutine
                }
            }
        }
    }
}
