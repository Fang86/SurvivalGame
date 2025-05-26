using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    [Header("Health Bar Settings")]
    public float maxHealth = 100f;
    public float currentHealth = 100f;
    public Vector3 offset = new Vector3(0, 2f, 0); // Offset above entity
    public Vector2 healthBarSize = new Vector2(1.35f, 0.15f);
    
    [Header("Colors")]
    public Color healthColor = Color.green;
    public Color lowHealthColor = Color.green;
    public Color backgroundColor = Color.red;
    
    [Header("Behavior")]
    public bool alwaysVisible = true;
    public float hideWhenFullDelay = 2f; // Hide after 2 seconds when full health
    public bool faceCamera = true;
    
    // UI Components
    private Canvas healthCanvas;
    private Image healthBarFill;
    private Image healthBarBackground;
    private GameObject healthBarUI;
    public Camera playerCamera;
    
    // Internal
    private float hideTimer = 0f;
    private bool shouldHide = false;

    void Start()
    {
        CreateHealthBar();
        if (playerCamera == null)
            playerCamera = Camera.main;
        UpdateHealthBar();
    }

    void Update()
    {
        if (healthBarUI == null) return;
        
        // Face camera if enabled
        if (faceCamera && playerCamera != null)
        {
            healthCanvas.transform.LookAt(healthCanvas.transform.position + playerCamera.transform.rotation * Vector3.forward,
                                         playerCamera.transform.rotation * Vector3.up);
        }
        
        // Handle hiding when at full health
        if (!alwaysVisible)
        {
            if (currentHealth >= maxHealth && !shouldHide)
            {
                hideTimer += Time.deltaTime;
                if (hideTimer >= hideWhenFullDelay)
                {
                    shouldHide = true;
                }
            }
            else if (currentHealth < maxHealth)
            {
                shouldHide = false;
                hideTimer = 0f;
            }
            
            healthBarUI.SetActive(!shouldHide);
        }
    }

    void CreateHealthBar()
    {
        // Create Canvas
        GameObject canvasObj = new GameObject("HealthBarCanvas");
        canvasObj.transform.SetParent(transform);
        canvasObj.transform.localPosition = offset;
        
        healthCanvas = canvasObj.AddComponent<Canvas>();
        healthCanvas.renderMode = RenderMode.WorldSpace;
        healthCanvas.sortingOrder = 10; // Render above other UI
        
        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.dynamicPixelsPerUnit = 100;
        
        // Set canvas size
        RectTransform canvasRect = healthCanvas.GetComponent<RectTransform>();
        canvasRect.sizeDelta = healthBarSize;
        
        // Create health bar background
        GameObject backgroundObj = new GameObject("HealthBarBackground");
        backgroundObj.transform.SetParent(canvasObj.transform);
        backgroundObj.transform.localPosition = Vector3.zero;
        
        healthBarBackground = backgroundObj.AddComponent<Image>();
        healthBarBackground.color = backgroundColor;
        
        RectTransform bgRect = backgroundObj.GetComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.offsetMin = Vector2.zero;
        bgRect.offsetMax = Vector2.zero;
        
        // Create health bar fill
        GameObject fillObj = new GameObject("HealthBarFill");
        fillObj.transform.SetParent(backgroundObj.transform);
        
        healthBarFill = fillObj.AddComponent<Image>();
        healthBarFill.color = healthColor;
        healthBarFill.type = Image.Type.Filled;
        healthBarFill.fillMethod = Image.FillMethod.Horizontal;
        
        RectTransform fillRect = fillObj.GetComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = Vector2.one;
        fillRect.offsetMin = Vector2.zero;
        fillRect.offsetMax = Vector2.zero;
        
        healthBarUI = canvasObj;
    }

    public void TakeDamage(float damage)
    {
        currentHealth = Mathf.Max(0, currentHealth - damage);
        UpdateHealthBar();
        ShowHealthBar(); // Show when taking damage
    }

    public void Heal(float healAmount)
    {
        currentHealth = Mathf.Min(maxHealth, currentHealth + healAmount);
        UpdateHealthBar();
        ShowHealthBar(); // Show when healing
    }

    public void SetHealth(float newHealth)
    {
        currentHealth = Mathf.Clamp(newHealth, 0, maxHealth);
        UpdateHealthBar();
    }

    public void SetMaxHealth(float newMaxHealth)
    {
        maxHealth = newMaxHealth;
        currentHealth = Mathf.Min(currentHealth, maxHealth);
        UpdateHealthBar();
    }

    void UpdateHealthBar()
    {
        if (healthBarFill == null) return;
        
        float healthPercentage = currentHealth / maxHealth;
        healthBarFill.fillAmount = healthPercentage;
        
        // Change color based on health percentage
        Color currentColor = Color.Lerp(lowHealthColor, healthColor, healthPercentage);
        healthBarFill.color = currentColor;
    }

    void ShowHealthBar()
    {
        shouldHide = false;
        hideTimer = 0f;
        if (healthBarUI != null)
        {
            healthBarUI.SetActive(true);
        }
    }

    // Public methods for external scripts
    public float GetHealthPercentage()
    {
        return currentHealth / maxHealth;
    }

    public bool IsDead()
    {
        return currentHealth <= 0;
    }

    public void SetVisibility(bool visible)
    {
        alwaysVisible = visible;
        if (healthBarUI != null)
        {
            healthBarUI.SetActive(visible);
        }
    }

    // Optional: Damage interface implementation
    public void OnDamageTaken(float damage)
    {
        TakeDamage(damage);
    }

    void OnDrawGizmosSelected()
    {
        // Show health bar position in editor
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(transform.position + offset, new Vector3(healthBarSize.x, healthBarSize.y, 0.1f));
    }
}