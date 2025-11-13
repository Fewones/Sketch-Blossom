using UnityEngine;
using TMPro;
using System.Collections;

/// <summary>
/// Represents a unit in battle (player or enemy)
/// </summary>
public class BattleUnit : MonoBehaviour
{
    [Header("Unit Stats")]
    public string unitName = "Unit";
    public int maxHealth = 30;
    public int currentHealth;
    public int attack = 10;
    public int defense = 5;
    
    [Header("Visual")]
    public SpriteRenderer spriteRenderer;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI healthText;
    
    [Header("Animation")]
    public float attackMoveDistance = 1f;
    public float attackDuration = 0.3f;
    
    [Header("Unit Type")]
    public bool isPlayerUnit = false;

    private Vector3 originalPosition;
    private Color originalColor;

    private void Start()
    {
        currentHealth = maxHealth;
        originalPosition = transform.position;
        
        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }
        
        UpdateUI();
    }

    /// <summary>
    /// Initialize unit from DrawnUnitData (for player unit)
    /// </summary>
    public void InitializeFromDrawing(DrawnUnitData data)
    {
        if (data == null)
        {
            Debug.LogWarning("No DrawnUnitData found, using defaults");
            return;
        }

        attack = data.attack;
        defense = data.defense;
        maxHealth = data.health;
        currentHealth = maxHealth;

        // Set name based on detected plant type
        string plantName = !string.IsNullOrEmpty(data.plantDisplayName) ? data.plantDisplayName : data.plantType.ToString();
        unitName = $"{plantName} ({data.element})";

        Debug.Log($"Player unit initialized: {unitName}");
        Debug.Log($"Stats: ATK {attack}, DEF {defense}, HP {currentHealth}");
        Debug.Log($"Detection Confidence: {data.detectionConfidence:P0}");

        UpdateUI();
    }

    /// <summary>
    /// Initialize enemy with specific stats
    /// </summary>
    public void InitializeAsEnemy(string name, int hp, int atk, int def)
    {
        unitName = name;
        maxHealth = hp;
        currentHealth = hp;
        attack = atk;
        defense = def;
        isPlayerUnit = false;

        UpdateUI();
    }

    /// <summary>
    /// Take damage from an attack
    /// </summary>
    public void TakeDamage(int damage)
    {
        // Apply defense
        int actualDamage = Mathf.Max(1, damage - defense);
        currentHealth -= actualDamage;
        currentHealth = Mathf.Max(0, currentHealth);

        Debug.Log($"{unitName} takes {actualDamage} damage! ({currentHealth}/{maxHealth} HP)");

        UpdateUI();
        StartCoroutine(DamageFlash());

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    /// <summary>
    /// Attack another unit
    /// </summary>
    public IEnumerator AttackAnimation(BattleUnit target, int damageAmount)
    {
        Debug.Log($"{unitName} attacks {target.unitName} for {damageAmount} damage!");

        // Move toward target
        Vector3 targetPos = originalPosition + (isPlayerUnit ? Vector3.right : Vector3.left) * attackMoveDistance;
        
        float elapsed = 0f;
        while (elapsed < attackDuration / 2)
        {
            transform.position = Vector3.Lerp(originalPosition, targetPos, elapsed / (attackDuration / 2));
            elapsed += Time.deltaTime;
            yield return null;
        }

        // Deal damage
        target.TakeDamage(damageAmount);

        // Return to position
        elapsed = 0f;
        while (elapsed < attackDuration / 2)
        {
            transform.position = Vector3.Lerp(targetPos, originalPosition, elapsed / (attackDuration / 2));
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.position = originalPosition;
    }

    private IEnumerator DamageFlash()
    {
        if (spriteRenderer == null) yield break;

        spriteRenderer.color = Color.red;
        yield return new WaitForSeconds(0.15f);
        spriteRenderer.color = originalColor;
    }

    private void Die()
    {
        Debug.Log($"{unitName} has been defeated!");
        // Visual feedback - fade out
        if (spriteRenderer != null)
        {
            StartCoroutine(FadeOut());
        }
    }

    private IEnumerator FadeOut()
    {
        float elapsed = 0f;
        float duration = 0.5f;
        Color startColor = spriteRenderer.color;

        while (elapsed < duration)
        {
            float alpha = Mathf.Lerp(1f, 0.3f, elapsed / duration);
            spriteRenderer.color = new Color(startColor.r, startColor.g, startColor.b, alpha);
            elapsed += Time.deltaTime;
            yield return null;
        }
    }

    private void UpdateUI()
    {
        if (nameText != null)
        {
            nameText.text = unitName;
        }

        if (healthText != null)
        {
            healthText.text = $"HP: {currentHealth}/{maxHealth}\nATK: {attack} | DEF: {defense}";
        }
    }

    public bool IsAlive()
    {
        return currentHealth > 0;
    }
}
