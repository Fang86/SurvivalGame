[System.Serializable]
public class Item
{
    public int Id;
    public string Name;
    public int? AttackDamage;
    public string IconPath; // Path to sprite in Resources folder
    
    public Item(int id)
    {
        Id = id;
    }
}