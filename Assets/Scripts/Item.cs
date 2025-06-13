[System.Serializable]
public class Item
{
    public int Id;
    public string Name;
    public int? AttackDamage;
    public string IconPath; // Path to sprite in Resources folder
    public bool canPickUp = true;

    public Item(int id)
    {
        Id = id;
    }
    
    // TODO: This doesnt work because not monobehavior
    public bool PickupItem()
    {
        if (canPickUp)
        {
            canPickUp = false;
            return true;
        }

        return false;
    }
}