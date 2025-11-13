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
    public static MoveData[] GetMovesForPlant(PlantAnalyzer.PlantType plantType)
    {
        switch (plantType)
        {
            case PlantAnalyzer.PlantType.Sunflower:
                return new MoveData[]
                {
                    new MoveData(MoveType.Fireball, "Fireball", "A blazing ball of fire", ElementType.Fire, 15),
                    new MoveData(MoveType.FlameWave, "Flame Wave", "A spreading wave of flames", ElementType.Fire, 20),
                    new MoveData(MoveType.Burn, "Burn", "Intense burning attack", ElementType.Fire, 25)
                };

            case PlantAnalyzer.PlantType.Cactus:
                return new MoveData[]
                {
                    new MoveData(MoveType.VineWhip, "Vine Whip", "A whipping vine attack", ElementType.Grass, 15),
                    new MoveData(MoveType.LeafStorm, "Leaf Storm", "A barrage of sharp leaves", ElementType.Grass, 20),
                    new MoveData(MoveType.RootAttack, "Root Attack", "Underground root assault", ElementType.Grass, 25)
                };

            case PlantAnalyzer.PlantType.WaterLily:
                return new MoveData[]
                {
                    new MoveData(MoveType.WaterSplash, "Water Splash", "A splash of water", ElementType.Water, 15),
                    new MoveData(MoveType.Bubble, "Bubble", "Bubble projectiles", ElementType.Water, 20),
                    new MoveData(MoveType.HealingWave, "Healing Wave", "Restore HP with water energy", ElementType.Water, 15, true)
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
