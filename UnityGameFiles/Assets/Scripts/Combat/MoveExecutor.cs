using UnityEngine;
using System.Collections;
using TMPro;

/// <summary>
/// Executes detected battle moves with animations and effects
/// Enhanced with unique move colors, screen shake, and visual effects
/// </summary>
public class MoveExecutor : MonoBehaviour
{
    [Header("Visual Feedback")]
    public TextMeshProUGUI moveNameText;
    public TextMeshProUGUI effectivenessText;

    [Header("Attack Colors - Fallback (used if move colors not available)")]
    public Color fireColor = new Color(1f, 0.3f, 0f);
    public Color grassColor = new Color(0.2f, 0.8f, 0.2f);
    public Color waterColor = new Color(0.2f, 0.5f, 1f);

    [Header("Screen Shake Settings")]
    public Camera mainCamera;
    public float screenShakeMultiplier = 1.0f;  // Global screen shake intensity

    /// <summary>
    /// Execute a detected move from attacker to target
    /// </summary>
    public IEnumerator ExecuteMove(
        MoveData move,
        BattleUnit attacker,
        BattleUnit target,
        PlantRecognitionSystem.PlantType attackerPlantType,
        PlantRecognitionSystem.PlantType targetPlantType,
        float qualityMultiplier = 1f)
    {
        Debug.Log($"=== EXECUTING MOVE: {move.moveName} (Quality: {qualityMultiplier:F2}x) ===");

        // Show move name with unique move color
        if (moveNameText != null)
        {
            moveNameText.gameObject.SetActive(true);
            moveNameText.text = move.moveName.ToUpper();
            // Use move's unique primary color
            moveNameText.color = move.primaryColor;

            Debug.Log($"Move color: {move.primaryColor}, Effect: {move.visualEffect}");
        }

        yield return new WaitForSeconds(0.5f * move.animationIntensity);

        // Execute move based on type
        if (move.isDefensiveMove)
        {
            yield return ExecuteDefensiveMove(move, attacker, qualityMultiplier);
        }
        else if (move.isHealingMove)
        {
            yield return ExecuteHealingMove(move, attacker, qualityMultiplier);
        }
        else
        {
            yield return ExecuteAttackMove(move, attacker, target, attackerPlantType, targetPlantType, qualityMultiplier);
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
        PlantRecognitionSystem.PlantType targetPlantType,
        float qualityMultiplier = 1f)
    {
        // Calculate type advantage
        MoveData.ElementType attackElement = move.element;
        MoveData.ElementType defenseElement = GetElementTypeForPlant(targetPlantType);

        float typeMultiplier = MoveData.GetTypeAdvantage(attackElement, defenseElement);

        // Calculate damage with quality multiplier
        int baseDamage = move.basePower + attacker.attack;
        int finalDamage = Mathf.RoundToInt(baseDamage * typeMultiplier * qualityMultiplier);

        Debug.Log($"Damage calculation: ({move.basePower} (move) + {attacker.attack} (ATK)) × {typeMultiplier} (type) × {qualityMultiplier:F2} (quality) = {finalDamage}");

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
    private IEnumerator ExecuteHealingMove(MoveData move, BattleUnit healer, float qualityMultiplier = 1f)
    {
        int healAmount = Mathf.RoundToInt(move.basePower * qualityMultiplier);
        int oldHealth = healer.currentHealth;

        healer.currentHealth = Mathf.Min(healer.currentHealth + healAmount, healer.maxHealth);
        int actualHeal = healer.currentHealth - oldHealth;

        Debug.Log($"{healer.unitName} heals for {actualHeal} HP! (Quality: {qualityMultiplier:F2}x)");

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
    /// Execute a defensive move (Block)
    /// </summary>
    private IEnumerator ExecuteDefensiveMove(MoveData move, BattleUnit defender, float qualityMultiplier = 1f)
    {
        // Calculate damage reduction based on quality
        // Better quality = better defense (30% to 70% reduction)
        float damageReduction = Mathf.Lerp(0.3f, 0.7f, qualityMultiplier);

        // Apply defensive buff to the unit
        defender.SetDefensiveStance(damageReduction);

        Debug.Log($"{defender.unitName} uses Block! Damage reduction: {damageReduction:P0} (Quality: {qualityMultiplier:F2}x)");

        // Visual feedback
        if (effectivenessText != null)
        {
            effectivenessText.text = $"Blocking! ({damageReduction:P0} damage reduction)";
            effectivenessText.color = new Color(0.5f, 0.5f, 1f); // Blue for defense
        }

        // Defensive animation (shield effect)
        yield return PlayDefensiveAnimation(defender);

        yield return new WaitForSeconds(1.2f);

        if (effectivenessText != null)
        {
            effectivenessText.text = "";
        }
    }

    /// <summary>
    /// Play defensive animation
    /// </summary>
    private IEnumerator PlayDefensiveAnimation(BattleUnit defender)
    {
        SpriteRenderer sprite = defender.GetComponent<SpriteRenderer>();
        if (sprite != null)
        {
            Color originalColor = sprite.color;
            Color shieldColor = new Color(0.5f, 0.5f, 1f, 1f); // Blue shield

            // Pulse blue (defensive stance)
            for (int i = 0; i < 3; i++)
            {
                sprite.color = shieldColor;
                yield return new WaitForSeconds(0.15f);
                sprite.color = originalColor;
                yield return new WaitForSeconds(0.15f);
            }
        }
    }

    /// <summary>
    /// Play animation for a specific move type with enhanced colors and effects
    /// </summary>
    private IEnumerator PlayMoveAnimation(MoveData move, BattleUnit attacker, BattleUnit target)
    {
        // Basic attack animation (move toward target)
        Vector3 originalPos = attacker.transform.position;
        float moveDistance = 1.5f * move.animationIntensity;
        Vector3 targetPos = originalPos + (attacker.isPlayerUnit ? Vector3.right : Vector3.left) * moveDistance;

        // Move forward (speed based on animation intensity)
        float elapsed = 0f;
        float moveDuration = 0.25f / Mathf.Clamp(move.animationIntensity, 0.5f, 2f);
        while (elapsed < moveDuration)
        {
            attacker.transform.position = Vector3.Lerp(originalPos, targetPos, elapsed / moveDuration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        // Screen shake based on move power
        if (mainCamera != null && move.screenShakeAmount > 0)
        {
            StartCoroutine(ScreenShake(move.screenShakeAmount * screenShakeMultiplier, 0.2f));
        }

        // Enhanced visual effect using move's unique colors
        yield return FlashEffectWithGradient(target, move.primaryColor, move.secondaryColor);

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
    /// Enhanced flash effect with gradient transition (primary to secondary color)
    /// </summary>
    private IEnumerator FlashEffectWithGradient(BattleUnit target, Color primaryColor, Color secondaryColor)
    {
        SpriteRenderer sprite = target.GetComponent<SpriteRenderer>();
        if (sprite != null)
        {
            Color originalColor = sprite.color;

            // Flash primary color
            sprite.color = primaryColor;
            yield return new WaitForSeconds(0.08f);

            // Transition to secondary color
            sprite.color = secondaryColor;
            yield return new WaitForSeconds(0.08f);

            // Return to original
            sprite.color = originalColor;
        }
    }

    /// <summary>
    /// Screen shake effect for powerful attacks
    /// </summary>
    private IEnumerator ScreenShake(float intensity, float duration)
    {
        if (mainCamera == null)
        {
            Debug.LogWarning("[MoveExecutor] Cannot shake screen - mainCamera not assigned!");
            yield break;
        }

        Vector3 originalPosition = mainCamera.transform.position;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float x = Random.Range(-1f, 1f) * intensity;
            float y = Random.Range(-1f, 1f) * intensity;

            mainCamera.transform.position = originalPosition + new Vector3(x, y, 0);

            elapsed += Time.deltaTime;
            yield return null;
        }

        // Return to original position
        mainCamera.transform.position = originalPosition;
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
