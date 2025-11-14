using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;

/// <summary>
/// Manages turn-based combat where player draws attacks
/// </summary>
public class CombatManager : MonoBehaviour
{
    [Header("Units")]
    public BattleUnit playerUnit;
    public BattleUnit enemyUnit;

    [Header("Drawing System")]
    public GameObject drawingPanel;
    public DrawingCanvas drawingCanvas;
    public Button attackButton;

    [Header("Move Detection System")]
    public MovesetDetector movesetDetector;
    public MoveExecutor moveExecutor;

    [Header("UI")]
    public TextMeshProUGUI turnIndicatorText;
    public TextMeshProUGUI actionText;
    public TextMeshProUGUI availableMovesText;
    public Button nextEncounterButton;

    [Header("Combat Settings")]
    public float turnDelay = 1f;
    public float drawingTimeLimit = 5f;

    private enum BattleState
    {
        Start,
        PlayerTurn,
        EnemyTurn,
        Victory,
        Defeat
    }

    private BattleState currentState;
    private bool attackSubmitted = false;
    private PlantRecognitionSystem.PlantType playerPlantType;
    private PlantRecognitionSystem.PlantType enemyPlantType;

    private void Start()
    {
        // Hide drawing panel initially
        if (drawingPanel != null)
        {
            drawingPanel.SetActive(false);
        }

        // Hide next encounter button
        if (nextEncounterButton != null)
        {
            nextEncounterButton.gameObject.SetActive(false);
            nextEncounterButton.onClick.AddListener(OnNextEncounter);
        }

        // Setup attack button - Clear any existing listeners first!
        if (attackButton != null)
        {
            attackButton.onClick.RemoveAllListeners(); // Remove DrawingCanvas listener
            attackButton.onClick.AddListener(OnAttackButtonPressed);
            attackButton.interactable = false;
        }

        // Initialize player unit from DrawnUnitData
        if (playerUnit != null && DrawnUnitData.Instance != null)
        {
            playerUnit.InitializeFromDrawing(DrawnUnitData.Instance);
            playerPlantType = DrawnUnitData.Instance.plantType;
        }
        else
        {
            playerPlantType = PlantRecognitionSystem.PlantType.Sunflower; // Default
        }

        // Initialize enemy (you can randomize this later)
        if (enemyUnit != null)
        {
            // Randomize enemy plant type
            PlantRecognitionSystem.PlantType[] plantTypes = new PlantRecognitionSystem.PlantType[]
            {
                PlantRecognitionSystem.PlantType.Sunflower,
                PlantRecognitionSystem.PlantType.FireRose,
                PlantRecognitionSystem.PlantType.FlameTulip,
                PlantRecognitionSystem.PlantType.Cactus,
                PlantRecognitionSystem.PlantType.VineFlower,
                PlantRecognitionSystem.PlantType.GrassSprout,
                PlantRecognitionSystem.PlantType.WaterLily,
                PlantRecognitionSystem.PlantType.CoralBloom,
                PlantRecognitionSystem.PlantType.BubbleFlower
            };
            enemyPlantType = plantTypes[Random.Range(0, plantTypes.Length)];

            string enemyName = $"Wild {enemyPlantType}";
            enemyUnit.InitializeAsEnemy(enemyName, 40, 12, 3);
        }

        // Initialize move detection components
        if (movesetDetector == null)
        {
            movesetDetector = gameObject.AddComponent<MovesetDetector>();
        }
        if (moveExecutor == null)
        {
            moveExecutor = gameObject.AddComponent<MoveExecutor>();
        }

        StartCoroutine(BattleSequence());
    }

    private IEnumerator BattleSequence()
    {
        currentState = BattleState.Start;

        if (actionText != null)
        {
            actionText.text = "Battle Start!";
        }

        yield return new WaitForSeconds(turnDelay);

        // Main battle loop
        while (playerUnit.IsAlive() && enemyUnit.IsAlive())
        {
            // Player Turn
            yield return StartCoroutine(PlayerTurnSequence());

            if (!enemyUnit.IsAlive())
            {
                currentState = BattleState.Victory;
                break;
            }

            yield return new WaitForSeconds(turnDelay * 0.5f);

            // Enemy Turn
            yield return StartCoroutine(EnemyTurnSequence());

            if (!playerUnit.IsAlive())
            {
                currentState = BattleState.Defeat;
                break;
            }

            yield return new WaitForSeconds(turnDelay * 0.5f);
        }

        // Battle End
        yield return StartCoroutine(BattleEndSequence());
    }

    private IEnumerator PlayerTurnSequence()
    {
        currentState = BattleState.PlayerTurn;

        Debug.Log("=== PLAYER TURN START ===");

        if (turnIndicatorText != null)
        {
            turnIndicatorText.text = "YOUR TURN";
            turnIndicatorText.color = Color.green;
        }

        if (actionText != null)
        {
            actionText.text = "Draw your attack!";
        }

        // Show available moves
        ShowAvailableMoves();

        // Show drawing panel
        if (drawingPanel != null)
        {
            drawingPanel.SetActive(true);
        }

        // Clear previous drawing
        if (drawingCanvas != null)
        {
            drawingCanvas.ClearCanvas();
        }

        // Enable attack button after drawing starts
        attackSubmitted = false;
        if (attackButton != null)
        {
            attackButton.interactable = false;
        }

        // Monitor for drawing activity to enable button
        StartCoroutine(MonitorDrawingForButton());

        // Wait for player to submit attack
        float timeElapsed = 0f;
        while (!attackSubmitted && timeElapsed < drawingTimeLimit)
        {
            timeElapsed += Time.deltaTime;
            yield return null;
        }

        // Hide drawing panel
        if (drawingPanel != null)
        {
            drawingPanel.SetActive(false);
        }

        // Hide available moves text
        if (availableMovesText != null)
        {
            availableMovesText.gameObject.SetActive(false);
        }

        // Detect the move from the drawing
        if (movesetDetector != null && drawingCanvas != null)
        {
            var detectionResult = movesetDetector.DetectMove(
                drawingCanvas.GetAllStrokes(),
                playerPlantType
            );

            if (detectionResult.wasRecognized)
            {
                // Move recognized - execute it with quality multiplier!
                MoveData move = GetMoveData(detectionResult.detectedMove, playerPlantType);

                if (move != null)
                {
                    if (actionText != null)
                    {
                        actionText.text = $"You used {move.moveName}! ({detectionResult.qualityRating})";
                    }

                    yield return StartCoroutine(
                        moveExecutor.ExecuteMove(
                            move,
                            playerUnit,
                            enemyUnit,
                            playerPlantType,
                            enemyPlantType,
                            detectionResult.damageMultiplier)
                    );
                }
            }
            else
            {
                // Move not recognized - attack fails!
                if (actionText != null)
                {
                    actionText.text = "Your attack failed!";
                }

                yield return StartCoroutine(moveExecutor.ExecuteFailedAttack(playerUnit));
            }
        }
        else
        {
            // Fallback to basic attack if systems not initialized
            int damage = CalculateAttackDamage();
            if (actionText != null)
            {
                actionText.text = $"You attack for {damage} damage!";
            }
            yield return StartCoroutine(playerUnit.AttackAnimation(enemyUnit, damage));
        }

        // Wait to show the result
        yield return new WaitForSeconds(1.5f);
    }

    private IEnumerator MonitorDrawingForButton()
    {
        // Wait a moment for drawing to start
        yield return new WaitForSeconds(0.5f);

        while (!attackSubmitted && currentState == BattleState.PlayerTurn)
        {
            // Enable button if player has drawn at least one stroke
            if (drawingCanvas != null && drawingCanvas.currentStrokeCount > 0)
            {
                if (attackButton != null && !attackButton.interactable)
                {
                    attackButton.interactable = true;
                }
            }
            yield return null;
        }
    }

    private IEnumerator EnemyTurnSequence()
    {
        currentState = BattleState.EnemyTurn;

        Debug.Log("=== ENEMY TURN START ===");

        if (turnIndicatorText != null)
        {
            turnIndicatorText.text = "ENEMY TURN";
            turnIndicatorText.color = Color.red;
        }

        if (actionText != null)
        {
            actionText.text = "Enemy is preparing attack...";
        }

        yield return new WaitForSeconds(turnDelay);

        // Enemy attacks with base attack stat + random variance
        int damage = enemyUnit.attack + Random.Range(-2, 3);

        if (actionText != null)
        {
            actionText.text = $"Enemy attacks for {damage} damage!";
        }

        yield return StartCoroutine(enemyUnit.AttackAnimation(playerUnit, damage));

        // Wait 2 seconds to show the damage result
        yield return new WaitForSeconds(2f);
    }

    private IEnumerator BattleEndSequence()
    {
        yield return new WaitForSeconds(turnDelay);

        if (currentState == BattleState.Victory)
        {
            if (turnIndicatorText != null)
            {
                turnIndicatorText.text = "VICTORY!";
                turnIndicatorText.color = Color.yellow;
            }

            if (actionText != null)
            {
                actionText.text = "You defeated the enemy!";
            }

            // Show next encounter button
            if (nextEncounterButton != null)
            {
                nextEncounterButton.gameObject.SetActive(true);
            }
        }
        else if (currentState == BattleState.Defeat)
        {
            if (turnIndicatorText != null)
            {
                turnIndicatorText.text = "DEFEAT";
                turnIndicatorText.color = Color.red;
            }

            if (actionText != null)
            {
                actionText.text = "You were defeated...";
            }

            // Option to return to menu or retry
            yield return new WaitForSeconds(2f);
            SceneManager.LoadScene("MainMenu");
        }
    }

    private int CalculateAttackDamage()
    {
        if (drawingCanvas == null)
        {
            return playerUnit.attack;
        }

        int strokeCount = drawingCanvas.currentStrokeCount;

        // Base damage from unit stats
        int baseDamage = playerUnit.attack;

        // Bonus damage from drawing complexity
        // More strokes = more damage, but with diminishing returns
        float drawingMultiplier = 1f + (Mathf.Sqrt(strokeCount) * 0.3f);

        int totalDamage = Mathf.RoundToInt(baseDamage * drawingMultiplier);

        // Add small random variance
        totalDamage += Random.Range(-2, 3);

        Debug.Log($"Attack calculated: {strokeCount} strokes, {baseDamage} base → {totalDamage} total damage");

        return Mathf.Max(1, totalDamage);
    }

    private void OnAttackButtonPressed()
    {
        Debug.Log($"✅ Attack button pressed! Current state: {currentState}");

        if (currentState != BattleState.PlayerTurn)
        {
            Debug.LogWarning("Attack button pressed but not player turn!");
            return;
        }

        attackSubmitted = true;
        Debug.Log("Attack submitted - proceeding with attack!");

        if (attackButton != null)
        {
            attackButton.interactable = false;
        }
    }

    private void OnNextEncounter()
    {
        // For now, just reload the drawing scene
        // Later you can add: upgrade choice, new enemy, etc.
        SceneManager.LoadScene("DrawingScene");
    }

    /// <summary>
    /// Helper to clear drawing canvas (public for button binding if needed)
    /// </summary>
    public void ClearDrawing()
    {
        if (drawingCanvas != null)
        {
            drawingCanvas.ClearCanvas();
        }
    }

    /// <summary>
    /// Show available moves for the player's plant type
    /// </summary>
    private void ShowAvailableMoves()
    {
        if (availableMovesText == null) return;

        MoveData[] moves = MoveData.GetMovesForPlant(playerPlantType);
        if (moves.Length == 0)
        {
            availableMovesText.gameObject.SetActive(false);
            return;
        }

        string movesInfo = "Available Moves:\n";
        foreach (var move in moves)
        {
            movesInfo += $"• {move.moveName}\n";
        }

        availableMovesText.text = movesInfo;
        availableMovesText.gameObject.SetActive(true);
    }

    /// <summary>
    /// Get MoveData for a specific move type
    /// </summary>
    private MoveData GetMoveData(MoveData.MoveType moveType, PlantRecognitionSystem.PlantType plantType)
    {
        MoveData[] moves = MoveData.GetMovesForPlant(plantType);
        foreach (var move in moves)
        {
            if (move.moveType == moveType)
            {
                return move;
            }
        }
        return null;
    }
}