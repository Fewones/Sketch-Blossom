using UnityEngine;

/// <summary>
/// Defines a battle move with its properties, visual effects, and type
/// Robust move system with colors, particles, and unique characteristics
/// </summary>
[System.Serializable]
public class MoveData
{
    public enum MoveType
    {
        Unknown,

        // Universal Moves
        Block,          // Defensive move - easy to draw and recognize

        // Normal Moves (no type advantage)
        Sting,          // Fire plant basic attack
        Cut,            // Grass plant basic attack
        Bump,           // Water plant basic attack

        // Fire Moves
        Fireball,
        FlameWave,
        Burn,

        // Grass Moves
        VineWhip,
        LeafStorm,
        RootAttack,

        // Water Moves
        WaterSplash,
        Bubble,
        HealingWave
    }

    public enum ElementType
    {
        Normal,
        Fire,
        Grass,
        Water
    }

    public enum VisualEffect
    {
        None,
        Sparks,
        Smoke,
        Flames,
        Embers,
        Leaves,
        Vines,
        Petals,
        Roots,
        Water,
        Bubbles,
        Steam,
        Crystals,
        Lightning
    }

    // Core Properties
    public MoveType moveType;
    public string moveName;
    public string description;
    public ElementType element;
    public int basePower;
    public bool isHealingMove;
    public bool isDefensiveMove;

    // Cooldown Properties
    public int cooldownTurns;         // Turns of cooldown after use (0 = no cooldown)

    // Visual Properties (Robust Enhancement)
    public Color primaryColor;
    public Color secondaryColor;
    public VisualEffect visualEffect;
    public float animationIntensity;  // 0.5 = subtle, 1.0 = normal, 2.0 = dramatic
    public float screenShakeAmount;   // 0 = none, 0.5 = light, 1.0 = heavy
    public string drawingHint;        // Guide text for how to draw this move

    public MoveData(MoveType type, string name, string desc, ElementType elem, int power,
                    Color primary, Color secondary, VisualEffect effect,
                    float intensity = 1.0f, float shake = 0.3f, string hint = "",
                    bool heals = false, bool defensive = false, int cooldown = 0)
    {
        moveType = type;
        moveName = name;
        description = desc;
        element = elem;
        basePower = power;
        isHealingMove = heals;
        isDefensiveMove = defensive;
        cooldownTurns = cooldown;

        // Visual properties
        primaryColor = primary;
        secondaryColor = secondary;
        visualEffect = effect;
        animationIntensity = intensity;
        screenShakeAmount = shake;
        drawingHint = hint;

        // Auto-detect defensive moves
        if (type == MoveType.Block)
        {
            isDefensiveMove = true;
        }
    }

    /// <summary>
    /// Get all available moves for a specific plant type
    /// Enhanced with unique colors and visual effects
    /// </summary>
    public static MoveData[] GetMovesForPlant(PlantRecognitionSystem.PlantType plantType)
    {
        switch (plantType)
        {
            // ═══════════════════════════════════════════════════════════
            // FIRE PLANTS - Aggressive, high damage, warm colors
            // ═══════════════════════════════════════════════════════════

            case PlantRecognitionSystem.PlantType.Sunflower:
                return new MoveData[]
                {
                    new MoveData(MoveType.Block, "Block", "Create a protective golden shield",
                        ElementType.Fire, 0,
                        new Color(1f, 0.84f, 0f),      // Gold
                        new Color(1f, 0.65f, 0f),      // Orange
                        VisualEffect.Crystals,
                        0.8f, 0.1f, "Draw 1-3 circular strokes",
                        false, true),

                    new MoveData(MoveType.Sting, "Sting", "A quick stinging jab of solar energy",
                        ElementType.Normal, 10,
                        new Color(1f, 0.9f, 0.6f),     // Warm white
                        new Color(1f, 0.7f, 0.3f),     // Soft orange
                        VisualEffect.Sparks,
                        0.7f, 0.2f, "Draw a single straight line"),

                    new MoveData(MoveType.Fireball, "Fireball", "Launch a blazing sphere of solar fire",
                        ElementType.Fire, 15,
                        new Color(1f, 0.4f, 0f),       // Bright orange
                        new Color(1f, 0.8f, 0f),       // Yellow
                        VisualEffect.Flames,
                        1.0f, 0.4f, "Draw a perfect circle"),

                    new MoveData(MoveType.Burn, "Solar Flare", "Unleash intense burning rays",
                        ElementType.Fire, 25,
                        new Color(1f, 0.2f, 0f),       // Deep orange-red
                        new Color(1f, 1f, 0.3f),       // Bright yellow
                        VisualEffect.Lightning,
                        1.5f, 0.7f, "Draw sharp zigzag patterns",
                        false, false, 1)
                };

            case PlantRecognitionSystem.PlantType.FireRose:
                return new MoveData[]
                {
                    new MoveData(MoveType.Block, "Block", "Thorny petals form a defensive barrier",
                        ElementType.Fire, 0,
                        new Color(0.8f, 0.2f, 0.3f),   // Deep red
                        new Color(1f, 0.5f, 0.2f),     // Orange-red
                        VisualEffect.Petals,
                        0.8f, 0.1f, "Draw 1-3 circular strokes",
                        false, true),

                    new MoveData(MoveType.Sting, "Sting", "A sharp thorn jabs the enemy",
                        ElementType.Normal, 10,
                        new Color(0.9f, 0.7f, 0.7f),   // Light rose
                        new Color(0.7f, 0.3f, 0.3f),   // Muted red
                        VisualEffect.Sparks,
                        0.7f, 0.2f, "Draw a single straight line"),

                    new MoveData(MoveType.Burn, "Ember Petals", "Burning rose petals rain down on foes",
                        ElementType.Fire, 15,
                        new Color(1f, 0.1f, 0.2f),     // Crimson
                        new Color(1f, 0.4f, 0f),       // Orange
                        VisualEffect.Petals,
                        1.2f, 0.5f, "Draw scattered jagged lines"),

                    new MoveData(MoveType.Fireball, "Passion Burst", "Explosive fire erupts from blooming roses",
                        ElementType.Fire, 25,
                        new Color(1f, 0f, 0.3f),       // Hot pink-red
                        new Color(1f, 0.3f, 0f),       // Red-orange
                        VisualEffect.Flames,
                        1.4f, 0.6f, "Draw a large circle with flair",
                        false, false, 1)
                };

            case PlantRecognitionSystem.PlantType.FlameTulip:
                return new MoveData[]
                {
                    new MoveData(MoveType.Block, "Block", "Tulip petals close into a protective shell",
                        ElementType.Fire, 0,
                        new Color(1f, 0.3f, 0.4f),     // Rose
                        new Color(1f, 0.6f, 0.2f),     // Coral
                        VisualEffect.Petals,
                        0.8f, 0.1f, "Draw 1-3 circular strokes",
                        false, true),

                    new MoveData(MoveType.Sting, "Sting", "A swift fiery poke singes the target",
                        ElementType.Normal, 10,
                        new Color(1f, 0.8f, 0.6f),     // Peach
                        new Color(1f, 0.5f, 0.3f),     // Soft coral
                        VisualEffect.Sparks,
                        0.7f, 0.2f, "Draw a single straight line"),

                    new MoveData(MoveType.Fireball, "Flame Strike", "A precise beam of concentrated fire",
                        ElementType.Fire, 15,
                        new Color(1f, 0.25f, 0f),      // Pure flame orange
                        new Color(1f, 0.5f, 0.1f),     // Light orange
                        VisualEffect.Flames,
                        1.1f, 0.5f, "Draw a clean circle"),

                    new MoveData(MoveType.Burn, "Inferno Wave", "A devastating wave of scorching heat",
                        ElementType.Fire, 25,
                        new Color(1f, 0.15f, 0f),      // Deep flame
                        new Color(1f, 0.7f, 0f),       // Bright fire
                        VisualEffect.Smoke,
                        1.8f, 0.9f, "Draw aggressive zigzags",
                        false, false, 1)
                };

            // ═══════════════════════════════════════════════════════════
            // GRASS PLANTS - Balanced, natural, earth tones
            // ═══════════════════════════════════════════════════════════

            case PlantRecognitionSystem.PlantType.Cactus:
                return new MoveData[]
                {
                    new MoveData(MoveType.Block, "Block", "Harden into a spiny defensive posture",
                        ElementType.Grass, 0,
                        new Color(0.3f, 0.6f, 0.2f),   // Desert green
                        new Color(0.5f, 0.4f, 0.2f),   // Sandy brown
                        VisualEffect.Crystals,
                        0.8f, 0.1f, "Draw 1-3 circular strokes",
                        false, true),

                    new MoveData(MoveType.Cut, "Cut", "A quick slash with a sharp spine",
                        ElementType.Normal, 10,
                        new Color(0.6f, 0.7f, 0.5f),   // Pale sage
                        new Color(0.4f, 0.5f, 0.3f),   // Muted green
                        VisualEffect.Sparks,
                        0.7f, 0.2f, "Draw a single straight line"),

                    new MoveData(MoveType.VineWhip, "Needle Shot", "Fire sharp cactus needles at enemies",
                        ElementType.Grass, 15,
                        new Color(0.4f, 0.7f, 0.3f),   // Bright green
                        new Color(0.8f, 0.8f, 0.6f),   // Tan (needle color)
                        VisualEffect.Crystals,
                        1.0f, 0.4f, "Draw a single curved line"),

                    new MoveData(MoveType.LeafStorm, "Spine Storm", "A relentless barrage of sharp spines",
                        ElementType.Grass, 25,
                        new Color(0.35f, 0.65f, 0.25f),// Dark green
                        new Color(0.9f, 0.85f, 0.5f),  // Pale yellow
                        VisualEffect.Crystals,
                        1.4f, 0.6f, "Draw 5+ scattered strokes",
                        false, false, 1)
                };

            case PlantRecognitionSystem.PlantType.VineFlower:
                return new MoveData[]
                {
                    new MoveData(MoveType.Block, "Block", "Vines coil into a protective shield",
                        ElementType.Grass, 0,
                        new Color(0.2f, 0.7f, 0.3f),   // Vibrant green
                        new Color(0.4f, 0.5f, 0.2f),   // Olive
                        VisualEffect.Vines,
                        0.8f, 0.1f, "Draw 1-3 circular strokes",
                        false, true),

                    new MoveData(MoveType.Cut, "Cut", "A swift vine slices through the air",
                        ElementType.Normal, 10,
                        new Color(0.5f, 0.8f, 0.5f),   // Light green
                        new Color(0.3f, 0.6f, 0.3f),   // Medium green
                        VisualEffect.Sparks,
                        0.7f, 0.2f, "Draw a single straight line"),

                    new MoveData(MoveType.VineWhip, "Vine Lash", "A powerful whipping vine strikes with force",
                        ElementType.Grass, 15,
                        new Color(0.25f, 0.75f, 0.3f), // Fresh green
                        new Color(0.15f, 0.5f, 0.2f),  // Dark green
                        VisualEffect.Vines,
                        1.1f, 0.5f, "Draw a long curved line"),

                    new MoveData(MoveType.RootAttack, "Strangling Roots", "Massive roots bind and crush the enemy",
                        ElementType.Grass, 25,
                        new Color(0.3f, 0.5f, 0.2f),   // Forest green
                        new Color(0.4f, 0.3f, 0.2f),   // Brown
                        VisualEffect.Roots,
                        1.3f, 0.7f, "Draw vertical downward strokes",
                        false, false, 1)
                };

            case PlantRecognitionSystem.PlantType.GrassSprout:
                return new MoveData[]
                {
                    new MoveData(MoveType.Block, "Block", "Young sprouts form a protective wall",
                        ElementType.Grass, 0,
                        new Color(0.4f, 0.9f, 0.4f),   // Light green
                        new Color(0.6f, 0.8f, 0.3f),   // Yellow-green
                        VisualEffect.Leaves,
                        0.8f, 0.1f, "Draw 1-3 circular strokes",
                        false, true),

                    new MoveData(MoveType.Cut, "Cut", "A sharp leaf blade slashes the foe",
                        ElementType.Normal, 10,
                        new Color(0.7f, 0.9f, 0.5f),   // Yellow-green
                        new Color(0.5f, 0.7f, 0.3f),   // Olive green
                        VisualEffect.Sparks,
                        0.7f, 0.2f, "Draw a single straight line"),

                    new MoveData(MoveType.LeafStorm, "Razor Leaf", "Sharp grass blades slice through the air",
                        ElementType.Grass, 15,
                        new Color(0.5f, 0.95f, 0.4f),  // Bright grass
                        new Color(0.3f, 0.7f, 0.3f),   // Medium green
                        VisualEffect.Leaves,
                        1.0f, 0.4f, "Draw 5+ quick strokes"),

                    new MoveData(MoveType.RootAttack, "Growth Surge", "Rapid growing roots assault the target",
                        ElementType.Grass, 25,
                        new Color(0.45f, 0.85f, 0.35f),// Grass green
                        new Color(0.5f, 0.4f, 0.25f),  // Earth brown
                        VisualEffect.Roots,
                        1.2f, 0.5f, "Draw tall vertical lines",
                        false, false, 1)
                };

            // ═══════════════════════════════════════════════════════════
            // WATER PLANTS - Healing/support, cool colors, fluid
            // ═══════════════════════════════════════════════════════════

            case PlantRecognitionSystem.PlantType.WaterLily:
                return new MoveData[]
                {
                    new MoveData(MoveType.Block, "Block", "Float on a cushion of protective water",
                        ElementType.Water, 0,
                        new Color(0.4f, 0.7f, 0.9f),   // Sky blue
                        new Color(0.6f, 0.9f, 0.7f),   // Aqua
                        VisualEffect.Water,
                        0.8f, 0.1f, "Draw 1-3 circular strokes",
                        false, true),

                    new MoveData(MoveType.Bump, "Bump", "A forceful watery shove",
                        ElementType.Normal, 10,
                        new Color(0.6f, 0.8f, 0.9f),   // Pale blue
                        new Color(0.7f, 0.85f, 0.8f),  // Light teal
                        VisualEffect.Sparks,
                        0.7f, 0.2f, "Draw a single straight line"),

                    new MoveData(MoveType.WaterSplash, "Lily Splash", "Gentle waves wash over the enemy",
                        ElementType.Water, 15,
                        new Color(0.3f, 0.6f, 0.95f),  // Clear blue
                        new Color(0.5f, 0.85f, 0.9f),  // Light cyan
                        VisualEffect.Water,
                        0.9f, 0.3f, "Draw smooth wavy curves"),

                    new MoveData(MoveType.HealingWave, "Tranquil Petals", "Soothing lily petals restore health",
                        ElementType.Water, 20,
                        new Color(0.5f, 0.9f, 0.95f),  // Pale cyan
                        new Color(0.7f, 0.95f, 0.7f),  // Mint
                        VisualEffect.Petals,
                        1.0f, 0.1f, "Draw gentle horizontal waves",
                        true, false, 1)
                };

            case PlantRecognitionSystem.PlantType.CoralBloom:
                return new MoveData[]
                {
                    new MoveData(MoveType.Block, "Block", "Coral hardens into a defensive formation",
                        ElementType.Water, 0,
                        new Color(0.9f, 0.5f, 0.6f),   // Coral pink
                        new Color(0.3f, 0.5f, 0.8f),   // Ocean blue
                        VisualEffect.Crystals,
                        0.8f, 0.1f, "Draw 1-3 circular strokes",
                        false, true),

                    new MoveData(MoveType.Bump, "Bump", "A solid coral headbutt",
                        ElementType.Normal, 10,
                        new Color(0.9f, 0.7f, 0.75f),  // Light coral
                        new Color(0.6f, 0.5f, 0.7f),   // Muted purple
                        VisualEffect.Sparks,
                        0.7f, 0.2f, "Draw a single straight line"),

                    new MoveData(MoveType.WaterSplash, "Coral Spike", "Sharp coral projectiles pierce enemies",
                        ElementType.Water, 15,
                        new Color(1f, 0.4f, 0.5f),     // Pink coral
                        new Color(0.2f, 0.5f, 0.9f),   // Deep blue
                        VisualEffect.Crystals,
                        1.1f, 0.5f, "Draw curved flowing lines"),

                    new MoveData(MoveType.Bubble, "Tidal Burst", "Explosive pressurized water bubbles",
                        ElementType.Water, 25,
                        new Color(0.2f, 0.6f, 1f),     // Vivid blue
                        new Color(0.8f, 0.95f, 1f),    // White foam
                        VisualEffect.Bubbles,
                        1.4f, 0.6f, "Draw multiple circles",
                        false, false, 1)
                };

            case PlantRecognitionSystem.PlantType.BubbleFlower:
                return new MoveData[]
                {
                    new MoveData(MoveType.Block, "Block", "Surround yourself with protective bubbles",
                        ElementType.Water, 0,
                        new Color(0.6f, 0.8f, 1f),     // Light blue
                        new Color(0.9f, 0.95f, 1f),    // Almost white
                        VisualEffect.Bubbles,
                        0.8f, 0.1f, "Draw 1-3 circular strokes",
                        false, true),

                    new MoveData(MoveType.Bump, "Bump", "A bubbly body slam",
                        ElementType.Normal, 10,
                        new Color(0.7f, 0.85f, 1f),    // Soft blue
                        new Color(0.85f, 0.9f, 0.95f), // Pale silver
                        VisualEffect.Sparks,
                        0.7f, 0.2f, "Draw a single straight line"),

                    new MoveData(MoveType.Bubble, "Bubble Barrage", "Countless bubbles bombard the target",
                        ElementType.Water, 15,
                        new Color(0.5f, 0.75f, 0.95f), // Medium blue
                        new Color(0.85f, 0.92f, 1f),   // Pale blue
                        VisualEffect.Bubbles,
                        1.2f, 0.5f, "Draw many small circles"),

                    new MoveData(MoveType.HealingWave, "Bubble Remedy", "Healing bubbles restore vitality",
                        ElementType.Water, 20,
                        new Color(0.4f, 0.85f, 0.9f),  // Turquoise
                        new Color(0.7f, 1f, 0.8f),     // Mint green
                        VisualEffect.Bubbles,
                        1.0f, 0.1f, "Draw smooth flowing waves",
                        true, false, 1)
                };

            default:
                return new MoveData[0];
        }
    }

    /// <summary>
    /// Calculate type advantage multiplier
    /// Water > Fire > Grass > Water
    /// </summary>
    public static float GetTypeAdvantage(ElementType attackType, ElementType defenseType)
    {
        // Normal type is always neutral - no advantage or disadvantage
        if (attackType == ElementType.Normal) return 1.0f;

        if (attackType == ElementType.Water && defenseType == ElementType.Fire) return 1.5f;
        if (attackType == ElementType.Fire && defenseType == ElementType.Grass) return 1.5f;
        if (attackType == ElementType.Grass && defenseType == ElementType.Water) return 1.5f;

        if (attackType == ElementType.Fire && defenseType == ElementType.Water) return 0.5f;
        if (attackType == ElementType.Grass && defenseType == ElementType.Fire) return 0.5f;
        if (attackType == ElementType.Water && defenseType == ElementType.Grass) return 0.5f;

        return 1.0f; // Neutral
    }

    public override string ToString()
    {
        return $"{moveName} ({element}) - Power: {basePower}";
    }
}
