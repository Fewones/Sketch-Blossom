using UnityEngine;

/// <summary>
/// Represents the Healing Center on the world map.
/// Players can interact with it to heal one plant after each battle victory.
/// Tracks available healing charges (earned by winning battles).
/// </summary>
public class HealingCenter : MonoBehaviour
{
    [Header("Interaction Settings")]
    [SerializeField] private float interactionRange = 1.2f;
    [SerializeField] private KeyCode interactionKey = KeyCode.E;

    [Header("Visual Feedback")]
    [SerializeField] private GameObject interactionPrompt;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Color highlightColor = new Color(1f, 1f, 1f, 1f); // Bright white highlight
    [SerializeField] private Color baseColor = new Color(0.85f, 0.85f, 0.85f, 1f); // Slightly dimmed when idle

    private Transform player;
    private bool playerInRange = false;
    private HealingCenterUI healingUI;

    // Static healing charge tracker - persists across scene reloads via PlayerPrefs
    private const string HEAL_CHARGES_KEY = "HealingCenter_Charges";

    public static int HealCharges
    {
        get { return PlayerPrefs.GetInt(HEAL_CHARGES_KEY, 0); }
        private set
        {
            PlayerPrefs.SetInt(HEAL_CHARGES_KEY, value);
            PlayerPrefs.Save();
        }
    }

    /// <summary>
    /// Called after winning a battle to grant a healing charge
    /// </summary>
    public static void GrantHealCharge()
    {
        int current = PlayerPrefs.GetInt(HEAL_CHARGES_KEY, 0);
        PlayerPrefs.SetInt(HEAL_CHARGES_KEY, current + 1);
        PlayerPrefs.Save();
        Debug.Log($"Healing charge granted! Total charges: {current + 1}");
    }

    /// <summary>
    /// Consume one healing charge. Returns true if successful.
    /// </summary>
    public static bool ConsumeHealCharge()
    {
        int current = PlayerPrefs.GetInt(HEAL_CHARGES_KEY, 0);
        if (current <= 0) return false;

        PlayerPrefs.SetInt(HEAL_CHARGES_KEY, current - 1);
        PlayerPrefs.Save();
        Debug.Log($"Healing charge consumed. Remaining: {current - 1}");
        return true;
    }

    private void Start()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }

        healingUI = FindObjectOfType<HealingCenterUI>();

        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        if (spriteRenderer != null)
        {
            baseColor = spriteRenderer.color;
        }

        if (interactionPrompt == null)
        {
            CreateInteractionPrompt();
        }

        if (interactionPrompt != null)
        {
            interactionPrompt.SetActive(false);
        }
    }

    private void CreateInteractionPrompt()
    {
        GameObject promptObj = new GameObject("InteractionPrompt");
        promptObj.transform.SetParent(transform);
        promptObj.transform.localPosition = new Vector3(0, 2.5f, 0);

        Canvas canvas = promptObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;

        RectTransform canvasRect = canvas.GetComponent<RectTransform>();
        canvasRect.sizeDelta = new Vector2(200f, 50f);
        canvasRect.localScale = new Vector3(0.005f, 0.005f, 0.005f);

        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(promptObj.transform);

        UnityEngine.UI.Text text = textObj.AddComponent<UnityEngine.UI.Text>();
        text.text = "Press E";
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = 24;
        text.alignment = TextAnchor.MiddleCenter;
        text.color = Color.white;
        text.horizontalOverflow = HorizontalWrapMode.Overflow;
        text.verticalOverflow = VerticalWrapMode.Overflow;

        RectTransform textRect = text.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;

        GameObject bgObj = new GameObject("Background");
        bgObj.transform.SetParent(promptObj.transform);
        bgObj.transform.SetAsFirstSibling();

        UnityEngine.UI.Image bgImage = bgObj.AddComponent<UnityEngine.UI.Image>();
        bgImage.color = new Color(0, 0.3f, 0.1f, 0.7f); // Dark green background

        RectTransform bgRect = bgImage.GetComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.offsetMin = Vector2.zero;
        bgRect.offsetMax = Vector2.zero;

        interactionPrompt = promptObj;
    }

    private void Update()
    {
        if (player == null) return;

        float distance = Vector2.Distance(transform.position, player.position);
        bool wasInRange = playerInRange;
        playerInRange = distance <= interactionRange;

        if (playerInRange != wasInRange)
        {
            OnRangeChanged(playerInRange);
        }

        if (playerInRange && Input.GetKeyDown(interactionKey))
        {
            OnInteract();
        }
    }

    private void OnRangeChanged(bool inRange)
    {
        if (interactionPrompt != null)
        {
            interactionPrompt.SetActive(inRange);
        }

        if (spriteRenderer != null)
        {
            spriteRenderer.color = inRange ? highlightColor : baseColor;
        }
    }

    private void OnInteract()
    {
        if (healingUI == null)
        {
            healingUI = FindObjectOfType<HealingCenterUI>();
        }

        if (healingUI != null)
        {
            healingUI.Show();
        }
        else
        {
            Debug.LogError("HealingCenterUI not found in scene!");
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, interactionRange);
    }
}
