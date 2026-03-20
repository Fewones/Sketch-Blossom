using UnityEngine;

/// <summary>
/// Handles player character movement in the world map using WASD controls
/// </summary>
public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed;

    // The corners of the area the player is able to move in
    [SerializeField] private Vector3[] leftConstraints;
    [SerializeField] private Vector3[] rightConstraints;
    [SerializeField] private Vector3[] lowerConstraints;
    [SerializeField] private Vector3[] upperConstraints;

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
        movement.x = ApplyHorizontalConstraints(movement.x);
        movement.y = ApplyVerticalConstraints(movement.y);

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

    public float ApplyHorizontalConstraints(float direction)
    {
        // Get current player position
        Vector3 currentPosition = this.GetPosition();

        if (direction < 0) {
            // Check for left constraints
            // z=0: constrain along the whole y axis
            // z=1: constrain if y is greater than constraint y value
            // z=-1: constrain if y is smaller than constraint y value
            bool constrainLeft = false;
            foreach (Vector3 leftConstraint in leftConstraints){
                switch (leftConstraint.z) {
                    case 0: constrainLeft = constrainLeft || (currentPosition.x < leftConstraint.x);
                    break;
                    case 1: constrainLeft = constrainLeft || ((currentPosition.x < leftConstraint.x) && (currentPosition.y > leftConstraint.y));
                    break;
                    case -1: constrainLeft = constrainLeft || ((currentPosition.x < leftConstraint.x) && (currentPosition.y < leftConstraint.y));
                    break;
                }
            }
            return constrainLeft ? 0 : direction;
        } 
        else if (direction > 0) {
            // Check for right constraints
            bool constrainRight = false;
            foreach (Vector3 rightConstraint in rightConstraints){
                switch (rightConstraint.z) {
                    case 0: constrainRight = constrainRight || (currentPosition.x > rightConstraint.x);
                    break;
                    case 1: constrainRight = constrainRight || ((currentPosition.x > rightConstraint.x) && (currentPosition.y > rightConstraint.y));
                    break;
                    case -1: constrainRight = constrainRight || ((currentPosition.x > rightConstraint.x) && (currentPosition.y < rightConstraint.y));
                    break;
                }
            }
            return constrainRight ? 0 : direction;
        } 
        else {
            return direction;
        }
    }

    public float ApplyVerticalConstraints(float direction)
    {
        // Get current player position
        Vector3 currentPosition = this.GetPosition();

        if (direction < 0) {
            // Check for lower constraints
            // z=0: constrain along the whole x axis
            // z=1: constrain if x is greater than constraint x value
            // z=-1: constrain if x is smaller than constraint x value
            bool constrainLower = false;
            foreach (Vector3 lowerConstraint in lowerConstraints){
                switch (lowerConstraint.z) {
                    case 0: constrainLower = constrainLower || (currentPosition.y < lowerConstraint.y);
                    break;
                    case 1: constrainLower = constrainLower || ((currentPosition.y < lowerConstraint.y) && (currentPosition.x > lowerConstraint.x));
                    break;
                    case -1: constrainLower = constrainLower || ((currentPosition.y < lowerConstraint.y) && (currentPosition.x < lowerConstraint.x));
                    break;
                }
            }
            return constrainLower ? 0 : direction;
        } 
        else if (direction > 0) {
            // Check for right constraints
            bool constrainUpper = false;
            foreach (Vector3 upperConstraint in upperConstraints){
                switch (upperConstraint.z) {
                    case 0: constrainUpper = constrainUpper || (currentPosition.y > upperConstraint.y);
                    break;
                    case 1: constrainUpper = constrainUpper || ((currentPosition.y > upperConstraint.y) && (currentPosition.x > upperConstraint.x));
                    break;
                    case -1: constrainUpper = constrainUpper || ((currentPosition.y > upperConstraint.y) && (currentPosition.x < upperConstraint.x));
                    break;
                }
            }
            return constrainUpper ? 0 : direction;
        } 
        else {
            return direction;
        }
    }
}
