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
        Tackle,         // Normal type attack - simple straight line

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
                    bool heals = false, bool defensive = false)
    {
        moveType = type;
        moveName = name;
        description = desc;
        element = elem;
        basePower = power;
        isHealingMove = heals;
        isDefensiveMove = defensive;

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
    /// Each plant has 3 moves: Tackle (Normal), Block (Defensive), and a Signature Type Move
    /// </summary>
    public static MoveData[] GetMovesForPlant(PlantRecognitionSystem.PlantType plantType)
    {
        switch (plantType)
        {
            // ═══════════════════════════════════════════════════════════
            // FIRE PLANTS
            // ═══════════════════════════════════════════════════════════

            case PlantRecognitionSystem.PlantType.Sunflower:
                return new MoveData[]
                {
                    CreateTackle(),

                    new MoveData(MoveType.Block, "Block", "Create a protective golden shield",
                        ElementType.Normal, 0,
                        new Color(1f, 0.84f, 0f),      // Gold
                        new Color(1f, 0.65f, 0f),      // Orange
                        VisualEffect.Crystals,
                        0.8f, 0.1f, "Draw 1-3 circular strokes",
                        false, true),

                    new MoveData(MoveType.Burn, "Solar Flare", "Unleash intense burning rays",
                        ElementType.Fire, 25,
                        new Color(1f, 0.2f, 0f),       // Deep orange-red
                        new Color(1f, 1f, 0.3f),       // Bright yellow
                        VisualEffect.Lightning,
                        1.5f, 0.7f, "Draw sharp zigzag patterns")
                };

            case PlantRecognitionSystem.PlantType.FireRose:
                return new MoveData[]
                {
                    CreateTackle(),

                    new MoveData(MoveType.Block, "Block", "Thorny petals form a defensive barrier",
                        ElementType.Normal, 0,
                        new Color(0.8f, 0.2f, 0.3f),   // Deep red
                        new Color(1f, 0.5f, 0.2f),     // Orange-red
                        VisualEffect.Petals,
                        0.8f, 0.1f, "Draw 1-3 circular strokes",
                        false, true),

                    new MoveData(MoveType.Fireball, "Fireball", "Launch a blazing sphere of fire",
                        ElementType.Fire, 25,
                        new Color(1f, 0.4f, 0f),       // Bright orange
                        new Color(1f, 0.8f, 0f),       // Yellow
                        VisualEffect.Flames,
                        1.2f, 0.5f, "Draw a perfect circle")
                };

            case PlantRecognitionSystem.PlantType.FlameTulip:
                return new MoveData[]
                {
                    CreateTackle(),

                    new MoveData(MoveType.Block, "Block", "Tulip petals close into a protective shell",
                        ElementType.Normal, 0,
                        new Color(1f, 0.3f, 0.4f),     // Rose
                        new Color(1f, 0.6f, 0.2f),     // Coral
                        VisualEffect.Petals,
                        0.8f, 0.1f, "Draw 1-3 circular strokes",
                        false, true),

                    new MoveData(MoveType.FlameWave, "Flame Burst", "An explosive burst of searing flames",
                        ElementType.Fire, 25,
                        new Color(1f, 0.25f, 0f),      // Pure flame orange
                        new Color(1f, 0.5f, 0.1f),     // Light orange
                        VisualEffect.Flames,
                        1.4f, 0.6f, "Draw horizontal wavy lines")
                };

            // ═══════════════════════════════════════════════════════════
            // GRASS PLANTS
            // ═══════════════════════════════════════════════════════════

            case PlantRecognitionSystem.PlantType.Cactus:
                return new MoveData[]
                {
                    CreateTackle(),

                    new MoveData(MoveType.Block, "Block", "Harden into a spiny defensive posture",
                        ElementType.Normal, 0,
                        new Color(0.3f, 0.6f, 0.2f),   // Desert green
                        new Color(0.5f, 0.4f, 0.2f),   // Sandy brown
                        VisualEffect.Crystals,
                        0.8f, 0.1f, "Draw 1-3 circular strokes",
                        false, true),

                    new MoveData(MoveType.VineWhip, "Needle Shot", "Fire sharp cactus needles at enemies",
                        ElementType.Grass, 25,
                        new Color(0.4f, 0.7f, 0.3f),   // Bright green
                        new Color(0.8f, 0.8f, 0.6f),   // Tan (needle color)
                        VisualEffect.Crystals,
                        1.0f, 0.4f, "Draw a single curved line")
                };

            case PlantRecognitionSystem.PlantType.VineFlower:
                return new MoveData[]
                {
                    CreateTackle(),

                    new MoveData(MoveType.Block, "Block", "Vines coil into a protective shield",
                        ElementType.Normal, 0,
                        new Color(0.2f, 0.7f, 0.3f),   // Vibrant green
                        new Color(0.4f, 0.5f, 0.2f),   // Olive
                        VisualEffect.Vines,
                        0.8f, 0.1f, "Draw 1-3 circular strokes",
                        false, true),

                    new MoveData(MoveType.RootAttack, "Vine Lash", "A powerful whipping vine strikes with force",
                        ElementType.Grass, 25,
                        new Color(0.25f, 0.75f, 0.3f), // Fresh green
                        new Color(0.15f, 0.5f, 0.2f),  // Dark green
                        VisualEffect.Vines,
                        1.1f, 0.5f, "Draw vertical downward strokes")
                };

            case PlantRecognitionSystem.PlantType.GrassSprout:
                return new MoveData[]
                {
                    CreateTackle(),

                    new MoveData(MoveType.Block, "Block", "Young sprouts form a protective wall",
                        ElementType.Normal, 0,
                        new Color(0.4f, 0.9f, 0.4f),   // Light green
                        new Color(0.6f, 0.8f, 0.3f),   // Yellow-green
                        VisualEffect.Leaves,
                        0.8f, 0.1f, "Draw 1-3 circular strokes",
                        false, true),

                    new MoveData(MoveType.LeafStorm, "Razor Leaf", "Sharp grass blades slice through the air",
                        ElementType.Grass, 25,
                        new Color(0.5f, 0.95f, 0.4f),  // Bright grass
                        new Color(0.3f, 0.7f, 0.3f),   // Medium green
                        VisualEffect.Leaves,
                        1.0f, 0.4f, "Draw 5+ quick strokes")
                };

            // ═══════════════════════════════════════════════════════════
            // WATER PLANTS
            // ═══════════════════════════════════════════════════════════

            case PlantRecognitionSystem.PlantType.WaterLily:
                return new MoveData[]
                {
                    CreateTackle(),

                    new MoveData(MoveType.Block, "Block", "Float on a cushion of protective water",
                        ElementType.Normal, 0,
                        new Color(0.4f, 0.7f, 0.9f),   // Sky blue
                        new Color(0.6f, 0.9f, 0.7f),   // Aqua
                        VisualEffect.Water,
                        0.8f, 0.1f, "Draw 1-3 circular strokes",
                        false, true),

                    new MoveData(MoveType.WaterSplash, "Tidal Wave", "A powerful wave crashes down on the enemy",
                        ElementType.Water, 25,
                        new Color(0.3f, 0.6f, 0.95f),  // Clear blue
                        new Color(0.5f, 0.85f, 0.9f),  // Light cyan
                        VisualEffect.Water,
                        1.2f, 0.5f, "Draw smooth wavy curves")
                };

            case PlantRecognitionSystem.PlantType.CoralBloom:
                return new MoveData[]
                {
                    CreateTackle(),

                    new MoveData(MoveType.Block, "Block", "Coral hardens into a defensive formation",
                        ElementType.Normal, 0,
                        new Color(0.9f, 0.5f, 0.6f),   // Coral pink
                        new Color(0.3f, 0.5f, 0.8f),   // Ocean blue
                        VisualEffect.Crystals,
                        0.8f, 0.1f, "Draw 1-3 circular strokes",
                        false, true),

                    new MoveData(MoveType.Bubble, "Coral Spike", "Sharp coral projectiles pierce enemies",
                        ElementType.Water, 25,
                        new Color(1f, 0.4f, 0.5f),     // Pink coral
                        new Color(0.2f, 0.5f, 0.9f),   // Deep blue
                        VisualEffect.Crystals,
                        1.1f, 0.5f, "Draw multiple circles")
                };

            case PlantRecognitionSystem.PlantType.BubbleFlower:
                return new MoveData[]
                {
                    CreateTackle(),

                    new MoveData(MoveType.Block, "Block", "Surround yourself with protective bubbles",
                        ElementType.Normal, 0,
                        new Color(0.6f, 0.8f, 1f),     // Light blue
                        new Color(0.9f, 0.95f, 1f),    // Almost white
                        VisualEffect.Bubbles,
                        0.8f, 0.1f, "Draw 1-3 circular strokes",
                        false, true),

                    new MoveData(MoveType.HealingWave, "Bubble Barrage", "Countless bubbles bombard the target",
                        ElementType.Water, 25,
                        new Color(0.5f, 0.75f, 0.95f), // Medium blue
                        new Color(0.85f, 0.92f, 1f),   // Pale blue
                        VisualEffect.Bubbles,
                        1.2f, 0.5f, "Draw smooth flowing waves")
                };

            default:
                return new MoveData[0];
        }
    }

    /// <summary>
    /// Create the universal Tackle move (Normal type, 1x multiplier, no type advantage)
    /// </summary>
    private static MoveData CreateTackle()
    {
        return new MoveData(MoveType.Tackle, "Tackle", "A basic physical charge attack",
            ElementType.Normal, 15,
            new Color(0.7f, 0.7f, 0.7f),   // Gray
            new Color(0.9f, 0.9f, 0.9f),   // Light gray
            VisualEffect.None,
            0.8f, 0.2f, "Draw a straight line");
    }

    /// <summary>
    /// Calculate type advantage multiplier
    /// Water > Fire > Grass > Water
    /// Normal type always deals neutral (1.0x) damage
    /// </summary>
    public static float GetTypeAdvantage(ElementType attackType, ElementType defenseType)
    {
        // Normal type attacks always deal neutral damage
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
