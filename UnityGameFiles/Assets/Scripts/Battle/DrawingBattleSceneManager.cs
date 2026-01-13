using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using SketchBlossom.Progression;

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

        [Header("Attack Animations")]
        [SerializeField] private AttackAnimationManager attackAnimationManager;
        [SerializeField] private MoveExecutor moveExecutor;
        [SerializeField] private DrawnMoveStorage drawnMoveStorage;
        [SerializeField] private DrawingCaptureHandler moveDrawingCapture;

        [Header("UI Elements")]
        [SerializeField] private TextMeshProUGUI turnIndicatorText;
        [SerializeField] private TextMeshProUGUI actionText;
        [SerializeField] private TextMeshProUGUI availableMovesText;
        [SerializeField] private GameObject playerTurnPanel;
        [SerializeField] private GameObject enemyTurnPanel;

        [Header("Battle Settings")]
        [SerializeField] private float turnDelay = 1.5f;
        [SerializeField] private float actionTextDelay = 2f;

        [Header("Turn Timer")]
        [SerializeField] private BattleTimer battleTimer;

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

        // Enemy difficulty (1-5 stars)
        private int enemyDifficultyStars = 1;

        // Flag: was auto-submit triggered by timer?
        private bool isAutoSubmittingMove = false;

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
            if (playerHPBar != null)
            {
                playerHPBar.Initialize(playerPlantName, playerMaxHP);
                Debug.Log($"✓ Player HP Bar initialized: {playerPlantName} with {playerMaxHP} HP");
            }
            else
            {
                Debug.LogError("❌ Player HP Bar is NULL! Cannot initialize.");
            }

            if (enemyHPBar != null)
            {
                enemyHPBar.Initialize(enemyPlantName, enemyMaxHP);
                Debug.Log($"✓ Enemy HP Bar initialized: {enemyPlantName} with {enemyMaxHP} HP");
            }
            else
            {
                Debug.LogError("❌ Enemy HP Bar is NULL! Cannot initialize.");
            }

            // Setup UI
            SetupUI();

            // Setup drawing canvas
            SetupDrawingCanvas();

            // Setup attack animation system
            SetupAttackAnimationSystem();

            // Start continuous visibility monitoring
            StartCoroutine(ContinuousVisibilityCheck());

            // Start battle
            StartCoroutine(BattleSequence());
        }

        /// <summary>
        /// Continuously check and ensure unit visibility throughout the battle
        /// This runs in the background to catch any edge cases where sprites might disappear
        /// </summary>
        private IEnumerator ContinuousVisibilityCheck()
        {
            Debug.Log("[VISIBILITY] Starting continuous visibility monitoring");

            while (currentState != BattleState.Victory && currentState != BattleState.Defeat)
            {
                // Check every 0.1 seconds to avoid excessive logging
                yield return new WaitForSeconds(0.1f);

                // Only enforce visibility for units that aren't dead
                if (playerUnit != null && !playerUnit.IsDead())
                {
                    playerUnit.EnsureVisible();
                }

                if (enemyUnit != null && !enemyUnit.IsDead())
                {
                    enemyUnit.EnsureVisible();
                }
            }

            Debug.Log("[VISIBILITY] Stopping continuous visibility monitoring - battle ended");
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
                playerUnit.SetCoroutineRunner(this);
                playerUnit.Initialize(playerPlantType, playerElement, playerPlantName, playerDrawingTexture, true);
            }
        }

        /// <summary>
        /// Load enemy unit (from world map encounter or random)
        /// </summary>
        private void LoadEnemyUnit()
        {
            // Check if this is a world map encounter
            if (EnemyEncounterData.Instance != null && EnemyEncounterData.Instance.isWorldMapEncounter)
            {
                // Use the enemy data from the world map encounter
                enemyPlantType = EnemyEncounterData.Instance.encounterPlantType;
                enemyElement = EnemyEncounterData.Instance.encounterElement;
                enemyPlantName = EnemyEncounterData.Instance.encounterDisplayName;

                // Get base stats and scale by difficulty
                var plantData = PlantRecognitionSystem.GetPlantData(enemyPlantType);

                // Stars 1-5
                enemyDifficultyStars = EnemyEncounterData.Instance.encounterDifficulty;

                // Scale enemy stats based on difficulty (1-5)
                float difficultyMultiplier = 1.0f + ((enemyDifficultyStars - 1) * 0.15f); // 1.0x to 1.6x
                enemyMaxHP = Mathf.RoundToInt(plantData.baseHP * difficultyMultiplier);
                enemyAttack = Mathf.RoundToInt(plantData.baseAttack * difficultyMultiplier);
                enemyDefense = Mathf.RoundToInt(plantData.baseDefense * difficultyMultiplier);

                Debug.Log($"World Map Encounter! Enemy: {enemyPlantName} (Difficulty: {enemyDifficultyStars} stars)");
                Debug.Log($"Enemy stats: HP:{enemyMaxHP}, ATK:{enemyAttack}, DEF:{enemyDefense}");
            }
            else
            {
                // Pick a random enemy (legacy behavior)
                PlantRecognitionSystem.PlantType[] allPlants = (PlantRecognitionSystem.PlantType[])System.Enum.GetValues(typeof(PlantRecognitionSystem.PlantType));
                enemyPlantType = allPlants[Random.Range(0, allPlants.Length)];

                var plantData = PlantRecognitionSystem.GetPlantData(enemyPlantType);
                enemyPlantName = plantData.displayName;
                enemyElement = plantData.element;
                enemyMaxHP = plantData.baseHP;
                enemyAttack = plantData.baseAttack;
                enemyDefense = plantData.baseDefense;

                enemyDifficultyStars = 1; // default difficulty

                Debug.Log($"Random enemy: {enemyPlantName} (HP:{enemyMaxHP}, ATK:{enemyAttack}, DEF:{enemyDefense})");
            }

            // Initialize enemy unit display (no drawing texture for enemy)
            if (enemyUnit != null)
            {
                enemyUnit.SetCoroutineRunner(this);
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

                // Force thick line width for battle moves
                drawingCanvas.SetLineWidth(20f);
                Debug.Log("DrawingBattleSceneManager: Set battle canvas line width to 80 pixels");
            }
        }

        /// <summary>
        /// Setup attack animation system components
        /// </summary>
        private void SetupAttackAnimationSystem()
        {
            // Auto-find or create DrawnMoveStorage
            if (drawnMoveStorage == null)
            {
                drawnMoveStorage = FindFirstObjectByType<DrawnMoveStorage>();
                if (drawnMoveStorage == null)
                {
                    GameObject storageObj = new GameObject("DrawnMoveStorage");
                    drawnMoveStorage = storageObj.AddComponent<DrawnMoveStorage>();
                    Debug.Log("DrawingBattleSceneManager: Created DrawnMoveStorage");
                }
            }

            // Auto-find or create AttackAnimationManager
            if (attackAnimationManager == null)
            {
                attackAnimationManager = FindFirstObjectByType<AttackAnimationManager>();
                if (attackAnimationManager == null)
                {
                    GameObject animManagerObj = new GameObject("AttackAnimationManager");
                    attackAnimationManager = animManagerObj.AddComponent<AttackAnimationManager>();
                    Debug.Log("DrawingBattleSceneManager: Created AttackAnimationManager");
                }
            }

            // Auto-find or create DrawingCaptureHandler for moves
            if (moveDrawingCapture == null)
            {
                moveDrawingCapture = FindFirstObjectByType<DrawingCaptureHandler>();
                if (moveDrawingCapture == null)
                {
                    GameObject captureObj = new GameObject("MoveDrawingCapture");
                    moveDrawingCapture = captureObj.AddComponent<DrawingCaptureHandler>();
                    Debug.Log("DrawingBattleSceneManager: Created DrawingCaptureHandler for moves");
                }
            }

            // Set attack points for animations (player unit to enemy unit)
            if (attackAnimationManager != null && playerUnit != null && enemyUnit != null)
            {
                // Get transforms from the unit displays' images
                Transform playerTransform = playerUnit.GetTransform();
                Transform enemyTransform = enemyUnit.GetTransform();

                if (playerTransform != null && enemyTransform != null)
                {
                    attackAnimationManager.SetAttackPoints(playerTransform, enemyTransform);
                }
                else
                {
                    Debug.LogWarning("Could not get unit transforms for attack animations");
                }
            }

            Debug.Log("✓ Attack animation system initialized");
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
            // Ensure both units are visible at the start of player's turn
            if (playerUnit != null) playerUnit.EnsureVisible();
            if (enemyUnit != null) enemyUnit.EnsureVisible();

            UpdateTurnIndicator("YOUR TURN");
            UpdateActionText("Draw your move!");
            ShowPlayerTurnUI(true);

            // Enable drawing
            currentState = BattleState.PlayerDrawing;
            drawingCanvas.EnableDrawing();
            drawingCanvas.ClearCanvas();

            // Start turn timer
            if (battleTimer != null)
            {
                bool isHardMode = enemyDifficultyStars >= 4;
                battleTimer.StartPlayerTurnTimer(isHardMode);
            }

            // Wait for player to finish drawing
            while (currentState == BattleState.PlayerDrawing)
            {
                yield return null;
            }

            // Stop timer when drawing phase is over (either finished or timed out)
            if (battleTimer != null)
            {
                battleTimer.Stop();
            }

            Debug.Log("[BATTLE] Player finished drawing, executing move");

            // Execute move was already called by OnDrawingCompleted / OnFinishDrawingClicked
            while (currentState == BattleState.PlayerExecuting)
            {
                yield return null;
            }

            Debug.Log("[BATTLE] Player move execution finished");

            // Ensure both units are visible before hiding UI
            if (playerUnit != null) playerUnit.EnsureVisible();
            if (enemyUnit != null) enemyUnit.EnsureVisible();

            ShowPlayerTurnUI(false);

            // Ensure both units are still visible after hiding UI
            if (playerUnit != null) playerUnit.EnsureVisible();
            if (enemyUnit != null) enemyUnit.EnsureVisible();

            yield return new WaitForSeconds(turnDelay);

            // Final visibility check before turn ends
            if (playerUnit != null) playerUnit.EnsureVisible();
            if (enemyUnit != null) enemyUnit.EnsureVisible();
        }

        /// <summary>
        /// Handle enemy's turn - AI selects and executes move
        /// </summary>
        private IEnumerator EnemyTurn()
        {
            // Make sure timer is not running during enemy turn
            if (battleTimer != null)
            {
                battleTimer.Stop();
            }

            // Ensure both units are visible at the start of enemy's turn
            if (playerUnit != null) playerUnit.EnsureVisible();
            if (enemyUnit != null) enemyUnit.EnsureVisible();

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

            // Show enemy move name with unique color
            string colorHex = ColorUtility.ToHtmlStringRGB(selectedMove.primaryColor);
            UpdateActionText($"{enemyPlantName} used <color=#{colorHex}>{selectedMove.moveName}</color>!");

            // Execute enemy move
            yield return StartCoroutine(ExecuteEnemyMove(selectedMove));

            yield return new WaitForSeconds(actionTextDelay);

            // Ensure both units are still visible after enemy attack
            if (playerUnit != null) playerUnit.EnsureVisible();
            if (enemyUnit != null) enemyUnit.EnsureVisible();

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
                if (isAutoSubmittingMove)
                {
                    UpdateActionText("Time's up! You hesitated and lose your turn.");
                    drawingCanvas.ClearCanvas();
                    currentState = BattleState.EnemyTurn;
                }
                else
                {
                    UpdateActionText("Draw something first!");
                    currentState = BattleState.PlayerDrawing;
                }
                return;
            }

            // Detect the move
            var result = movesetDetector.DetectMove(lineRenderers, playerPlantType);

            if (result.wasRecognized)
            {
                // CAPTURE THE MOVE DRAWING before clearing the canvas
                CaptureMoveDrawing(lineRenderers);

                // Execute the move
                StartCoroutine(ExecutePlayerMove(result));
            }
            else
            {
                if (isAutoSubmittingMove)
                {
                    UpdateActionText("Time's up! Your move failed and you lose your turn.");
                    drawingCanvas.ClearCanvas();
                    currentState = BattleState.EnemyTurn;
                }
                else
                {
                    UpdateActionText("Move not recognized! Try again.");
                    drawingCanvas.ClearCanvas();
                    currentState = BattleState.PlayerDrawing;
                }
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
        /// Capture the current move drawing and store it for use in attack animations
        /// </summary>
        private void CaptureMoveDrawing(List<LineRenderer> strokes)
        {
            if (moveDrawingCapture == null || drawnMoveStorage == null)
            {
                Debug.LogWarning("DrawingBattleSceneManager: Move drawing capture system not initialized!");
                return;
            }

            if (strokes == null || strokes.Count == 0)
            {
                Debug.LogWarning("DrawingBattleSceneManager: No strokes to capture!");
                return;
            }

            Debug.Log("========== CAPTURING MOVE DRAWING ==========");

            // Get the camera for rendering
            Camera mainCamera = Camera.main;
            if (mainCamera == null)
            {
                Debug.LogError("DrawingBattleSceneManager: No main camera found for move capture!");
                return;
            }

            // Capture the move drawing as a texture
            Texture2D moveTexture = moveDrawingCapture.CaptureDrawing(
                strokes,
                mainCamera,
                drawingCanvas.drawingArea
            );

            if (moveTexture != null)
            {
                // Store the captured move drawing
                drawnMoveStorage.AddMoveDrawing(moveTexture);
                Debug.Log($"✓ Move drawing captured! Texture size: {moveTexture.width}x{moveTexture.height}");
                Debug.Log($"   Total stored move drawings: {drawnMoveStorage.GetDrawingCount()}");
            }
            else
            {
                Debug.LogError("Failed to capture move drawing texture!");
            }

            Debug.Log("============================================");
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

            // Show move name with unique color using rich text
            string colorHex = ColorUtility.ToHtmlStringRGB(moveData.primaryColor);
            UpdateActionText($"You used <color=#{colorHex}>{moveData.moveName}</color>! (Quality: {result.qualityRating})");
            yield return new WaitForSeconds(0.5f);

            // Ensure both units are visible before attack animation
            if (playerUnit != null) playerUnit.EnsureVisible();
            if (enemyUnit != null) enemyUnit.EnsureVisible();

            // PLAY ATTACK ANIMATION WITH THE CAPTURED MOVE DRAWING
            if (attackAnimationManager != null && !moveData.isDefensiveMove && !moveData.isHealingMove)
            {
                Transform playerTransform = playerUnit.GetTransform();
                Transform enemyTransform = enemyUnit.GetTransform();

                if (playerTransform != null && enemyTransform != null)
                {
                    Debug.Log("[BATTLE] Starting attack animation");
                    // Play the attack animation using the drawn move
                    yield return StartCoroutine(attackAnimationManager.PlayAttackAnimation(
                        playerTransform,
                        enemyTransform,
                        moveData
                    ));
                    Debug.Log("[BATTLE] Attack animation completed");
                }
                else
                {
                    Debug.LogWarning("Could not get unit transforms for attack animation");
                }
            }

            // ENHANCED VISUAL EFFECTS (Screen shake, unique colors, gradient flash)
            if (!moveData.isDefensiveMove && !moveData.isHealingMove)
            {
                Debug.Log($"[BATTLE] Playing enhanced move visual effects - Colors: {moveData.primaryColor} -> {moveData.secondaryColor}");

                // Play the gradient color flash on the enemy
                if (enemyUnit != null)
                {
                    yield return StartCoroutine(enemyUnit.FlashWithGradient(moveData.primaryColor, moveData.secondaryColor));
                }

                // Play the screen shake based on move power
                if (moveExecutor != null && moveExecutor.mainCamera != null && moveData.screenShakeAmount > 0)
                {
                    StartCoroutine(PlayScreenShake(moveData.screenShakeAmount * moveExecutor.screenShakeMultiplier, 0.2f));
                }
            }

            // Ensure both units are still visible after attack animation
            if (playerUnit != null) playerUnit.EnsureVisible();
            if (enemyUnit != null) enemyUnit.EnsureVisible();

            // Calculate and apply move effects
            if (moveData.isDefensiveMove)
            {
                playerIsBlocking = true;
                UpdateActionText("You're defending!");

                // Show defensive color flash on player
                if (playerUnit != null)
                {
                    yield return StartCoroutine(playerUnit.FlashWithGradient(moveData.primaryColor, moveData.secondaryColor));
                }
            }
            else if (moveData.isHealingMove)
            {
                int healAmount = Mathf.RoundToInt(moveData.basePower * result.damageMultiplier);
                playerHPBar.ModifyHP(healAmount);
                UpdateActionText($"Restored {healAmount} HP!");

                // Show healing color flash on player
                if (playerUnit != null)
                {
                    yield return StartCoroutine(playerUnit.FlashWithGradient(moveData.primaryColor, moveData.secondaryColor));
                }
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

                Debug.Log($"[DAMAGE] Player attacks enemy for {damage} damage");
                Debug.Log($"[HP BEFORE] Enemy HP: {enemyHPBar.GetCurrentHP()}/{enemyHPBar.GetMaxHP()}");

                if (enemyHPBar != null)
                {
                    enemyHPBar.ModifyHP(-damage);
                    enemyHPBar.PlayDamageAnimation();
                    Debug.Log($"[HP AFTER] Enemy HP: {enemyHPBar.GetCurrentHP()}/{enemyHPBar.GetMaxHP()}");
                }
                else
                {
                    Debug.LogError("❌ Enemy HP Bar is NULL! Cannot apply damage.");
                }

                UpdateActionText($"Dealt {damage} damage!");

                if (enemyUnit != null)
                {
                    enemyUnit.PlayHitAnimation();
                }
            }

            yield return new WaitForSeconds(actionTextDelay);

            // Ensure both units are visible before clearing canvas
            if (playerUnit != null) playerUnit.EnsureVisible();
            if (enemyUnit != null) enemyUnit.EnsureVisible();

            // Clear the canvas for the next turn
            drawingCanvas.ClearCanvas();

            // Ensure both units are still visible after clearing canvas
            if (playerUnit != null) playerUnit.EnsureVisible();
            if (enemyUnit != null) enemyUnit.EnsureVisible();

            // Reset blocking state
            enemyIsBlocking = false;

            Debug.Log("[BATTLE] Player move execution complete, transitioning to enemy turn");

            // Ensure both units are visible before state change
            if (playerUnit != null) playerUnit.EnsureVisible();
            if (enemyUnit != null) enemyUnit.EnsureVisible();

            currentState = BattleState.EnemyTurn;
        }

        /// <summary>
        /// Execute enemy's move with visual effects
        /// </summary>
        private IEnumerator ExecuteEnemyMove(MoveData moveData)
        {
            if (moveData.isDefensiveMove)
            {
                enemyIsBlocking = true;
                UpdateActionText($"{enemyPlantName} is defending!");

                // Show defensive color flash on enemy
                if (enemyUnit != null)
                {
                    yield return StartCoroutine(enemyUnit.FlashWithGradient(moveData.primaryColor, moveData.secondaryColor));
                }
            }
            else if (moveData.isHealingMove)
            {
                int healAmount = moveData.basePower;
                enemyHPBar.ModifyHP(healAmount);
                UpdateActionText($"{enemyPlantName} restored {healAmount} HP!");

                // Show healing color flash on enemy
                if (enemyUnit != null)
                {
                    yield return StartCoroutine(enemyUnit.FlashWithGradient(moveData.primaryColor, moveData.secondaryColor));
                }
            }
            else
            {
                // ENHANCED VISUAL EFFECTS for enemy attack
                Debug.Log($"[BATTLE] Enemy playing enhanced visual effects - Colors: {moveData.primaryColor} -> {moveData.secondaryColor}");

                // Play the gradient color flash on the player
                if (playerUnit != null)
                {
                    yield return StartCoroutine(playerUnit.FlashWithGradient(moveData.primaryColor, moveData.secondaryColor));
                }

                // Screen shake for enemy attacks
                if (moveExecutor != null && moveExecutor.mainCamera != null && moveData.screenShakeAmount > 0)
                {
                    StartCoroutine(PlayScreenShake(moveData.screenShakeAmount * moveExecutor.screenShakeMultiplier * 0.7f, 0.2f)); // Slightly less shake for enemy
                }

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

                Debug.Log($"[DAMAGE] Enemy attacks player for {damage} damage");
                Debug.Log($"[HP BEFORE] Player HP: {playerHPBar.GetCurrentHP()}/{playerHPBar.GetMaxHP()}");

                if (playerHPBar != null)
                {
                    playerHPBar.ModifyHP(-damage);
                    playerHPBar.PlayDamageAnimation();
                    Debug.Log($"[HP AFTER] Player HP: {playerHPBar.GetCurrentHP()}/{playerHPBar.GetMaxHP()}");
                }
                else
                {
                    Debug.LogError("❌ Player HP Bar is NULL! Cannot apply damage.");
                }

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
        /// Called by BattleTimer: standard battle, auto-submit current drawing.
        /// </summary>
        public void OnPlayerTurnTimedOut_AutoSubmit()
        {
            if (currentState != BattleState.PlayerDrawing)
                return;

            Debug.Log("[TIMER] Player timed out (auto submit).");

            isAutoSubmittingMove = true;
            OnFinishDrawingClicked(); // reuse existing logic
            isAutoSubmittingMove = false;
        }

        /// <summary>
        /// Called by BattleTimer: hard battles (>=4 stars) - entire turn is lost.
        /// </summary>
        public void OnPlayerTurnTimedOut_Hard()
        {
            if (currentState != BattleState.PlayerDrawing)
                return;

            Debug.Log("[TIMER] Player timed out (hard) - turn forfeited.");

            if (battleTimer != null)
            {
                battleTimer.Stop();
            }

            drawingCanvas.ClearCanvas();
            UpdateActionText("Time's up! You lose your turn.");
            ShowPlayerTurnUI(false);

            currentState = BattleState.EnemyTurn;
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

            Debug.Log("=== VICTORY! Player wins the battle ===");

            // Ensure player unit stays visible during victory
            if (playerUnit != null)
            {
                playerUnit.EnsureVisible();
                Debug.Log("Player unit visibility ensured during victory");
            }

            // Trigger enemy death animation
            if (enemyUnit != null)
            {
                Debug.Log("Triggering enemy death animation");
                enemyUnit.Die(this);
            }

            // Store enemy plant data for potential taming
            StoreEnemyPlantData();

            // Record victory for player's plant in inventory
            RecordPlayerVictory();

            // Keep ensuring player visibility during the victory wait period
            yield return new WaitForSeconds(1.5f);
            if (playerUnit != null)
            {
                playerUnit.EnsureVisible();
            }

            yield return new WaitForSeconds(1.5f);

            // Load PostBattleScene to choose Wild Growth or Tame
            SceneManager.LoadScene("PostBattleScene");
        }

        /// <summary>
        /// Handle defeat - implements rogue-like permanent death
        /// </summary>
        private IEnumerator HandleDefeat()
        {
            UpdateTurnIndicator("DEFEAT");
            UpdateActionText("You lost...");

            Debug.Log("=== DEFEAT! Player loses the battle ===");

            // Ensure enemy unit stays visible during defeat
            if (enemyUnit != null)
            {
                enemyUnit.EnsureVisible();
                Debug.Log("Enemy unit visibility ensured during defeat");
            }

            // Trigger player death animation
            if (playerUnit != null)
            {
                Debug.Log("Triggering player death animation");
                playerUnit.Die(this);
            }

            yield return new WaitForSeconds(3f);

            // ROGUE-LIKE MECHANIC: Remove dead plant from inventory (permanent death)
            PlayerInventory inventory = PlayerInventory.Instance;
            if (inventory != null)
            {
                PlantInventoryEntry deadPlant = inventory.GetSelectedPlant();
                if (deadPlant != null)
                {
                    Debug.Log($"[Rogue-like] Plant {deadPlant.plantName} has died permanently!");
                    inventory.RemovePlant(deadPlant.plantId);

                    // Check if player has any plants left
                    if (inventory.GetPlantCount() == 0)
                    {
                        // GAME OVER - No plants remaining
                        Debug.Log("[Rogue-like] GAME OVER - All plants dead! Returning to main menu.");
                        UpdateTurnIndicator("GAME OVER");
                        UpdateActionText("All your plants have perished...");
                        yield return new WaitForSeconds(2f);
                        SceneManager.LoadScene("MainMenuScene");
                        yield break;
                    }
                    else
                    {
                        // Player has plants remaining - let them choose another
                        Debug.Log($"[Rogue-like] Player has {inventory.GetPlantCount()} plants remaining. Going to selection screen.");
                        UpdateActionText("Choose your next plant...");
                        yield return new WaitForSeconds(1.5f);
                        SceneManager.LoadScene("PlantSelectionScene");
                        yield break;
                    }
                }
            }

            // Fallback if inventory not found
            SceneManager.LoadScene("WorldMapScene");
        }

        /// <summary>
        /// Stores enemy plant data in EnemyUnitData singleton for potential taming
        /// </summary>
        private void StoreEnemyPlantData()
        {
            // Create or find EnemyUnitData singleton
            if (EnemyUnitData.Instance == null)
            {
                GameObject enemyDataObj = new GameObject("EnemyUnitData");
                enemyDataObj.AddComponent<EnemyUnitData>();
            }

            if (EnemyUnitData.Instance != null)
            {
                // Get color based on element
                Color enemyColor = GetElementColor(enemyElement);

                // Store enemy data
                EnemyUnitData.Instance.SetPlantData(
                    enemyPlantType,
                    enemyElement,
                    enemyPlantName,
                    enemyMaxHP,
                    enemyAttack,
                    enemyDefense,
                    enemyColor
                );

                Debug.Log($"Stored enemy plant data: {enemyPlantName} for potential taming");
            }
        }

        /// <summary>
        /// Records the victory for the player's plant in the inventory
        /// </summary>
        private void RecordPlayerVictory()
        {
            PlayerInventory inventory = PlayerInventory.Instance;
            if (inventory != null)
            {
                PlantInventoryEntry selectedPlant = inventory.GetSelectedPlant();
                if (selectedPlant != null)
                {
                    inventory.RecordVictory(selectedPlant.plantId);
                    Debug.Log($"Recorded victory for {selectedPlant.plantName}");
                }
            }
        }

        /// <summary>
        /// Gets a color based on element type
        /// </summary>
        private Color GetElementColor(PlantRecognitionSystem.ElementType element)
        {
            switch (element)
            {
                case PlantRecognitionSystem.ElementType.Fire:
                    return new Color(1f, 0.3f, 0.2f); // Red
                case PlantRecognitionSystem.ElementType.Water:
                    return new Color(0.2f, 0.5f, 1f); // Blue
                case PlantRecognitionSystem.ElementType.Grass:
                    return new Color(0.3f, 0.9f, 0.3f); // Green
                default:
                    return Color.gray;
            }
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
        /// Screen shake effect for powerful attacks
        /// Called when moveExecutor is available and move has screen shake
        /// </summary>
        private IEnumerator PlayScreenShake(float intensity, float duration)
        {
            if (moveExecutor == null || moveExecutor.mainCamera == null)
            {
                yield break;
            }

            Camera cam = moveExecutor.mainCamera;
            Vector3 originalPosition = cam.transform.position;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                float x = Random.Range(-1f, 1f) * intensity;
                float y = Random.Range(-1f, 1f) * intensity;

                cam.transform.position = originalPosition + new Vector3(x, y, 0);

                elapsed += Time.deltaTime;
                yield return null;
            }

            // Return to original position
            cam.transform.position = originalPosition;
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

            private bool isDead = false;
            private MonoBehaviour coroutineRunner;
            private Sprite assignedSprite; // Keep reference to prevent garbage collection
            private string unitIdentifier = "Unknown"; // For debugging which unit this is
            private bool hasLoggedDeadSkip = false; // Track if we've already logged the dead skip message

            public void Initialize(PlantRecognitionSystem.PlantType plantType, PlantRecognitionSystem.ElementType element, string displayName, Texture2D drawingTexture = null, bool isPlayerUnit = false)
            {
                unitIdentifier = isPlayerUnit ? "PLAYER" : "ENEMY";
                Debug.Log($"BattleUnitDisplay.Initialize() called - Unit: {unitIdentifier}, HasTexture:{drawingTexture != null}");

                // Reset death state and logging flags
                isDead = false;
                hasLoggedDeadSkip = false;

                if (unitNameText != null)
                {
                    unitNameText.text = displayName;
                }

                if (unitImage == null)
                {
                    Debug.LogError("BattleUnitDisplay: unitImage is NULL! Cannot display sprite.");
                    return;
                }

                // Ensure the unit image GameObject is active and visible
                if (unitImage.gameObject != null)
                {
                    unitImage.gameObject.SetActive(true);
                    Debug.Log($"BattleUnitDisplay: Ensured unitImage GameObject is active");
                }

                // If we have a drawing texture (player's drawn plant), use it as the sprite
                if (drawingTexture != null && isPlayerUnit)
                {
                    Debug.Log($"BattleUnitDisplay: Using player's drawing texture as sprite! Texture size: {drawingTexture.width}x{drawingTexture.height}");

                    // Convert Texture2D to Sprite and keep reference
                    assignedSprite = Texture2DToSprite(drawingTexture);

                    if (assignedSprite != null)
                    {
                        unitImage.sprite = assignedSprite;
                        unitImage.color = Color.white; // Reset color to show texture properly
                        unitImage.preserveAspect = true; // Keep aspect ratio
                        unitImage.enabled = true; // Ensure Image component is enabled
                        Debug.Log($"✓ Drawing sprite applied to player unit! Sprite bounds: {assignedSprite.bounds}");
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
                    unitImage.enabled = true; // Ensure Image component is enabled
                }

                // Final check to ensure everything is visible
                EnsureVisible();
            }

            /// <summary>
            /// Ensure the sprite remains visible (call this to prevent disappearing sprites)
            /// </summary>
            public void EnsureVisible()
            {
                if (isDead)
                {
                    // Only log this once to avoid spam
                    if (!hasLoggedDeadSkip)
                    {
                        Debug.Log($"BattleUnitDisplay ({unitIdentifier}): EnsureVisible skipped - unit is dead");
                        hasLoggedDeadSkip = true;
                    }
                    return; // Don't make dead units visible
                }

                if (unitImage != null)
                {
                    // Ensure the Image component is enabled
                    unitImage.enabled = true;

                    // Ensure the GameObject is active
                    if (unitImage.gameObject != null && !unitImage.gameObject.activeSelf)
                    {
                        unitImage.gameObject.SetActive(true);
                        Debug.LogWarning($"BattleUnitDisplay ({unitIdentifier}): Had to reactivate unitImage GameObject!");
                    }

                    // Ensure we still have a sprite assigned
                    if (unitImage.sprite == null && assignedSprite != null)
                    {
                        unitImage.sprite = assignedSprite;
                        Debug.LogWarning($"BattleUnitDisplay ({unitIdentifier}): Had to reassign sprite!");
                    }

                    // Ensure alpha is not zero (unless fading out during death)
                    Color currentColor = unitImage.color;
                    if (currentColor.a < 0.1f && !isDead)
                    {
                        currentColor.a = 1f;
                        unitImage.color = currentColor;
                        Debug.LogWarning($"BattleUnitDisplay ({unitIdentifier}): Had to restore alpha from {currentColor.a:F2} to 1.0!");
                    }
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
                // Don't play hit animation if already dead
                if (isDead) return;

                if (animator != null)
                {
                    animator.SetTrigger("Hit");
                }
                else if (unitImage != null && coroutineRunner != null)
                {
                    // Simple shake effect
                    coroutineRunner.StartCoroutine(ShakeEffect());
                }
            }

            /// <summary>
            /// Mark this unit as dead and play death animation
            /// </summary>
            public void Die(MonoBehaviour runner)
            {
                if (isDead)
                {
                    Debug.LogWarning($"BattleUnitDisplay ({unitIdentifier}): Die() called but unit is already dead!");
                    return; // Already dead, don't fade again
                }

                isDead = true;
                Debug.Log($"BattleUnitDisplay ({unitIdentifier}): Unit is dying, starting fade out animation");

                if (runner != null && unitImage != null)
                {
                    runner.StartCoroutine(FadeOutEffect());
                }
                else
                {
                    Debug.LogError($"BattleUnitDisplay ({unitIdentifier}): Cannot start fade - runner or unitImage is null!");
                }
            }

            /// <summary>
            /// Shake effect for taking damage
            /// </summary>
            private IEnumerator ShakeEffect()
            {
                if (unitImage == null) yield break;

                Vector3 originalPosition = unitImage.transform.localPosition;
                float shakeDuration = 0.3f;
                float shakeAmount = 5f;
                float elapsed = 0f;

                while (elapsed < shakeDuration)
                {
                    float x = originalPosition.x + Random.Range(-shakeAmount, shakeAmount);
                    float y = originalPosition.y + Random.Range(-shakeAmount, shakeAmount);
                    unitImage.transform.localPosition = new Vector3(x, y, originalPosition.z);

                    elapsed += Time.deltaTime;
                    yield return null;
                }

                // Reset to original position
                unitImage.transform.localPosition = originalPosition;
            }

            /// <summary>
            /// Fade out effect when unit dies
            /// </summary>
            private IEnumerator FadeOutEffect()
            {
                if (unitImage == null)
                {
                    Debug.LogError($"BattleUnitDisplay ({unitIdentifier}): FadeOutEffect - unitImage is null!");
                    yield break;
                }

                float fadeDuration = 1.0f;
                float elapsed = 0f;
                Color originalColor = unitImage.color;

                Debug.Log($"BattleUnitDisplay ({unitIdentifier}): Starting fade from alpha {originalColor.a}");

                while (elapsed < fadeDuration)
                {
                    elapsed += Time.deltaTime;
                    float alpha = Mathf.Lerp(1f, 0.2f, elapsed / fadeDuration);
                    unitImage.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
                    yield return null;
                }

                // Set final alpha
                unitImage.color = new Color(originalColor.r, originalColor.g, originalColor.b, 0.2f);
                Debug.Log($"BattleUnitDisplay ({unitIdentifier}): Fade out complete - final alpha: 0.2");
            }

            /// <summary>
            /// Set the MonoBehaviour that will run coroutines for this display
            /// </summary>
            public void SetCoroutineRunner(MonoBehaviour runner)
            {
                coroutineRunner = runner;
            }

            /// <summary>
            /// Get the transform of this unit display (from the unit image)
            /// </summary>
            public Transform GetTransform()
            {
                if (unitImage != null)
                {
                    return unitImage.transform;
                }
                return null;
            }

            /// <summary>
            /// Check if this unit is dead
            /// </summary>
            public bool IsDead()
            {
                return isDead;
            }

            /// <summary>
            /// Flash the unit with gradient colors (primary to secondary)
            /// Used for enhanced move visual effects
            /// </summary>
            public IEnumerator FlashWithGradient(Color primaryColor, Color secondaryColor)
            {
                if (unitImage == null || isDead)
                {
                    yield break;
                }

                Color originalColor = unitImage.color;

                // Flash primary color
                unitImage.color = primaryColor;
                yield return new WaitForSeconds(0.08f);

                // Transition to secondary color
                unitImage.color = secondaryColor;
                yield return new WaitForSeconds(0.08f);

                // Return to original color
                unitImage.color = originalColor;
            }

            /// <summary>
            /// Get the UI Image component for direct access if needed
            /// </summary>
            public Image GetImage()
            {
                return unitImage;
            }
        }
    }
}
