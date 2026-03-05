using UnityEngine;

/// <summary>
/// Handles player character movement in the world map using WASD controls
/// </summary>
public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed;

    [SerializeField] private Sprite leftStep;
    [SerializeField] private Sprite rightStep;

    [Header("References")]
    [SerializeField] private SpriteRenderer spriteRenderer;

    private Sprite standingSprite;

    private Vector2 movement;
    private Rigidbody2D rb;
    private Animator animator; // Optional: if you add animations later

    private int stepCounter = 0;

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
            standingSprite = spriteRenderer.sprite;
        }

        animator = GetComponent<Animator>();
    }

    private void Update()
    {
        // Get WASD input
        movement.x = Input.GetAxisRaw("Horizontal"); // A/D or Left/Right arrows
        movement.y = Input.GetAxisRaw("Vertical");   // W/S or Up/Down arrows


        // Movement Constraints so Player cant leave the worldmap or walk on the sky
        if ((((this.GetPosition().x < -9) || 
             ((this.GetPosition().x < -4.2) && (this.GetPosition().y > -1))) && (movement.x < 0)) || 
             ((this.GetPosition().x > 9) && (movement.x > 0)))
        {
            movement.x = 0;
        }
        if (((this.GetPosition().y < -3) && (movement.y < 0)) || 
        ((((this.GetPosition().x < -4.3) && (this.GetPosition().y > -1.1)) || 
          ((this.GetPosition().x > -4.3) && (this.GetPosition().y > -0.3))) && (movement.y > 0)))
        {
            movement.y = 0;
        }

        // Normalize diagonal movement to prevent faster movement
        if (movement.magnitude > 1)
        {
            movement.Normalize();
        }

        if ((movement.x == 0) && (movement.y == 0))
        {
            spriteRenderer.sprite = standingSprite;
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
        stepCounter = (stepCounter % 30) + 1;
        if (stepCounter == 1)
        {
            spriteRenderer.sprite = leftStep;
        } else if (stepCounter == 16)
        {
            spriteRenderer.sprite = rightStep;
        }
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
