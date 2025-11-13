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

        // Fire Moves (Sunflower)
        Fireball,
        FlameWave,
        Burn,

        // Grass Moves (Cactus)
        VineWhip,
        LeafStorm,
        RootAttack,

        // Water Moves (Water Lily)
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

    public MoveData(MoveType type, string name, string desc, ElementType elem, int power, bool heals = false)
    {
        moveType = type;
        moveName = name;
        description = desc;
        element = elem;
        basePower = power;
        isHealingMove = heals;
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
                    new MoveData(MoveType.Fireball, "Fireball", "A blazing ball of fire", ElementType.Fire, 15),
                    new MoveData(MoveType.FlameWave, "Flame Wave", "A spreading wave of flames", ElementType.Fire, 20),
                    new MoveData(MoveType.Burn, "Burn", "Intense burning attack", ElementType.Fire, 25)
                };

            case PlantRecognitionSystem.PlantType.FireRose:
                return new MoveData[]
                {
                    new MoveData(MoveType.Burn, "Ember Petals", "Burning petals rain down", ElementType.Fire, 18),
                    new MoveData(MoveType.Fireball, "Fire Burst", "Explosive fire from blooms", ElementType.Fire, 22),
                    new MoveData(MoveType.FlameWave, "Rose Inferno", "Intense fiery assault", ElementType.Fire, 20)
                };

            case PlantRecognitionSystem.PlantType.FlameTulip:
                return new MoveData[]
                {
                    new MoveData(MoveType.Fireball, "Flame Strike", "Precise fiery beam", ElementType.Fire, 20),
                    new MoveData(MoveType.Burn, "Heat Wave", "Scorching heat blast", ElementType.Fire, 25),
                    new MoveData(MoveType.FlameWave, "Tulip Blaze", "Elegant fire dance", ElementType.Fire, 18)
                };

            // GRASS PLANTS
            case PlantRecognitionSystem.PlantType.Cactus:
                return new MoveData[]
                {
                    new MoveData(MoveType.VineWhip, "Needle Shot", "Sharp cactus needles", ElementType.Grass, 15),
                    new MoveData(MoveType.RootAttack, "Root Strike", "Underground assault", ElementType.Grass, 20),
                    new MoveData(MoveType.LeafStorm, "Spine Storm", "Barrage of spines", ElementType.Grass, 22)
                };

            case PlantRecognitionSystem.PlantType.VineFlower:
                return new MoveData[]
                {
                    new MoveData(MoveType.VineWhip, "Vine Lash", "Whipping vine attack", ElementType.Grass, 18),
                    new MoveData(MoveType.LeafStorm, "Petal Barrage", "Swirling flower petals", ElementType.Grass, 20),
                    new MoveData(MoveType.RootAttack, "Entangling Roots", "Bind and damage", ElementType.Grass, 22)
                };

            case PlantRecognitionSystem.PlantType.GrassSprout:
                return new MoveData[]
                {
                    new MoveData(MoveType.LeafStorm, "Grass Blade", "Cutting grass attack", ElementType.Grass, 15),
                    new MoveData(MoveType.VineWhip, "Sprout Strike", "Quick grass whip", ElementType.Grass, 18),
                    new MoveData(MoveType.RootAttack, "Growth Surge", "Rapid root assault", ElementType.Grass, 20)
                };

            // WATER PLANTS
            case PlantRecognitionSystem.PlantType.WaterLily:
                return new MoveData[]
                {
                    new MoveData(MoveType.WaterSplash, "Lily Splash", "Gentle water spray", ElementType.Water, 15),
                    new MoveData(MoveType.Bubble, "Bubble Shield", "Protective bubbles", ElementType.Water, 18),
                    new MoveData(MoveType.HealingWave, "Healing Lily", "Restore HP with water", ElementType.Water, 20, true)
                };

            case PlantRecognitionSystem.PlantType.CoralBloom:
                return new MoveData[]
                {
                    new MoveData(MoveType.WaterSplash, "Coral Strike", "Sharp coral projectiles", ElementType.Water, 18),
                    new MoveData(MoveType.Bubble, "Sea Burst", "Explosive bubbles", ElementType.Water, 22),
                    new MoveData(MoveType.HealingWave, "Coral Regeneration", "Heal over time", ElementType.Water, 15, true)
                };

            case PlantRecognitionSystem.PlantType.BubbleFlower:
                return new MoveData[]
                {
                    new MoveData(MoveType.Bubble, "Bubble Barrage", "Many small bubbles", ElementType.Water, 20),
                    new MoveData(MoveType.WaterSplash, "Pop Splash", "Bursting bubble attack", ElementType.Water, 18),
                    new MoveData(MoveType.HealingWave, "Bubble Heal", "Healing bubble aura", ElementType.Water, 18, true)
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
