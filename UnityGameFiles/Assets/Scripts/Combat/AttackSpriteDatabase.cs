using UnityEngine;

namespace SketchBlossom.Battle
{
    /// <summary>
    /// ScriptableObject that maps VisualEffect types and MoveTypes to sprite sheet animations.
    /// Create via Assets > Create > SketchBlossom > Attack Sprite Database.
    /// Assign sprite sheets (individual frames) for each attack effect.
    /// </summary>
    [CreateAssetMenu(fileName = "AttackSpriteDatabase", menuName = "SketchBlossom/Attack Sprite Database")]
    public class AttackSpriteDatabase : ScriptableObject
    {
        [System.Serializable]
        public class AttackSpriteEntry
        {
            [Tooltip("The visual effect type this animation is for")]
            public MoveData.VisualEffect visualEffect;

            [Tooltip("Sprite frames for the projectile animation (played in order)")]
            public Sprite[] projectileFrames;

            [Tooltip("Sprite frames for the impact animation on the target")]
            public Sprite[] impactFrames;

            [Tooltip("Frames per second for this animation")]
            public float framesPerSecond = 12f;

            [Tooltip("Scale multiplier for this effect (1 = default projectileScale)")]
            public float scaleMultiplier = 1f;

            [Tooltip("Should the projectile rotate while flying?")]
            public bool rotateWhileFlying = false;

            [Tooltip("Rotation speed in degrees/second (if rotating)")]
            public float rotationSpeed = 180f;

            [Tooltip("Should the impact animation loop until duration ends?")]
            public bool loopImpact = false;
        }

        [Header("Effect Animations")]
        [Tooltip("Sprite animations for each VisualEffect type")]
        public AttackSpriteEntry[] effectEntries;

        [Header("Fallback")]
        [Tooltip("Default impact frames used when no specific effect entry is found")]
        public Sprite[] defaultImpactFrames;

        [Tooltip("Default projectile frames used when no specific effect entry is found")]
        public Sprite[] defaultProjectileFrames;

        /// <summary>
        /// Find the sprite entry for a given VisualEffect type.
        /// Returns null if no entry is configured for this effect.
        /// </summary>
        public AttackSpriteEntry GetEntryForEffect(MoveData.VisualEffect effect)
        {
            if (effectEntries == null) return null;

            for (int i = 0; i < effectEntries.Length; i++)
            {
                if (effectEntries[i].visualEffect == effect)
                {
                    // Only return if it actually has frames assigned
                    if (effectEntries[i].projectileFrames != null && effectEntries[i].projectileFrames.Length > 0)
                    {
                        return effectEntries[i];
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Check if this database has any sprites configured at all
        /// </summary>
        public bool HasAnySprites()
        {
            if (effectEntries == null) return false;

            for (int i = 0; i < effectEntries.Length; i++)
            {
                if (effectEntries[i].projectileFrames != null && effectEntries[i].projectileFrames.Length > 0)
                    return true;
            }

            return (defaultProjectileFrames != null && defaultProjectileFrames.Length > 0);
        }
    }
}
