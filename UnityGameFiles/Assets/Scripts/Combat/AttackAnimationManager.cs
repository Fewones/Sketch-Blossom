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
        [SerializeField] private float projectileDuration = 0.8f; // Fixed duration instead of speed
        [SerializeField] private float projectileScale = 200f; // Size for UI (pixels)
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
        [SerializeField] private Canvas battleCanvas; // The canvas that contains battle UI

        [Header("Debug")]
        [SerializeField] private bool useDebugSquare = false; // Use white square instead of drawing for testing

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

            // Auto-find battle canvas
            if (battleCanvas == null)
            {
                battleCanvas = FindFirstObjectByType<Canvas>();
                if (battleCanvas != null)
                {
                    Debug.Log($"AttackAnimationManager: Found canvas '{battleCanvas.name}' with renderMode: {battleCanvas.renderMode}");
                }
                else
                {
                    Debug.LogError("AttackAnimationManager: No Canvas found in scene!");
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

            // For debugging: optionally use a simple white square instead of the drawing
            Texture2D projectileTexture = moveTexture;
            if (useDebugSquare)
            {
                projectileTexture = CreateDebugSquareTexture();
                Debug.Log("AttackAnimationManager: Using DEBUG WHITE SQUARE instead of drawing");
            }

            // Create projectile as UI element
            GameObject projectile = CreateUIProjectile(projectileTexture, source.position);

            if (projectile == null)
            {
                Debug.LogError("AttackAnimationManager: Failed to create projectile!");
                yield return PlayFallbackAnimation(source, target, moveData);
                yield break;
            }

            Debug.Log("AttackAnimationManager: Projectile created successfully, starting animation");

            // Animate projectile from source to target
            yield return StartCoroutine(AnimateUIProjectile(projectile, source, target));

            Debug.Log("AttackAnimationManager: Animation completed, cleaning up");

            // Clean up
            if (projectile != null)
            {
                Destroy(projectile);
            }
        }

        /// <summary>
        /// Create a projectile GameObject as a UI Image element
        /// </summary>
        private GameObject CreateUIProjectile(Texture2D moveTexture, Vector3 spawnPosition)
        {
            try
            {
                Debug.Log($"AttackAnimationManager: CreateUIProjectile at {spawnPosition}");

                if (battleCanvas == null)
                {
                    Debug.LogError("AttackAnimationManager: No battle canvas available!");
                    return null;
                }

                // Create projectile GameObject as child of canvas
                GameObject projectile = new GameObject("AttackProjectile");
                projectile.transform.SetParent(battleCanvas.transform, false);

                // Add RectTransform for UI positioning
                RectTransform rectTransform = projectile.AddComponent<RectTransform>();
                rectTransform.sizeDelta = new Vector2(projectileScale, projectileScale);
                rectTransform.anchorMin = new Vector2(0, 0);
                rectTransform.anchorMax = new Vector2(0, 0);
                rectTransform.pivot = new Vector2(0.5f, 0.5f);

                // Set position to spawn point (will be the source unit's UI position)
                rectTransform.position = spawnPosition;

                Debug.Log($"AttackAnimationManager: UI position set to {rectTransform.position}, anchoredPosition: {rectTransform.anchoredPosition}");

                // Add Image component
                Image image = projectile.AddComponent<Image>();

                // Convert texture to sprite
                Sprite moveSprite = Texture2DToSprite(moveTexture);
                if (moveSprite != null)
                {
                    image.sprite = moveSprite;
                    image.preserveAspect = true;
                    Debug.Log("AttackAnimationManager: Sprite assigned to UI Image");
                }
                else
                {
                    Debug.LogError("AttackAnimationManager: Failed to create sprite from texture!");
                    Destroy(projectile);
                    return null;
                }

                // Set initial alpha to 0 for fade-in
                Color color = image.color;
                color.a = 0f;
                image.color = color;

                Debug.Log($"AttackAnimationManager: UI Projectile setup complete - Size: {projectileScale}x{projectileScale}, Color: {color}");
                Debug.Log($"AttackAnimationManager: Sprite size: {moveSprite.rect.size}");

                return projectile;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"AttackAnimationManager: Exception creating UI projectile: {e.Message}\n{e.StackTrace}");
                return null;
            }
        }

        /// <summary>
        /// Animate the UI projectile from source to target
        /// </summary>
        private IEnumerator AnimateUIProjectile(GameObject projectile, Transform source, Transform target)
        {
            RectTransform rectTransform = projectile.GetComponent<RectTransform>();
            Image image = projectile.GetComponent<Image>();

            if (rectTransform == null || image == null)
            {
                Debug.LogError("AttackAnimationManager: Missing RectTransform or Image!");
                yield break;
            }

            Vector3 startPos = source.position;
            Vector3 endPos = target.position;

            Debug.Log($"AttackAnimationManager: AnimateUIProjectile from {startPos} to {endPos}");
            Debug.Log($"AttackAnimationManager: Image check - Assigned: {image.sprite != null}, Enabled: {image.enabled}, GameObject active: {projectile.activeSelf}");

            float duration = projectileDuration;
            Vector3 initialScale = rectTransform.localScale;

            Debug.Log($"AttackAnimationManager: Distance: {Vector3.Distance(startPos, endPos)}, Duration: {duration}s (fixed)");

            // Fade in
            Debug.Log("AttackAnimationManager: Starting fade in");
            yield return StartCoroutine(FadeUIImage(image, 0f, 1f, fadeInDuration));

            // Move from start to end
            Debug.Log("AttackAnimationManager: Starting movement");
            float elapsed = 0f;
            int frameCount = 0;
            while (elapsed < duration)
            {
                float t = elapsed / duration;

                // Position
                rectTransform.position = Vector3.Lerp(startPos, endPos, t);

                // Log position every 10 frames for debugging
                if (frameCount % 10 == 0)
                {
                    Debug.Log($"AttackAnimationManager: Frame {frameCount}, Position: {rectTransform.position}, Alpha: {image.color.a}");
                }

                // Rotation (optional)
                if (rotateProjectile)
                {
                    rectTransform.Rotate(Vector3.forward, rotationSpeed * Time.deltaTime);
                }

                // Scale animation (optional)
                if (scaleAnimation)
                {
                    float scaleMultiplier = Mathf.Lerp(1f, maxScale, scaleCurve.Evaluate(t));
                    rectTransform.localScale = initialScale * scaleMultiplier;
                }

                elapsed += Time.deltaTime;
                frameCount++;
                yield return null;
            }

            // Ensure final position
            rectTransform.position = endPos;
            Debug.Log($"AttackAnimationManager: Reached target position: {endPos}");

            // Fade out
            Debug.Log("AttackAnimationManager: Starting fade out");
            yield return StartCoroutine(FadeUIImage(image, 1f, 0f, fadeOutDuration));

            Debug.Log("AttackAnimationManager: AnimateUIProjectile complete");
        }

        /// <summary>
        /// Fade UI image from startAlpha to endAlpha
        /// </summary>
        private IEnumerator FadeUIImage(Image image, float startAlpha, float endAlpha, float duration)
        {
            if (image == null)
            {
                Debug.LogWarning("AttackAnimationManager: FadeUIImage - image is null");
                yield break;
            }

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

            Debug.Log("AttackAnimationManager: Fallback animation complete");
        }

        /// <summary>
        /// Create a simple white square texture for debugging
        /// </summary>
        private Texture2D CreateDebugSquareTexture()
        {
            int size = 256;
            Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);

            Color[] pixels = new Color[size * size];
            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i] = Color.white; // Solid white
            }

            texture.SetPixels(pixels);
            texture.Apply();

            Debug.Log($"AttackAnimationManager: Created debug square texture {size}x{size}");
            return texture;
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
