using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace SketchBlossom.Battle
{
    /// <summary>
    /// Manages attack animations using sprite sheet assets mapped to each attack's VisualEffect.
    /// Falls back to player-drawn textures when no sprite asset is assigned.
    /// </summary>
    public class AttackAnimationManager : MonoBehaviour
    {
        [Header("Sprite Database")]
        [Tooltip("Assign the AttackSpriteDatabase ScriptableObject with sprite sheets for each effect")]
        [SerializeField] private AttackSpriteDatabase spriteDatabase;

        [Header("Projectile Settings")]
        [SerializeField] private GameObject projectilePrefab;
        [SerializeField] private Transform playerAttackSpawnPoint;
        [SerializeField] private Transform enemyTargetPoint;
        [SerializeField] private float projectileDuration = 0.8f;
        [SerializeField] private float projectileScale = 300f;
        [SerializeField] private Vector3 projectileRotation = Vector3.zero;

        [Header("Animation Settings")]
        [SerializeField] private float fadeInDuration = 0.1f;
        [SerializeField] private float fadeOutDuration = 0.1f;
        [SerializeField] private bool rotateProjectile = false;
        [SerializeField] private float rotationSpeed = 360f;

        [Header("Impact Settings")]
        [SerializeField] private float impactDuration = 0.4f;
        [SerializeField] private float impactScale = 250f;

        [Header("Effect Settings")]
        [SerializeField] private bool scaleAnimation = true;
        [SerializeField] private AnimationCurve scaleCurve = AnimationCurve.EaseInOut(0, 1, 1, 1);
        [SerializeField] private float maxScale = 1.2f;

        [Header("References")]
        [SerializeField] private DrawnMoveStorage moveStorage;
        [SerializeField] private Canvas battleCanvas;

        [Header("Debug")]
        [SerializeField] private bool useDebugSquare = false;

        private void Awake()
        {
            if (moveStorage == null)
            {
                moveStorage = DrawnMoveStorage.Instance;
                if (moveStorage == null)
                {
                    Debug.LogWarning("AttackAnimationManager: DrawnMoveStorage not found!");
                }
            }

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

            // Try to load sprite database from Resources if not assigned
            if (spriteDatabase == null)
            {
                spriteDatabase = Resources.Load<AttackSpriteDatabase>("AttackSpriteDatabase");
                if (spriteDatabase != null)
                {
                    Debug.Log("AttackAnimationManager: Loaded AttackSpriteDatabase from Resources");
                }
            }

            Debug.Log("AttackAnimationManager: Initialized");
        }

        /// <summary>
        /// Play an attack animation. Uses sprite assets if available, otherwise falls back to drawn textures.
        /// </summary>
        public IEnumerator PlayAttackAnimation(Transform source, Transform target, MoveData moveData)
        {
            Debug.Log($"AttackAnimationManager: PlayAttackAnimation called - Move: {moveData.moveName}, Effect: {moveData.visualEffect}");

            if (source == null || target == null)
            {
                Debug.LogError("AttackAnimationManager: Source or target transform is null!");
                yield return new WaitForSeconds(0.5f);
                yield break;
            }

            // Check if we have sprite assets for this move's visual effect
            AttackSpriteDatabase.AttackSpriteEntry spriteEntry = null;
            if (spriteDatabase != null)
            {
                spriteEntry = spriteDatabase.GetEntryForEffect(moveData.visualEffect);
            }

            if (spriteEntry != null)
            {
                // Use sprite-based animation
                Debug.Log($"AttackAnimationManager: Using sprite animation for {moveData.visualEffect}");
                yield return StartCoroutine(PlaySpriteAttackAnimation(source, target, moveData, spriteEntry));
            }
            else
            {
                // Fall back to drawn texture animation
                Debug.Log($"AttackAnimationManager: No sprite entry for {moveData.visualEffect}, using drawn texture fallback");
                yield return StartCoroutine(PlayDrawnTextureAnimation(source, target, moveData));
            }
        }

        /// <summary>
        /// Play attack animation using sprite sheet frames from the database.
        /// Animates a projectile from source to target, then plays an impact animation.
        /// </summary>
        private IEnumerator PlaySpriteAttackAnimation(Transform source, Transform target, MoveData moveData, AttackSpriteDatabase.AttackSpriteEntry entry)
        {
            if (battleCanvas == null)
            {
                Debug.LogError("AttackAnimationManager: No battle canvas for sprite animation!");
                yield break;
            }

            // Create projectile UI element
            GameObject projectile = CreateSpriteUIElement("AttackProjectile_Sprite", source.position, projectileScale * entry.scaleMultiplier);
            if (projectile == null) yield break;

            Image projectileImage = projectile.GetComponent<Image>();
            RectTransform projectileRect = projectile.GetComponent<RectTransform>();

            // Set tint to the move's primary color for visual variety
            projectileImage.color = new Color(moveData.primaryColor.r, moveData.primaryColor.g, moveData.primaryColor.b, 0f);

            // Fade in
            yield return StartCoroutine(FadeUIImage(projectileImage, 0f, 1f, fadeInDuration));

            // Animate projectile flight with sprite sheet frames
            float elapsed = 0f;
            float frameDuration = 1f / entry.framesPerSecond;
            int frameIndex = 0;
            float frameTimer = 0f;
            Vector3 startPos = source.position;
            Vector3 endPos = target.position;
            Vector3 initialScale = projectileRect.localScale;

            Sprite[] frames = entry.projectileFrames;

            // Set first frame
            if (frames.Length > 0)
            {
                projectileImage.sprite = frames[0];
            }

            while (elapsed < projectileDuration)
            {
                float t = elapsed / projectileDuration;

                // Update position
                projectileRect.position = Vector3.Lerp(startPos, endPos, t);

                // Advance sprite frame
                frameTimer += Time.deltaTime;
                if (frameTimer >= frameDuration)
                {
                    frameTimer -= frameDuration;
                    frameIndex = (frameIndex + 1) % frames.Length;
                    projectileImage.sprite = frames[frameIndex];
                }

                // Rotation
                if (entry.rotateWhileFlying)
                {
                    projectileRect.Rotate(Vector3.forward, entry.rotationSpeed * Time.deltaTime);
                }

                // Scale animation
                if (scaleAnimation)
                {
                    float scaleMultiplier = Mathf.Lerp(1f, maxScale, scaleCurve.Evaluate(t));
                    projectileRect.localScale = initialScale * scaleMultiplier;
                }

                elapsed += Time.deltaTime;
                yield return null;
            }

            // Ensure final position
            projectileRect.position = endPos;

            // Fade out projectile
            yield return StartCoroutine(FadeUIImage(projectileImage, 1f, 0f, fadeOutDuration));
            Destroy(projectile);

            // Play impact animation at target if we have impact frames
            Sprite[] impactFrames = entry.impactFrames;
            if (impactFrames == null || impactFrames.Length == 0)
            {
                // Try default impact frames
                if (spriteDatabase != null && spriteDatabase.defaultImpactFrames != null && spriteDatabase.defaultImpactFrames.Length > 0)
                {
                    impactFrames = spriteDatabase.defaultImpactFrames;
                }
            }

            if (impactFrames != null && impactFrames.Length > 0)
            {
                yield return StartCoroutine(PlayImpactAnimation(target.position, impactFrames, entry, moveData));
            }
        }

        /// <summary>
        /// Play an impact animation at a given position using sprite frames.
        /// </summary>
        private IEnumerator PlayImpactAnimation(Vector3 position, Sprite[] frames, AttackSpriteDatabase.AttackSpriteEntry entry, MoveData moveData)
        {
            GameObject impact = CreateSpriteUIElement("ImpactEffect", position, impactScale * entry.scaleMultiplier);
            if (impact == null) yield break;

            Image impactImage = impact.GetComponent<Image>();
            // Tint with secondary color for impact
            impactImage.color = new Color(moveData.secondaryColor.r, moveData.secondaryColor.g, moveData.secondaryColor.b, 1f);

            float frameDuration = 1f / entry.framesPerSecond;
            float elapsed = 0f;
            int frameIndex = 0;
            float frameTimer = 0f;

            impactImage.sprite = frames[0];

            while (elapsed < impactDuration)
            {
                frameTimer += Time.deltaTime;
                if (frameTimer >= frameDuration)
                {
                    frameTimer -= frameDuration;
                    frameIndex++;
                    if (entry.loopImpact)
                    {
                        frameIndex = frameIndex % frames.Length;
                    }
                    else if (frameIndex >= frames.Length)
                    {
                        frameIndex = frames.Length - 1; // Hold last frame
                    }
                    impactImage.sprite = frames[frameIndex];
                }

                // Fade out over the last third of the impact
                float fadeStart = impactDuration * 0.66f;
                if (elapsed > fadeStart)
                {
                    float fadeT = (elapsed - fadeStart) / (impactDuration - fadeStart);
                    Color c = impactImage.color;
                    c.a = Mathf.Lerp(1f, 0f, fadeT);
                    impactImage.color = c;
                }

                elapsed += Time.deltaTime;
                yield return null;
            }

            Destroy(impact);
        }

        /// <summary>
        /// Create a UI element with an Image component for sprite display.
        /// </summary>
        private GameObject CreateSpriteUIElement(string name, Vector3 position, float scale)
        {
            if (battleCanvas == null)
            {
                Debug.LogError("AttackAnimationManager: No battle canvas!");
                return null;
            }

            GameObject obj = new GameObject(name);
            obj.transform.SetParent(battleCanvas.transform, false);

            RectTransform rect = obj.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(scale, scale);
            rect.anchorMin = new Vector2(0, 0);
            rect.anchorMax = new Vector2(0, 0);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.position = position;

            Image image = obj.AddComponent<Image>();
            image.preserveAspect = true;
            image.raycastTarget = false;

            return obj;
        }

        // =====================================================================
        // DRAWN TEXTURE FALLBACK (original behavior)
        // =====================================================================

        /// <summary>
        /// Fallback: Play attack animation using the player's drawn move texture.
        /// Used when no sprite asset is configured for the move's visual effect.
        /// </summary>
        private IEnumerator PlayDrawnTextureAnimation(Transform source, Transform target, MoveData moveData)
        {
            if (moveStorage == null || !moveStorage.HasDrawings())
            {
                Debug.LogWarning("AttackAnimationManager: No move drawings available for animation!");
                yield return PlayFallbackAnimation(source, target, moveData);
                yield break;
            }

            Texture2D moveTexture = moveStorage.GetNextMoveDrawing();
            if (moveTexture == null)
            {
                Debug.LogWarning("AttackAnimationManager: Failed to get move drawing!");
                yield return PlayFallbackAnimation(source, target, moveData);
                yield break;
            }

            Texture2D projectileTexture = moveTexture;
            if (useDebugSquare)
            {
                projectileTexture = CreateDebugSquareTexture();
            }

            GameObject projectile = CreateUIProjectile(projectileTexture, source.position);
            if (projectile == null)
            {
                yield return PlayFallbackAnimation(source, target, moveData);
                yield break;
            }

            yield return StartCoroutine(AnimateUIProjectile(projectile, source, target));

            if (projectile != null)
            {
                Destroy(projectile);
            }
        }

        /// <summary>
        /// Create a projectile GameObject as a UI Image element (drawn texture version)
        /// </summary>
        private GameObject CreateUIProjectile(Texture2D moveTexture, Vector3 spawnPosition)
        {
            try
            {
                if (battleCanvas == null)
                {
                    Debug.LogError("AttackAnimationManager: No battle canvas available!");
                    return null;
                }

                GameObject projectile = new GameObject("AttackProjectile");
                projectile.transform.SetParent(battleCanvas.transform, false);

                RectTransform rectTransform = projectile.AddComponent<RectTransform>();
                rectTransform.sizeDelta = new Vector2(projectileScale, projectileScale);
                rectTransform.anchorMin = new Vector2(0, 0);
                rectTransform.anchorMax = new Vector2(0, 0);
                rectTransform.pivot = new Vector2(0.5f, 0.5f);
                rectTransform.position = spawnPosition;

                Image image = projectile.AddComponent<Image>();

                Sprite moveSprite = Texture2DToSprite(moveTexture);
                if (moveSprite != null)
                {
                    image.sprite = moveSprite;
                    image.preserveAspect = true;
                }
                else
                {
                    Destroy(projectile);
                    return null;
                }

                Color color = image.color;
                color.a = 0f;
                image.color = color;

                return projectile;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"AttackAnimationManager: Exception creating UI projectile: {e.Message}\n{e.StackTrace}");
                return null;
            }
        }

        /// <summary>
        /// Animate the UI projectile from source to target (drawn texture version)
        /// </summary>
        private IEnumerator AnimateUIProjectile(GameObject projectile, Transform source, Transform target)
        {
            RectTransform rectTransform = projectile.GetComponent<RectTransform>();
            Image image = projectile.GetComponent<Image>();

            if (rectTransform == null || image == null) yield break;

            Vector3 startPos = source.position;
            Vector3 endPos = target.position;
            Vector3 initialScale = rectTransform.localScale;

            // Fade in
            yield return StartCoroutine(FadeUIImage(image, 0f, 1f, fadeInDuration));

            // Move from start to end
            float elapsed = 0f;
            while (elapsed < projectileDuration)
            {
                float t = elapsed / projectileDuration;
                rectTransform.position = Vector3.Lerp(startPos, endPos, t);

                if (rotateProjectile)
                {
                    rectTransform.Rotate(Vector3.forward, rotationSpeed * Time.deltaTime);
                }

                if (scaleAnimation)
                {
                    float scaleMultiplier = Mathf.Lerp(1f, maxScale, scaleCurve.Evaluate(t));
                    rectTransform.localScale = initialScale * scaleMultiplier;
                }

                elapsed += Time.deltaTime;
                yield return null;
            }

            rectTransform.position = endPos;

            // Fade out
            yield return StartCoroutine(FadeUIImage(image, 1f, 0f, fadeOutDuration));
        }

        // =====================================================================
        // SHARED UTILITIES
        // =====================================================================

        private IEnumerator FadeUIImage(Image image, float startAlpha, float endAlpha, float duration)
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

        private IEnumerator PlayFallbackAnimation(Transform source, Transform target, MoveData moveData)
        {
            Debug.Log("AttackAnimationManager: Playing fallback animation (no drawing or sprite available)");
            yield return new WaitForSeconds(0.5f);
        }

        private Texture2D CreateDebugSquareTexture()
        {
            int size = 256;
            Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
            Color[] pixels = new Color[size * size];
            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i] = Color.white;
            }
            texture.SetPixels(pixels);
            texture.Apply();
            return texture;
        }

        private Sprite Texture2DToSprite(Texture2D texture)
        {
            if (texture == null) return null;

            try
            {
                return Sprite.Create(
                    texture,
                    new Rect(0, 0, texture.width, texture.height),
                    new Vector2(0.5f, 0.5f),
                    100f
                );
            }
            catch (System.Exception e)
            {
                Debug.LogError($"AttackAnimationManager: Exception creating sprite: {e.Message}");
                return null;
            }
        }

        public void SetAttackPoints(Transform spawnPoint, Transform targetPoint)
        {
            playerAttackSpawnPoint = spawnPoint;
            enemyTargetPoint = targetPoint;
        }
    }
}
