using UnityEngine;
using UnityEngine.UI;
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
        [SerializeField] private float projectileSpeed = 5f;
        [SerializeField] private float projectileScale = 1f;
        [SerializeField] private Vector3 projectileRotation = Vector3.zero;

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

            // Create default projectile prefab if none assigned
            if (projectilePrefab == null)
            {
                CreateDefaultProjectilePrefab();
            }
        }

        /// <summary>
        /// Play an attack animation using the next move drawing.
        /// Returns a coroutine that completes when the animation finishes.
        /// </summary>
        public IEnumerator PlayAttackAnimation(Transform source, Transform target, MoveData moveData)
        {
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

            Debug.Log($"AttackAnimationManager: Playing attack animation with move drawing");

            // Create projectile
            GameObject projectile = CreateProjectile(moveTexture, source.position);

            if (projectile == null)
            {
                Debug.LogError("AttackAnimationManager: Failed to create projectile!");
                yield break;
            }

            // Animate projectile from source to target
            yield return AnimateProjectile(projectile, source.position, target.position);

            // Clean up
            Destroy(projectile);
        }

        /// <summary>
        /// Create a projectile GameObject with the move drawing as its sprite
        /// </summary>
        private GameObject CreateProjectile(Texture2D moveTexture, Vector3 spawnPosition)
        {
            GameObject projectile;

            if (projectilePrefab != null)
            {
                projectile = Instantiate(projectilePrefab, spawnPosition, Quaternion.identity);
            }
            else
            {
                // Create simple projectile if no prefab
                projectile = new GameObject("AttackProjectile");
                projectile.transform.position = spawnPosition;
            }

            // Add or get Image component (for UI rendering)
            Image projectileImage = projectile.GetComponent<Image>();
            if (projectileImage == null)
            {
                Canvas canvas = projectile.GetComponent<Canvas>();
                if (canvas == null)
                {
                    canvas = projectile.AddComponent<Canvas>();
                    canvas.renderMode = RenderMode.WorldSpace;
                    canvas.sortingOrder = 100; // Render above other battle elements
                }

                projectileImage = projectile.AddComponent<Image>();
            }

            // Convert texture to sprite
            Sprite moveSprite = Texture2DToSprite(moveTexture);
            if (moveSprite != null)
            {
                projectileImage.sprite = moveSprite;
                projectileImage.preserveAspect = true;
            }
            else
            {
                Debug.LogError("AttackAnimationManager: Failed to create sprite from texture!");
            }

            // Set initial scale and rotation
            projectile.transform.localScale = Vector3.one * projectileScale;
            projectile.transform.rotation = Quaternion.Euler(projectileRotation);

            // Set initial alpha to 0 for fade-in
            Color color = projectileImage.color;
            color.a = 0f;
            projectileImage.color = color;

            return projectile;
        }

        /// <summary>
        /// Animate the projectile from source to target
        /// </summary>
        private IEnumerator AnimateProjectile(GameObject projectile, Vector3 startPos, Vector3 endPos)
        {
            Image projectileImage = projectile.GetComponent<Image>();
            float elapsed = 0f;
            float distance = Vector3.Distance(startPos, endPos);
            float duration = distance / projectileSpeed;

            Vector3 initialScale = projectile.transform.localScale;

            // Fade in
            yield return StartCoroutine(FadeProjectile(projectileImage, 0f, 1f, fadeInDuration));

            // Move from start to end
            elapsed = 0f;
            while (elapsed < duration)
            {
                float t = elapsed / duration;

                // Position
                projectile.transform.position = Vector3.Lerp(startPos, endPos, t);

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
                yield return null;
            }

            // Ensure final position
            projectile.transform.position = endPos;

            // Fade out
            yield return StartCoroutine(FadeProjectile(projectileImage, 1f, 0f, fadeOutDuration));
        }

        /// <summary>
        /// Fade projectile image from startAlpha to endAlpha
        /// </summary>
        private IEnumerator FadeProjectile(Image image, float startAlpha, float endAlpha, float duration)
        {
            if (image == null) yield break;

            float elapsed = 0f;
            Color color = image.color;

            while (elapsed < duration)
            {
                float t = elapsed / duration;
                color.a = Mathf.Lerp(startAlpha, endAlpha, t);
                image.color = color;
                elapsed += Time.deltaTime;
                yield return null;
            }

            color.a = endAlpha;
            image.color = color;
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
        }

        /// <summary>
        /// Convert Texture2D to Sprite
        /// </summary>
        private Sprite Texture2DToSprite(Texture2D texture)
        {
            if (texture == null) return null;

            Sprite sprite = Sprite.Create(
                texture,
                new Rect(0, 0, texture.width, texture.height),
                new Vector2(0.5f, 0.5f), // Pivot at center
                100f // Pixels per unit
            );

            return sprite;
        }

        /// <summary>
        /// Create a default projectile prefab if none is assigned
        /// </summary>
        private void CreateDefaultProjectilePrefab()
        {
            Debug.Log("AttackAnimationManager: No projectile prefab assigned, using runtime creation");
            // Projectiles will be created at runtime
        }

        /// <summary>
        /// Set spawn and target points for animations
        /// </summary>
        public void SetAttackPoints(Transform spawnPoint, Transform targetPoint)
        {
            playerAttackSpawnPoint = spawnPoint;
            enemyTargetPoint = targetPoint;
        }
    }
}
