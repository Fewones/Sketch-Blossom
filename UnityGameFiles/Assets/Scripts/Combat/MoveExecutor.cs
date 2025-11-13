using UnityEngine;
using System.Collections;
using TMPro;

/// <summary>
/// Executes detected battle moves with animations and effects
/// </summary>
public class MoveExecutor : MonoBehaviour
{
    [Header("Visual Feedback")]
    public TextMeshProUGUI moveNameText;
    public TextMeshProUGUI effectivenessText;

    [Header("Attack Colors")]
    public Color fireColor = new Color(1f, 0.3f, 0f);
    public Color grassColor = new Color(0.2f, 0.8f, 0.2f);
    public Color waterColor = new Color(0.2f, 0.5f, 1f);

    /// <summary>
    /// Execute a detected move from attacker to target
    /// </summary>
    public IEnumerator ExecuteMove(
        MoveData move,
        BattleUnit attacker,
        BattleUnit target,
        PlantRecognitionSystem.PlantType attackerPlantType,
        PlantRecognitionSystem.PlantType targetPlantType)
    {
        Debug.Log($"=== EXECUTING MOVE: {move.moveName} ===");

        // Show move name
        if (moveNameText != null)
        {
            moveNameText.gameObject.SetActive(true);
            moveNameText.text = move.moveName.ToUpper();
            moveNameText.color = GetElementColor(move.element);
        }

        yield return new WaitForSeconds(0.5f);

        // Execute move based on type
        if (move.isHealingMove)
        {
            yield return ExecuteHealingMove(move, attacker);
        }
        else
        {
            yield return ExecuteAttackMove(move, attacker, target, attackerPlantType, targetPlantType);
        }

        // Hide move name
        if (moveNameText != null)
        {
            moveNameText.gameObject.SetActive(false);
        }

        yield return new WaitForSeconds(0.5f);
    }

    /// <summary>
    /// Execute an attack move with type advantage calculations
    /// </summary>
    private IEnumerator ExecuteAttackMove(
        MoveData move,
        BattleUnit attacker,
        BattleUnit target,
        PlantRecognitionSystem.PlantType attackerPlantType,
        PlantRecognitionSystem.PlantType targetPlantType)
    {
        // Calculate type advantage
        MoveData.ElementType attackElement = move.element;
        MoveData.ElementType defenseElement = GetElementTypeForPlant(targetPlantType);

        float typeMultiplier = MoveData.GetTypeAdvantage(attackElement, defenseElement);

        // Calculate damage
        int baseDamage = move.basePower + attacker.attack;
        int finalDamage = Mathf.RoundToInt(baseDamage * typeMultiplier);

        Debug.Log($"Damage calculation: {move.basePower} (move) + {attacker.attack} (ATK) × {typeMultiplier} (type) = {finalDamage}");

        // Show effectiveness feedback
        if (effectivenessText != null)
        {
            if (typeMultiplier > 1.0f)
            {
                effectivenessText.text = "It's super effective!";
                effectivenessText.color = Color.green;
            }
            else if (typeMultiplier < 1.0f)
            {
                effectivenessText.text = "It's not very effective...";
                effectivenessText.color = Color.red;
            }
            else
            {
                effectivenessText.text = "";
            }
        }

        // Play move animation
        yield return PlayMoveAnimation(move, attacker, target);

        // Deal damage
        target.TakeDamage(finalDamage);

        yield return new WaitForSeconds(0.8f);

        // Clear effectiveness text
        if (effectivenessText != null)
        {
            effectivenessText.text = "";
        }
    }

    /// <summary>
    /// Execute a healing move
    /// </summary>
    private IEnumerator ExecuteHealingMove(MoveData move, BattleUnit healer)
    {
        int healAmount = move.basePower;
        int oldHealth = healer.currentHealth;

        healer.currentHealth = Mathf.Min(healer.currentHealth + healAmount, healer.maxHealth);
        int actualHeal = healer.currentHealth - oldHealth;

        Debug.Log($"{healer.unitName} heals for {actualHeal} HP!");

        // Visual feedback
        if (effectivenessText != null)
        {
            effectivenessText.text = $"+{actualHeal} HP";
            effectivenessText.color = Color.green;
        }

        // Healing animation (could add particle effects here)
        yield return PlayHealingAnimation(healer);

        yield return new WaitForSeconds(1f);

        if (effectivenessText != null)
        {
            effectivenessText.text = "";
        }
    }

    /// <summary>
    /// Play animation for a specific move type
    /// </summary>
    private IEnumerator PlayMoveAnimation(MoveData move, BattleUnit attacker, BattleUnit target)
    {
        // Basic attack animation (move toward target)
        Vector3 originalPos = attacker.transform.position;
        Vector3 targetPos = originalPos + (attacker.isPlayerUnit ? Vector3.right : Vector3.left) * 1.5f;

        // Move forward
        float elapsed = 0f;
        float moveDuration = 0.25f;
        while (elapsed < moveDuration)
        {
            attacker.transform.position = Vector3.Lerp(originalPos, targetPos, elapsed / moveDuration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        // Visual effect based on move type (simplified - could add particle systems)
        yield return FlashEffect(target, GetElementColor(move.element));

        // Move back
        elapsed = 0f;
        while (elapsed < moveDuration)
        {
            attacker.transform.position = Vector3.Lerp(targetPos, originalPos, elapsed / moveDuration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        attacker.transform.position = originalPos;
    }

    /// <summary>
    /// Play healing animation
    /// </summary>
    private IEnumerator PlayHealingAnimation(BattleUnit healer)
    {
        SpriteRenderer sprite = healer.GetComponent<SpriteRenderer>();
        if (sprite != null)
        {
            Color originalColor = sprite.color;
            Color healColor = new Color(0.5f, 1f, 0.5f, 1f);

            // Pulse green
            for (int i = 0; i < 2; i++)
            {
                sprite.color = healColor;
                yield return new WaitForSeconds(0.15f);
                sprite.color = originalColor;
                yield return new WaitForSeconds(0.15f);
            }
        }
    }

    /// <summary>
    /// Flash effect on target
    /// </summary>
    private IEnumerator FlashEffect(BattleUnit target, Color effectColor)
    {
        SpriteRenderer sprite = target.GetComponent<SpriteRenderer>();
        if (sprite != null)
        {
            Color originalColor = sprite.color;
            sprite.color = effectColor;
            yield return new WaitForSeconds(0.15f);
            sprite.color = originalColor;
        }
    }

    /// <summary>
    /// Get color for element type
    /// </summary>
    private Color GetElementColor(MoveData.ElementType element)
    {
        switch (element)
        {
            case MoveData.ElementType.Fire: return fireColor;
            case MoveData.ElementType.Grass: return grassColor;
            case MoveData.ElementType.Water: return waterColor;
            default: return Color.white;
        }
    }

    /// <summary>
    /// Convert plant type to element type
    /// </summary>
    private MoveData.ElementType GetElementTypeForPlant(PlantRecognitionSystem.PlantType plantType)
    {
        switch (plantType)
        {
            // Fire plants
            case PlantRecognitionSystem.PlantType.Sunflower:
            case PlantRecognitionSystem.PlantType.FireRose:
            case PlantRecognitionSystem.PlantType.FlameTulip:
                return MoveData.ElementType.Fire;

            // Grass plants
            case PlantRecognitionSystem.PlantType.Cactus:
            case PlantRecognitionSystem.PlantType.VineFlower:
            case PlantRecognitionSystem.PlantType.GrassSprout:
                return MoveData.ElementType.Grass;

            // Water plants
            case PlantRecognitionSystem.PlantType.WaterLily:
            case PlantRecognitionSystem.PlantType.CoralBloom:
            case PlantRecognitionSystem.PlantType.BubbleFlower:
                return MoveData.ElementType.Water;

            default: return MoveData.ElementType.Fire; // Default
        }
    }

    /// <summary>
    /// Public method to execute failed attack (when move not recognized)
    /// </summary>
    public IEnumerator ExecuteFailedAttack(BattleUnit attacker)
    {
        Debug.Log($"{attacker.unitName} failed to execute a move!");

        if (moveNameText != null)
        {
            moveNameText.gameObject.SetActive(true);
            moveNameText.text = "MOVE FAILED!";
            moveNameText.color = Color.red;
        }

        if (effectivenessText != null)
        {
            effectivenessText.text = "Drawing not recognized!";
            effectivenessText.color = Color.red;
        }

        // Attacker looks confused (no movement)
        yield return new WaitForSeconds(1.5f);

        if (moveNameText != null)
        {
            moveNameText.gameObject.SetActive(false);
        }

        if (effectivenessText != null)
        {
            effectivenessText.text = "";
        }
    }
}
