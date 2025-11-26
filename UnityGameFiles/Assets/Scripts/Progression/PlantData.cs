using System;

[Serializable]
public class PlantData
{
    public string id;      // unique ID if needed
    public string name;
    public int hp;
    public int attack;
    public int defense;
    public string type;    // "Fire", "Water", "Grass", etc.

    public PlantData(string name, int hp, int attack, int defense, string type = "Neutral")
    {
        this.id = Guid.NewGuid().ToString();
        this.name = name;
        this.hp = hp;
        this.attack = attack;
        this.defense = defense;
        this.type = type;
    }
}
