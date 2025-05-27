using UnityEngine;
using System.Collections.Generic;

public class Inventory : MonoBehaviour
{
    [SerializeField] private int maxSlots = 20;
    [SerializeField] private List<Item> items = new List<Item>();

    private bool isOpen;

    // Events to notify UI when inventory changes
    public System.Action OnInventoryChanged;

    public List<Item> GetItems() => items;
    public int MaxSlots => maxSlots;

    public bool AddItem(Item item)
    {
        if (items.Count >= maxSlots) return false;

        items.Add(item);
        OnInventoryChanged?.Invoke();
        return true;
    }

    public bool RemoveItem(Item item)
    {
        bool removed = items.Remove(item);
        if (removed) OnInventoryChanged?.Invoke();
        return removed;
    }

    void Start()
    {
        // Add some test items
        AddItem(new Item(1) { Name = "Sword", AttackDamage = 50 });
        AddItem(new Item(2) { Name = "Shield" });
        AddItem(new Item(3) { Name = "Potion" });
    }

    public bool IsOpen()
    {
        return isOpen;
    }

    public void SetOpen(bool open)
    {
        Debug.Log($"Setting open: {open}");
        isOpen = open;
    }
}