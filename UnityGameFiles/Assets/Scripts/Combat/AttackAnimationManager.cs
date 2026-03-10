using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace SketchBlossom.Battle
{
    /// <summary>
    /// Manages attack animations using sprite sheet assets mapped to each attack's VisualEffect.
    /// Sprite frames are configured in the AttackSpriteDatabase ScriptableObject.
    /// </summary>
    public class AttackAnimationManager : MonoBehaviour
    {
        [Header("Sprite Database")]
        [Tooltip("Assign the AttackSpriteDatabase ScriptableObject (or place it in Resources/)")]
        [SerializeField] private AttackSpriteDatabase spriteDatabase;

        [Header("Projectile Settings")]
        [SerializeField] private float projectileDuration = 0.8f;
        [SerializeField] private float projectileScale = 300f;

        [Header("Animation Settings")]
        [SerializeField] private float fadeInDuration = 0.1f;
        [SerializeField] private float fadeOutDuration = 0.1f;

        [Header("Impact Settings")]
        [SerializeField] private float impactDuration = 0.4f;
        [SerializeField] private float impactScale = 250f;

        [Header("Effect Settings")]
        [SerializeField] private bool scaleAnimation = true;
        [SerializeField] private AnimationCurve scaleCurve = AnimationCurve.EaseInOut(0, 1, 1, 1);
        [SerializeField] private float maxScale = 1.2f;

        [Header("References")]
        [SerializeField] private Canvas battleCanvas;

        // Cached attack points
        private Transform playerAttackSpawnPoint;
        private Transform enemyTargetPoint;

        private void Awake()
        {
            if (battleCanvas == null)
            {
                battleCanvas = FindFirstObjectByType<Canvas>();
                if (battleCanvas != null)
                {
                    Debug.Log($"AttackAnimationManager: Found canvas '{battleCanvas.name}'");
                }
                else
                {
                    Debug.LogError("AttackAnimationManager: No Canvas found in scene!");
                }
            }

            if (spriteDatabase == null)
            {
                spriteDatabase = Resources.Load<AttackSpriteDatabase>("AttackSpriteDatabase");
                if (spriteDatabase != null)
                {
                    Debug.Log("AttackAnimationManager: Loaded AttackSpriteDatabase from Resources");
                }
                else
                {
                    Debug.LogWarning("AttackAnimationManager: No AttackSpriteDatabase found! Assign one in the inspector or place in Resources/.");
                }
            }

            Debug.Log("AttackAnimationManager: Initialized");
        }

        /// <summary>
        /// Play an attack animation for the given move.
        /// Uses effect-specific sprites if available, otherwise falls back to default sprites.
        /// </summary>
        public IEnumerator PlayAttackAnimation(Transform source, Transform target, MoveData moveData)
        {
            Debug.Log($"AttackAnimationManager: Playing animation for '{moveData.moveName}' (Effect: {moveData.visualEffect})");

            if (source == null || target == null)
            {
                Debug.LogError("AttackAnimationManager: Source or target transform is null!");
                yield break;
            }

            if (battleCanvas == null)
            {
                Debug.LogError("AttackAnimationManager: No battle canvas!");
                yield break;
            }

            // Try to find effect-specific sprites first
            AttackSpriteDatabase.AttackSpriteEntry spriteEntry = null;
            Sprite[] projectileFrames = null;
            Sprite[] impactFrames = null;
            float fps = 12f;
            float entryScaleMultiplier = 1f;
            bool rotateWhileFlying = false;
            float rotationSpeed = 180f;
            bool loopImpact = false;

            if (spriteDatabase != null)
            {
                spriteEntry = spriteDatabase.GetEntryForEffect(moveData.visualEffect);

                if (spriteEntry != null)
                {
                    // Use effect-specific sprites
                    projectileFrames = spriteEntry.projectileFrames;
                    impactFrames = spriteEntry.impactFrames;
                    fps = spriteEntry.framesPerSecond;
                    entryScaleMultiplier = spriteEntry.scaleMultiplier;
                    rotateWhileFlying = spriteEntry.rotateWhileFlying;
                    rotationSpeed = spriteEntry.rotationSpeed;
                    loopImpact = spriteEntry.loopImpact;
                    Debug.Log($"AttackAnimationManager: Using effect-specific sprites for {moveData.visualEffect} ({projectileFrames.Length} frames)");
                }
                else
                {
                    // Fall back to default sprites from the database
                    if (spriteDatabase.defaultProjectileFrames != null && spriteDatabase.defaultProjectileFrames.Length > 0)
                    {
                        projectileFrames = spriteDatabase.defaultProjectileFrames;
                        Debug.Log($"AttackAnimationManager: Using default projectile sprites ({projectileFrames.Length} frames)");
                    }

                    if (spriteDatabase.defaultImpactFrames != null && spriteDatabase.defaultImpactFrames.Length > 0)
                    {
                        impactFrames = spriteDatabase.defaultImpactFrames;
                    }
                }
            }

            // If we have no sprites at all, just wait briefly
            if (projectileFrames == null || projectileFrames.Length == 0)
            {
                Debug.LogWarning($"AttackAnimationManager: No sprites available for {moveData.visualEffect} and no defaults configured. Skipping animation.");
                yield return new WaitForSeconds(0.3f);
                yield break;
            }

            // Play projectile animation
            yield return StartCoroutine(PlayProjectileAnimation(
                source.position, target.position, moveData,
                projectileFrames, fps, entryScaleMultiplier, rotateWhileFlying, rotationSpeed
            ));

            // Play impact animation at target
            if (impactFrames != null && impactFrames.Length > 0)
            {
                yield return StartCoroutine(PlayImpactAnimation(
                    target.position, moveData, impactFrames, fps, entryScaleMultiplier, loopImpact
                ));
            }
        }

        /// <summary>
        /// Animate a sprite-based projectile flying from source to target.
        /// </summary>
        private IEnumerator PlayProjectileAnimation(
            Vector3 startPos, Vector3 endPos, MoveData moveData,
            Sprite[] frames, float fps, float scaleMultiplier,
            bool rotate, float rotSpeed)
        {
            GameObject projectile = CreateSpriteUIElement("AttackProjectile", startPos, projectileScale * scaleMultiplier);
            if (projectile == null) yield break;

            Image image = projectile.GetComponent<Image>();
            RectTransform rect = projectile.GetComponent<RectTransform>();

            // Start with move's primary color, fully transparent
            image.color = new Color(moveData.primaryColor.r, moveData.primaryColor.g, moveData.primaryColor.b, 0f);
            image.sprite = frames[0];

            // Fade in
            yield return StartCoroutine(FadeImage(image, 0f, 1f, fadeInDuration));

            // Fly from source to target while cycling frames
            float elapsed = 0f;
            float frameDuration = 1f / Mathf.Max(fps, 1f);
            int frameIndex = 0;
            float frameTimer = 0f;
            Vector3 initialScale = rect.localScale;

            while (elapsed < projectileDuration)
            {
                float t = elapsed / projectileDuration;

                rect.position = Vector3.Lerp(startPos, endPos, t);

                // Advance frame
                frameTimer += Time.deltaTime;
                if (frameTimer >= frameDuration)
                {
                    frameTimer -= frameDuration;
                    frameIndex = (frameIndex + 1) % frames.Length;
                    image.sprite = frames[frameIndex];
                }

                if (rotate)
                {
                    rect.Rotate(Vector3.forward, rotSpeed * Time.deltaTime);
                }

                if (scaleAnimation)
                {
                    float s = Mathf.Lerp(1f, maxScale, scaleCurve.Evaluate(t));
                    rect.localScale = initialScale * s;
                }

                elapsed += Time.deltaTime;
                yield return null;
            }

            rect.position = endPos;

            // Fade out
            yield return StartCoroutine(FadeImage(image, 1f, 0f, fadeOutDuration));
            Destroy(projectile);
        }

        /// <summary>
        /// Play an impact animation at the target position.
        /// </summary>
        private IEnumerator PlayImpactAnimation(
            Vector3 position, MoveData moveData,
            Sprite[] frames, float fps, float scaleMultiplier, bool loop)
        {
            GameObject impact = CreateSpriteUIElement("ImpactEffect", position, impactScale * scaleMultiplier);
            if (impact == null) yield break;

            Image image = impact.GetComponent<Image>();
            image.color = new Color(moveData.secondaryColor.r, moveData.secondaryColor.g, moveData.secondaryColor.b, 1f);
            image.sprite = frames[0];

            float frameDuration = 1f / Mathf.Max(fps, 1f);
            float elapsed = 0f;
            int frameIndex = 0;
            float frameTimer = 0f;

            while (elapsed < impactDuration)
            {
                frameTimer += Time.deltaTime;
                if (frameTimer >= frameDuration)
                {
                    frameTimer -= frameDuration;
                    frameIndex++;
                    if (loop)
                    {
                        frameIndex = frameIndex % frames.Length;
                    }
                    else if (frameIndex >= frames.Length)
                    {
                        frameIndex = frames.Length - 1;
                    }
                    image.sprite = frames[frameIndex];
                }

                // Fade out over the last third
                float fadeStart = impactDuration * 0.66f;
                if (elapsed > fadeStart)
                {
                    float fadeT = (elapsed - fadeStart) / (impactDuration - fadeStart);
                    Color c = image.color;
                    c.a = Mathf.Lerp(1f, 0f, fadeT);
                    image.color = c;
                }

                elapsed += Time.deltaTime;
                yield return null;
            }

            Destroy(impact);
        }

        /// <summary>
        /// Create a UI element for displaying a sprite animation.
        /// </summary>
        private GameObject CreateSpriteUIElement(string name, Vector3 position, float scale)
        {
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

        private IEnumerator FadeImage(Image image, float startAlpha, float endAlpha, float duration)
        {
            if (image == null) yield break;

            float elapsed = 0f;
            Color color = image.color;

            while (elapsed < duration)
            {
                color.a = Mathf.Lerp(startAlpha, endAlpha, elapsed / duration);
                image.color = color;
                elapsed += Time.deltaTime;
                yield return null;
            }

            color.a = endAlpha;
            image.color = color;
        }

        public void SetAttackPoints(Transform spawnPoint, Transform targetPoint)
        {
            playerAttackSpawnPoint = spawnPoint;
            enemyTargetPoint = targetPoint;
        }
    }
}
