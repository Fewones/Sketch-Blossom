using UnityEngine;

/// <summary>
/// Handles player character movement in the world map using WASD controls
/// </summary>
public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;

    [Header("References")]
    [SerializeField] private SpriteRenderer spriteRenderer;

    private Vector2 movement;
    private Rigidbody2D rb;
    private Animator animator; // Optional: if you add animations later

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f; // Top-down movement, no gravity
            rb.constraints = RigidbodyConstraints2D.FreezeRotation; // Prevent rotation
        }

        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        animator = GetComponent<Animator>();
    }

    private void Update()
    {
        // Get WASD input
        movement.x = Input.GetAxisRaw("Horizontal"); // A/D or Left/Right arrows
        movement.y = Input.GetAxisRaw("Vertical");   // W/S or Up/Down arrows

        // Normalize diagonal movement to prevent faster movement
        if (movement.magnitude > 1)
        {
            movement.Normalize();
        }

        // Update sprite facing direction
        if (movement.x != 0 && spriteRenderer != null)
        {
            spriteRenderer.flipX = movement.x < 0; // Flip sprite when moving left
        }

        // Optional: Update animator parameters if animator exists
        if (animator != null)
        {
            animator.SetFloat("Speed", movement.magnitude);
            animator.SetFloat("Horizontal", movement.x);
            animator.SetFloat("Vertical", movement.y);
        }
    }

    private void FixedUpdate()
    {
        // Move the player using physics
        rb.linearVelocity = movement * moveSpeed;
    }

    /// <summary>
    /// Allow external scripts to enable/disable player movement
    /// </summary>
    public void SetMovementEnabled(bool enabled)
    {
        this.enabled = enabled;
        if (!enabled)
        {
            rb.linearVelocity = Vector2.zero;
            movement = Vector2.zero;
        }
    }

    /// <summary>
    /// Get the player's current position
    /// </summary>
    public Vector3 GetPosition()
    {
        return transform.position;
    }
}
