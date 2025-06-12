using UnityEngine;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine.InputSystem;

public class InventoryUI : MonoBehaviour
{
    [SerializeField] private GameObject inventoryPanel;
    [SerializeField] private GameObject itemSlotPrefab;
    [SerializeField] private Transform gridParent;
    [SerializeField] private Inventory playerInventory;
    public Tooltip tooltip;
    
    private List<GameObject> itemSlots = new List<GameObject>();

    void Start()
    {
        // Subscribe to inventory changes
        playerInventory.OnInventoryChanged += RefreshUI;

        // Create initial slots
        CreateSlots();
        RefreshUI();
    }

    void Update()
    {
        if (Keyboard.current.bKey.wasPressedThisFrame)
            ToggleInventory();
    }

    void ToggleInventory()
    {
        if (inventoryPanel.activeSelf)
            HideInventory();
        else
            ShowInventory();
    }

    void ShowInventory()
    {
        Cursor.lockState = CursorLockMode.Confined;
        inventoryPanel.SetActive(true);
        playerInventory.SetOpen(true);
    }

    void HideInventory()
    {
        Cursor.lockState = CursorLockMode.Locked;
        playerInventory.SetOpen(false);
        inventoryPanel.SetActive(false);
        tooltip.gameObject.SetActive(false);
    }
    
    void CreateSlots()
    {
        for (int i = 0; i < playerInventory.MaxSlots; i++)
        {
            GameObject slot = Instantiate(itemSlotPrefab, gridParent);
            itemSlots.Add(slot);
        }
    }
    
    void RefreshUI()
    {
        var items = playerInventory.GetItems();
        
        for (int i = 0; i < itemSlots.Count; i++)
        {
            ItemSlot slotScript = itemSlots[i].GetComponent<ItemSlot>();
            
            if (i < items.Count)
            {
                slotScript.SetItem(items[i]);
            }
            else
            {
                slotScript.ClearSlot();
            }
        }
    }
    
    void OnDestroy()
    {
        HideInventory();

        if (playerInventory != null)
            playerInventory.OnInventoryChanged -= RefreshUI;
    }
}