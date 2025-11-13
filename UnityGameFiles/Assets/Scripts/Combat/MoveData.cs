using UnityEngine;

/// <summary>
/// Defines a battle move with its properties and type
/// </summary>
[System.Serializable]
public class MoveData
{
    public enum MoveType
    {
        Unknown,

        // Universal Moves
        Block,          // Defensive move - easy to draw and recognize

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
        Fire,
        Grass,
        Water
    }

    public MoveType moveType;
    public string moveName;
    public string description;
    public ElementType element;
    public int basePower;
    public bool isHealingMove;
    public bool isDefensiveMove;

    public MoveData(MoveType type, string name, string desc, ElementType elem, int power, bool heals = false, bool defensive = false)
    {
        moveType = type;
        moveName = name;
        description = desc;
        element = elem;
        basePower = power;
        isHealingMove = heals;
        isDefensiveMove = defensive;

        // Auto-detect defensive moves
        if (type == MoveType.Block)
        {
            isDefensiveMove = true;
        }
    }

    /// <summary>
    /// Get all available moves for a specific plant type
    /// </summary>
    public static MoveData[] GetMovesForPlant(PlantRecognitionSystem.PlantType plantType)
    {
        switch (plantType)
        {
            // FIRE PLANTS
            case PlantRecognitionSystem.PlantType.Sunflower:
                return new MoveData[]
                {
                    new MoveData(MoveType.Block, "Block", "Defensive stance to reduce damage", ElementType.Fire, 0),
                    new MoveData(MoveType.Fireball, "Fireball", "A blazing ball of fire", ElementType.Fire, 20),
                    new MoveData(MoveType.Burn, "Burn", "Intense burning attack", ElementType.Fire, 28)
                };

            case PlantRecognitionSystem.PlantType.FireRose:
                return new MoveData[]
                {
                    new MoveData(MoveType.Block, "Block", "Defensive stance to reduce damage", ElementType.Fire, 0),
                    new MoveData(MoveType.Burn, "Ember Petals", "Burning petals rain down", ElementType.Fire, 22),
                    new MoveData(MoveType.Fireball, "Fire Burst", "Explosive fire from blooms", ElementType.Fire, 26)
                };

            case PlantRecognitionSystem.PlantType.FlameTulip:
                return new MoveData[]
                {
                    new MoveData(MoveType.Block, "Block", "Defensive stance to reduce damage", ElementType.Fire, 0),
                    new MoveData(MoveType.Fireball, "Flame Strike", "Precise fiery beam", ElementType.Fire, 24),
                    new MoveData(MoveType.Burn, "Heat Wave", "Scorching heat blast", ElementType.Fire, 30)
                };

            // GRASS PLANTS
            case PlantRecognitionSystem.PlantType.Cactus:
                return new MoveData[]
                {
                    new MoveData(MoveType.Block, "Block", "Defensive stance to reduce damage", ElementType.Grass, 0),
                    new MoveData(MoveType.VineWhip, "Needle Shot", "Sharp cactus needles", ElementType.Grass, 20),
                    new MoveData(MoveType.LeafStorm, "Spine Storm", "Barrage of spines", ElementType.Grass, 26)
                };

            case PlantRecognitionSystem.PlantType.VineFlower:
                return new MoveData[]
                {
                    new MoveData(MoveType.Block, "Block", "Defensive stance to reduce damage", ElementType.Grass, 0),
                    new MoveData(MoveType.VineWhip, "Vine Lash", "Whipping vine attack", ElementType.Grass, 22),
                    new MoveData(MoveType.RootAttack, "Entangling Roots", "Bind and damage", ElementType.Grass, 26)
                };

            case PlantRecognitionSystem.PlantType.GrassSprout:
                return new MoveData[]
                {
                    new MoveData(MoveType.Block, "Block", "Defensive stance to reduce damage", ElementType.Grass, 0),
                    new MoveData(MoveType.LeafStorm, "Grass Blade", "Cutting grass attack", ElementType.Grass, 20),
                    new MoveData(MoveType.RootAttack, "Growth Surge", "Rapid root assault", ElementType.Grass, 24)
                };

            // WATER PLANTS
            case PlantRecognitionSystem.PlantType.WaterLily:
                return new MoveData[]
                {
                    new MoveData(MoveType.Block, "Block", "Defensive stance to reduce damage", ElementType.Water, 0),
                    new MoveData(MoveType.WaterSplash, "Lily Splash", "Gentle water spray", ElementType.Water, 20),
                    new MoveData(MoveType.HealingWave, "Healing Lily", "Restore HP with water", ElementType.Water, 25, true)
                };

            case PlantRecognitionSystem.PlantType.CoralBloom:
                return new MoveData[]
                {
                    new MoveData(MoveType.Block, "Block", "Defensive stance to reduce damage", ElementType.Water, 0),
                    new MoveData(MoveType.WaterSplash, "Coral Strike", "Sharp coral projectiles", ElementType.Water, 22),
                    new MoveData(MoveType.Bubble, "Sea Burst", "Explosive bubbles", ElementType.Water, 26)
                };

            case PlantRecognitionSystem.PlantType.BubbleFlower:
                return new MoveData[]
                {
                    new MoveData(MoveType.Block, "Block", "Defensive stance to reduce damage", ElementType.Water, 0),
                    new MoveData(MoveType.Bubble, "Bubble Barrage", "Many small bubbles", ElementType.Water, 24),
                    new MoveData(MoveType.HealingWave, "Bubble Heal", "Healing bubble aura", ElementType.Water, 22, true)
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
