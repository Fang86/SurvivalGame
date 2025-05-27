using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ItemSlot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private Image itemIcon;
    [SerializeField] private Text itemName;
    [SerializeField] private Text itemStats;
    [SerializeField] private GameObject emptySlotIndicator;

    private Item currentItem;
    private Tooltip tooltipPrefab;

    public void SetItem(Item item)
    {
        Debug.Log($"Setting item to {item.Name}");
        currentItem = item;

        // Show item info
        itemName.text = item.Name;

        if (item.AttackDamage.HasValue)
            itemStats.text = $"DMG: {item.AttackDamage}";
        else
            itemStats.text = "";

        // Load icon (you'll need to add sprites to Resources/ItemIcons/)
        if (!string.IsNullOrEmpty(item.IconPath))
        {
            Sprite sprite = Resources.Load<Sprite>($"ItemIcons/{item.IconPath}");
            if (sprite != null) itemIcon.sprite = sprite;
        }

        // Hide empty slot indicator
        emptySlotIndicator.SetActive(false);
    }

    public void ClearSlot()
    {
        currentItem = null;
        itemName.text = "";
        itemStats.text = "";
        itemIcon.sprite = null;
        emptySlotIndicator.SetActive(true);
    }

    // For clicking on items later
    public void OnSlotClicked()
    {
        if (currentItem != null)
        {
            Debug.Log($"Clicked on {currentItem.Name}");
        }
    }
    
    public void OnPointerEnter(PointerEventData eventData)
    {
        Debug.Log("Pointer enter");
        if (currentItem != null)
        {
            // Get this slot's RectTransform
            RectTransform slotRect = GetComponent<RectTransform>();
            
            // Get the bottom-right corner in world/screen coordinates
            Vector3[] corners = new Vector3[4];
            slotRect.GetWorldCorners(corners);
            
            // corners[0] = bottom-left
            // corners[1] = top-left  
            // corners[2] = top-right
            // corners[3] = bottom-right
            Vector3 bottomRightCorner = corners[3];
            
            // Add small offset so tooltip doesn't overlap the slot
            Vector3 tooltipPosition = bottomRightCorner + new Vector3(5, -5, 0);
            
            Tooltip.Show(currentItem, tooltipPosition);
        }
    }
    
    public void OnPointerExit(PointerEventData eventData)
    {
        Debug.Log("Pointer exit");
        Tooltip.Hide();
    }
}