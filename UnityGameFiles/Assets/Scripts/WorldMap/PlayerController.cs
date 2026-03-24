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

    public SpawnManager spawnManager;
    private Sprite standingSprite;  
    private int stepCounter = 0;
    private Vector2 movement;
    private Rigidbody2D rb;

    [Header("References")]
    [SerializeField] private SpriteRenderer spriteRenderer;

    [SerializeField] private Transform background;
    [SerializeField] private float leftEnd;
    [SerializeField] private float rightEnd;    
    [SerializeField] private float lowerEnd;
    [SerializeField] private float upperEnd;

    [SerializeField] public bool movingBackground;
    private Rigidbody2D rbBackground;
    private EdgeCollider2D[] barriers;

    [Header("Progression")]
    [SerializeField] public BoxCollider2D formerLoadingZone; 
    [SerializeField] public BoxCollider2D nextLoadingZone; 
    [SerializeField] public int currentWorldMap;
    
    public bool changeWorldMap;

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

        

        if (spawnManager == null)
        {
            spawnManager = FindObjectOfType<SpawnManager>();
        }
        spawnManager.currentWorldMap = currentWorldMap;

        barriers = background.GetComponents<EdgeCollider2D>();
        if (movingBackground) {
                rbBackground = background.GetComponent<Rigidbody2D>();
            }

        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            standingSprite = spriteRenderer.sprite;
        }

        if (spawnManager.facingLeft){
                    spriteRenderer.flipX = true;
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
        // If the player should progress to another WorldMap, freeze movement
        if (formerLoadingZone != null)
        {
           if (checkWorldMapProgression(formerLoadingZone, changeWorldMap))
            {
                movement = Vector2.zero;
                SetSpawnPosition(currentWorldMap, spawnManager.formerOffset[currentWorldMap-1]);
                if (movingBackground)
                {
                   SetBackgroundPosition(currentWorldMap); 
                }
                currentWorldMap -= 1;
                spawnManager.facingLeft = true;
                changeWorldMap = true;
            } 
        }

        if (nextLoadingZone != null)
        {
          if (checkWorldMapProgression(nextLoadingZone, changeWorldMap))
            {
                movement = Vector2.zero;
                SetSpawnPosition(currentWorldMap, spawnManager.nextOffset[currentWorldMap-1]);
                if (movingBackground)
                {
                   SetBackgroundPosition(currentWorldMap); 
                }
                currentWorldMap += 1;
                spawnManager.facingLeft = false;
                changeWorldMap = true;
            }  
        }
        
        
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

    public Vector2 GetSpawnPosition()
    {
        return spawnManager.playerSpawnPoints[currentWorldMap-1];
    }

    public void SetSpawnPosition(int i, Vector2 offset)
    {   
        spawnManager.playerSpawnPoints[i-1] = GetPosition();
        spawnManager.playerSpawnPoints[i-1] += offset;
    }

    public Vector2 GetBackgroundPosition()
    {
        // WorldMaps 2,4 and 6 have moving backgrounds, so you can just use (currentWorldMap/2)-1
        return spawnManager.backgroundSpawnPoints[(currentWorldMap/2)-1];
    }

    public void SetBackgroundPosition(int i)
    {
        // WorldMaps 2,4 and 6 have moving backgrounds, so you can just use (currentWorldMap/2)-1
        spawnManager.backgroundSpawnPoints[(i/2)-1] = rbBackground.position;
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

    private bool checkWorldMapProgression(BoxCollider2D loadingZone, bool currentlyChanging)
    {
        return !currentlyChanging && 
                (leftCircleCollider.IsTouching(loadingZone) ||
                rightCircleCollider.IsTouching(loadingZone) ||
                lowerCircleCollider.IsTouching(loadingZone) ||
                upperCircleCollider.IsTouching(loadingZone));
    }

    public void ResetSpawnManager()
    {
        spawnManager.ResetValues();
    }
}
