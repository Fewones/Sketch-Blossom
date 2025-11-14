using UnityEngine;
using System.Collections;

namespace SketchBlossom.Battle
{
    /// <summary>
    /// Manages attack animations using player-drawn move textures.
    /// Creates projectiles/effects from drawings and animates them during combat.
    /// </summary>
    public class AttackAnimationManager : MonoBehaviour
    {
        [Header("Projectile Settings")]
        [SerializeField] private GameObject projectilePrefab;
        [SerializeField] private Transform playerAttackSpawnPoint;
        [SerializeField] private Transform enemyTargetPoint;
        [SerializeField] private float projectileDuration = 0.8f; // Fixed duration instead of speed
        [SerializeField] private float projectileScale = 2f; // Increased from 0.5 for visibility
        [SerializeField] private Vector3 projectileRotation = Vector3.zero;
        [SerializeField] private float projectileZOffset = -5f; // Z offset from camera

        [Header("Animation Settings")]
        [SerializeField] private float fadeInDuration = 0.2f;
        [SerializeField] private float fadeOutDuration = 0.2f;
        [SerializeField] private bool rotateProjectile = true;
        [SerializeField] private float rotationSpeed = 360f;

        [Header("Effect Settings")]
        [SerializeField] private bool scaleAnimation = true;
        [SerializeField] private AnimationCurve scaleCurve = AnimationCurve.EaseInOut(0, 1, 1, 1);
        [SerializeField] private float maxScale = 1.2f;

        [Header("References")]
        [SerializeField] private DrawnMoveStorage moveStorage;

        private void Awake()
        {
            // Auto-find move storage if not assigned
            if (moveStorage == null)
            {
                moveStorage = DrawnMoveStorage.Instance;
                if (moveStorage == null)
                {
                    Debug.LogWarning("AttackAnimationManager: DrawnMoveStorage not found!");
                }
            }

            Debug.Log("AttackAnimationManager: Initialized");
        }

        /// <summary>
        /// Play an attack animation using the next move drawing.
        /// Returns a coroutine that completes when the animation finishes.
        /// </summary>
        public IEnumerator PlayAttackAnimation(Transform source, Transform target, MoveData moveData)
        {
            Debug.Log($"AttackAnimationManager: PlayAttackAnimation called - Source: {source?.position}, Target: {target?.position}");

            if (source == null || target == null)
            {
                Debug.LogError("AttackAnimationManager: Source or target transform is null!");
                yield return new WaitForSeconds(0.5f);
                yield break;
            }

            if (moveStorage == null || !moveStorage.HasDrawings())
            {
                Debug.LogWarning("AttackAnimationManager: No move drawings available for animation!");
                yield return PlayFallbackAnimation(source, target, moveData);
                yield break;
            }

            // Get the next move drawing
            Texture2D moveTexture = moveStorage.GetNextMoveDrawing();

            if (moveTexture == null)
            {
                Debug.LogWarning("AttackAnimationManager: Failed to get move drawing!");
                yield return PlayFallbackAnimation(source, target, moveData);
                yield break;
            }

            Debug.Log($"AttackAnimationManager: Creating projectile with texture {moveTexture.width}x{moveTexture.height}");

            // Create projectile
            GameObject projectile = CreateProjectile(moveTexture, source.position);

            if (projectile == null)
            {
                Debug.LogError("AttackAnimationManager: Failed to create projectile!");
                yield return PlayFallbackAnimation(source, target, moveData);
                yield break;
            }

            Debug.Log("AttackAnimationManager: Projectile created successfully, starting animation");

            // Animate projectile from source to target
            yield return StartCoroutine(AnimateProjectile(projectile, source.position, target.position));

            Debug.Log("AttackAnimationManager: Animation completed, cleaning up");

            // Clean up
            if (projectile != null)
            {
                Destroy(projectile);
            }
        }

        /// <summary>
        /// Create a projectile GameObject with the move drawing as its sprite using SpriteRenderer
        /// </summary>
        private GameObject CreateProjectile(Texture2D moveTexture, Vector3 spawnPosition)
        {
            try
            {
                Debug.Log($"AttackAnimationManager: CreateProjectile at {spawnPosition}");

                GameObject projectile = new GameObject("AttackProjectile");

                // Adjust Z position to be in front of camera but behind UI
                Vector3 adjustedPosition = spawnPosition;
                adjustedPosition.z = projectileZOffset;
                projectile.transform.position = adjustedPosition;

                Debug.Log($"AttackAnimationManager: Adjusted position to {adjustedPosition} (Z offset: {projectileZOffset})");

                // Use SpriteRenderer instead of Canvas/Image for reliable world-space rendering
                SpriteRenderer spriteRenderer = projectile.AddComponent<SpriteRenderer>();

                // Convert texture to sprite
                Sprite moveSprite = Texture2DToSprite(moveTexture);
                if (moveSprite != null)
                {
                    spriteRenderer.sprite = moveSprite;
                    Debug.Log("AttackAnimationManager: Sprite assigned to SpriteRenderer");
                }
                else
                {
                    Debug.LogError("AttackAnimationManager: Failed to create sprite from texture!");
                    Destroy(projectile);
                    return null;
                }

                // Set sorting layer to render above battle elements
                spriteRenderer.sortingLayerName = "Default";
                spriteRenderer.sortingOrder = 1000; // Very high to ensure visibility

                // Set initial scale and rotation
                projectile.transform.localScale = Vector3.one * projectileScale;
                projectile.transform.rotation = Quaternion.Euler(projectileRotation);

                // Set initial alpha to 0 for fade-in
                Color color = spriteRenderer.color;
                color.a = 0f;
                spriteRenderer.color = color;

                Debug.Log($"AttackAnimationManager: Projectile setup complete - Position: {projectile.transform.position}, Scale: {projectileScale}, SortingOrder: {spriteRenderer.sortingOrder}, Color: {color}");
                Debug.Log($"AttackAnimationManager: Sprite bounds: {spriteRenderer.sprite.bounds}, Sprite size: {spriteRenderer.sprite.rect.size}");

                return projectile;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"AttackAnimationManager: Exception creating projectile: {e.Message}\n{e.StackTrace}");
                return null;
            }
        }

        /// <summary>
        /// Animate the projectile from source to target
        /// </summary>
        private IEnumerator AnimateProjectile(GameObject projectile, Vector3 startPos, Vector3 endPos)
        {
            // Adjust Z positions
            startPos.z = projectileZOffset;
            endPos.z = projectileZOffset;

            Debug.Log($"AttackAnimationManager: AnimateProjectile from {startPos} to {endPos}");

            SpriteRenderer spriteRenderer = projectile.GetComponent<SpriteRenderer>();
            if (spriteRenderer == null)
            {
                Debug.LogError("AttackAnimationManager: No SpriteRenderer on projectile!");
                yield break;
            }

            // Verify sprite is actually assigned and visible
            Debug.Log($"AttackAnimationManager: Sprite check - Assigned: {spriteRenderer.sprite != null}, Enabled: {spriteRenderer.enabled}, GameObject active: {projectile.activeSelf}");

            float distance = Vector3.Distance(startPos, endPos);
            float duration = projectileDuration; // Use fixed duration regardless of distance
            Vector3 initialScale = projectile.transform.localScale;

            Debug.Log($"AttackAnimationManager: Distance: {distance}, Duration: {duration}s (fixed)");

            // Fade in
            Debug.Log("AttackAnimationManager: Starting fade in");
            yield return StartCoroutine(FadeProjectile(spriteRenderer, 0f, 1f, fadeInDuration));

            // Move from start to end
            Debug.Log("AttackAnimationManager: Starting movement");
            float elapsed = 0f;
            int frameCount = 0;
            while (elapsed < duration)
            {
                float t = elapsed / duration;

                // Position
                projectile.transform.position = Vector3.Lerp(startPos, endPos, t);

                // Log position every 10 frames for debugging
                if (frameCount % 10 == 0)
                {
                    Debug.Log($"AttackAnimationManager: Frame {frameCount}, Position: {projectile.transform.position}, Alpha: {spriteRenderer.color.a}");
                }

                // Rotation (optional)
                if (rotateProjectile)
                {
                    projectile.transform.Rotate(Vector3.forward, rotationSpeed * Time.deltaTime);
                }

                // Scale animation (optional)
                if (scaleAnimation)
                {
                    float scaleMultiplier = Mathf.Lerp(1f, maxScale, scaleCurve.Evaluate(t));
                    projectile.transform.localScale = initialScale * scaleMultiplier;
                }

                elapsed += Time.deltaTime;
                frameCount++;
                yield return null;
            }

            // Ensure final position
            projectile.transform.position = endPos;
            Debug.Log($"AttackAnimationManager: Reached target position: {endPos}");

            // Fade out
            Debug.Log("AttackAnimationManager: Starting fade out");
            yield return StartCoroutine(FadeProjectile(spriteRenderer, 1f, 0f, fadeOutDuration));

            Debug.Log("AttackAnimationManager: AnimateProjectile complete");
        }

        /// <summary>
        /// Fade projectile sprite from startAlpha to endAlpha
        /// </summary>
        private IEnumerator FadeProjectile(SpriteRenderer spriteRenderer, float startAlpha, float endAlpha, float duration)
        {
            if (spriteRenderer == null)
            {
                Debug.LogWarning("AttackAnimationManager: FadeProjectile - spriteRenderer is null");
                yield break;
            }

            float elapsed = 0f;
            Color color = spriteRenderer.color;

            while (elapsed < duration)
            {
                float t = elapsed / duration;
                color.a = Mathf.Lerp(startAlpha, endAlpha, t);
                spriteRenderer.color = color;
                elapsed += Time.deltaTime;
                yield return null;
            }

            color.a = endAlpha;
            spriteRenderer.color = color;
        }

        /// <summary>
        /// Play a fallback animation when no move drawing is available
        /// </summary>
        private IEnumerator PlayFallbackAnimation(Transform source, Transform target, MoveData moveData)
        {
            Debug.Log("AttackAnimationManager: Playing fallback animation (no drawing available)");

            // Simple flash effect or placeholder animation
            // For now, just wait a short duration
            yield return new WaitForSeconds(0.5f);

            Debug.Log("AttackAnimationManager: Fallback animation complete");
        }

        /// <summary>
        /// Convert Texture2D to Sprite
        /// </summary>
        private Sprite Texture2DToSprite(Texture2D texture)
        {
            if (texture == null)
            {
                Debug.LogError("AttackAnimationManager: Texture2DToSprite - texture is null");
                return null;
            }

            try
            {
                Sprite sprite = Sprite.Create(
                    texture,
                    new Rect(0, 0, texture.width, texture.height),
                    new Vector2(0.5f, 0.5f), // Pivot at center
                    100f // Pixels per unit
                );

                Debug.Log($"AttackAnimationManager: Created sprite {sprite.bounds.size}");
                return sprite;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"AttackAnimationManager: Exception creating sprite: {e.Message}");
                return null;
            }
        }

        /// <summary>
        /// Set spawn and target points for animations
        /// </summary>
        public void SetAttackPoints(Transform spawnPoint, Transform targetPoint)
        {
            playerAttackSpawnPoint = spawnPoint;
            enemyTargetPoint = targetPoint;
            Debug.Log($"AttackAnimationManager: Attack points set - Spawn: {spawnPoint?.position}, Target: {targetPoint?.position}");
        }
    }
}
