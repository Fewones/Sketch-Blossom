using UnityEngine;

/// <summary>
/// Handles player character movement in the world map using WASD controls
/// </summary>
public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed;

    // The player has 4 circle colliders to constrain movement
    [SerializeField] private CircleCollider2D leftCircleCollider;
    [SerializeField] private CircleCollider2D rightCircleCollider;
    [SerializeField] private CircleCollider2D lowerCircleCollider;
    [SerializeField] private CircleCollider2D upperCircleCollider;
    
    [SerializeField] private Sprite leftStep;
    [SerializeField] private Sprite rightStep;
    private Sprite standingSprite;

    [Header("References")]
    [SerializeField] private SpriteRenderer spriteRenderer;

    [SerializeField] private Transform background;
    [SerializeField] private float leftEnd;
    [SerializeField] private float rightEnd;    
    [SerializeField] private float lowerEnd;
    [SerializeField] private float upperEnd;

    [SerializeField] private bool movingBackground;
    private Rigidbody2D rbBackground;
    private EdgeCollider2D[] barriers;
    
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

        if (movingBackground) {
            rbBackground = background.GetComponent<Rigidbody2D>();
            if (rbBackground == null)
            {
                rbBackground = gameObject.AddComponent<Rigidbody2D>();
                rbBackground.gravityScale = 0f; // Top-down movement, no gravity
                rbBackground.constraints = RigidbodyConstraints2D.FreezeRotation; // Prevent rotation
            }  
        }

        if (background != null)
        {
            barriers = background.GetComponents<EdgeCollider2D>();
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
        // Check if the the player collides with an edge
        movement = ApplyEdgeColliders(movement);

        // Normalize diagonal movement to prevent faster movement
        if (movement.magnitude > 1)
        {
            movement.Normalize();
        }

        if ((movement.x == 0) && (movement.y == 0))
        {
            spriteRenderer.sprite = standingSprite;
        } else {
            stepCounter = (stepCounter % 30) + 1;
            if (stepCounter == 1)
            {
                spriteRenderer.sprite = leftStep;
            } else if (stepCounter == 16)
            {
                spriteRenderer.sprite = rightStep;
            }
        }

        // Move the player using physics (depending on the scene having a moving background)
        if (movingBackground){
            rbBackground.linearVelocity = moveBackground(movement) * moveSpeed;
            rb.linearVelocity = (movement + moveBackground(movement)) * moveSpeed;  
        } else{
            rb.linearVelocity = movement * moveSpeed;
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

    // returns the movement of the background (the movement of the background is opposite to the player)
    public Vector2 moveBackground(Vector2 movement)
    {
        float x = -movement.x;
        float y = -movement.y;
        // Since the background moves opposite to the player, > 0 means left/down and < 0 means right/up
        // The background should not move left if it is at its leftmost point or if the player is left of the middle
        if (((x < 0) && ((background.position.x < leftEnd) || (GetPosition().x < 0))) ||
            ((x > 0) && ((background.position.x > rightEnd) || (GetPosition().x > 0)))){
            x = 0;
        }
        if (((y < 0) && ((background.position.y < lowerEnd) || (GetPosition().y < 0))) ||
            ((y > 0) && ((background.position.y > upperEnd) || (GetPosition().y > 0)))){
            y = 0;
        }
        return new Vector2(x,y);
    }

    public Vector2 ApplyEdgeColliders(Vector2 movement)
    {
        float x = movement.x;
        float y = movement.y;

        foreach(EdgeCollider2D edgeCollider in barriers)
        {
            if (leftCircleCollider.IsTouching(edgeCollider) && (x < 0))
            {
                x = 0;
            }
            if (rightCircleCollider.IsTouching(edgeCollider) && (x > 0))
            {
                x = 0;
            }
            if (lowerCircleCollider.IsTouching(edgeCollider) && (y < 0))
            {
                y = 0;
            }
            if (upperCircleCollider.IsTouching(edgeCollider) && (y > 0))
            {
                y = 0;
            }
        }
        return new Vector2(x,y);
    }
}
