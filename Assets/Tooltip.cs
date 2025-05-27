using UnityEngine;
using UnityEngine.UI;

public class Tooltip : MonoBehaviour
{
    [SerializeField] private Text itemNameText;
    [SerializeField] private Text itemDescriptionText;
    [SerializeField] private Text itemStatsText;
    
    private static Tooltip instance;
    
    void Awake()
    {
        instance = this;
        Hide();
    }
    
    public static void Show(Item item, Vector3 position)
    {
        if (instance == null) return;
        
        instance.itemNameText.text = item.Name;
        instance.itemStatsText.text = item.AttackDamage.HasValue ? 
            $"Attack Damage: {item.AttackDamage}" : "";

        RectTransform rectTransform = instance.GetComponent<RectTransform>();
        instance.transform.position = position + new Vector3(rectTransform.rect.width / 2, -rectTransform.rect.height / 2, 0f);
        instance.gameObject.SetActive(true);
    }
    
    public static void Hide()
    {
        if (instance != null)
            instance.gameObject.SetActive(false);
    }
}