using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Health bar UI with main bar + heart icon for bonus HP.
/// Main bar = base 100 HP (depletes FIRST)
/// Heart icon = bonus HP from MAX HP upgrade (depletes LAST, after bar is empty)
/// </summary>
public class HealthBarUI : MonoBehaviour
{
    [Header("UI Elements")]
    public Image fillBar;                    // Main health bar (depletes first)
    public Image heartIcon;                  // Heart icon - bonus HP (depletes last)
    public TextMeshProUGUI healthText;       // Optional: "80/120"
    
    [Header("Player")]
    public PlayerController player;
    
    [Header("Base HP")]
    public int baseMaxHP = 100;              // Base HP before any upgrades
    
    [Header("Animation")]
    public float fillSpeed = 5f;
    public float heartUnlockFillSpeed = 1.5f;  // Slower dramatic fill for unlock
    
    [Header("Colors")]
    public Color healthyColor = new Color(0.2f, 0.8f, 0.3f, 1f);    // Green
    public Color damagedColor = new Color(1f, 0.8f, 0.2f, 1f);      // Yellow
    public Color criticalColor = new Color(0.9f, 0.2f, 0.2f, 1f);   // Red
    
    [Header("Skill Tree Reference")]
    public GameObject skillTreePanel;        // Assign skill tree panel to delay animation
    
    private float targetBarFill = 1f;
    private float displayBarFill = 1f;
    private float targetHeartFill = 0f;
    private float displayHeartFill = 0f;
    
    private bool heartWasUnlocked = false;    // Track if heart was previously unlocked
    private bool playingUnlockAnimation = false;
    private bool pendingUnlockAnimation = false;  // Wait for skill tree to close
    
    private Color? overrideColor = null; // For special states like Overpower Mode
    
    void Start()
    {
        if (player == null)
            player = FindObjectOfType<PlayerController>();
        
        // Check if already unlocked on start
        if (player != null && player.maxHP > baseMaxHP)
            heartWasUnlocked = true;
        
        UpdateDisplay();
    }
    
    void Update()
    {
        if (player == null) return;
        
        // Check if pending animation should start (skill tree closed)
        if (pendingUnlockAnimation && !IsSkillTreeOpen())
        {
            pendingUnlockAnimation = false;
            playingUnlockAnimation = true;
            displayHeartFill = 0f;
            Debug.Log("[HealthBarUI] Skill tree closed - playing heart fill animation!");
        }
        
        UpdateDisplay();
        
        // Smooth fill animation for bar (use unscaledDeltaTime so it works when paused)
        displayBarFill = Mathf.Lerp(displayBarFill, targetBarFill, fillSpeed * Time.unscaledDeltaTime);
        
        // Heart uses slower speed during unlock animation
        float heartSpeed = playingUnlockAnimation ? heartUnlockFillSpeed : fillSpeed;
        displayHeartFill = Mathf.Lerp(displayHeartFill, targetHeartFill, heartSpeed * Time.unscaledDeltaTime);
        
        // Check if unlock animation is done
        if (playingUnlockAnimation && Mathf.Abs(displayHeartFill - targetHeartFill) < 0.01f)
        {
            playingUnlockAnimation = false;
            displayHeartFill = targetHeartFill;
        }
        
        if (fillBar != null)
        {
            fillBar.fillAmount = displayBarFill;
            
            // Color based on health
            if (overrideColor.HasValue)
            {
                fillBar.color = overrideColor.Value;
            }
            else if (targetBarFill > 0.5f)
                fillBar.color = healthyColor;
            else if (targetBarFill > 0.25f)
                fillBar.color = damagedColor;
            else
                fillBar.color = criticalColor;
        }
        
        if (heartIcon != null)
            heartIcon.fillAmount = displayHeartFill;
    }
    
    bool IsSkillTreeOpen()
    {
        return skillTreePanel != null && skillTreePanel.activeInHierarchy;
    }
    
    void UpdateDisplay()
    {
        int currentHP = player.GetHP();
        int maxHP = player.maxHP;
        int bonusMaxHP = maxHP - baseMaxHP;  // Extra HP from upgrade (e.g., 20)
        
        // Check for first-time unlock
        if (bonusMaxHP > 0 && !heartWasUnlocked)
        {
            heartWasUnlocked = true;
            
            // If skill tree is open, wait for it to close
            if (IsSkillTreeOpen())
            {
                pendingUnlockAnimation = true;
                Debug.Log("[HealthBarUI] Heart unlocked! Waiting for skill tree to close...");
            }
            else
            {
                playingUnlockAnimation = true;
                displayHeartFill = 0f;
                Debug.Log("[HealthBarUI] Heart unlocked! Playing fill animation...");
            }
        }
        
        // DAMAGE ORDER: Bar depletes FIRST, Heart depletes LAST
        // BUT: Keep heart at 0 while waiting for skill tree to close
        if (pendingUnlockAnimation)
        {
            targetHeartFill = 0f;  // Stay empty until animation starts
        }
        else if (bonusMaxHP > 0)
        {
            // Have bonus HP from upgrade
            if (currentHP > bonusMaxHP)
            {
                // Bar is taking damage, heart is full
                targetHeartFill = 1f;
                targetBarFill = (float)(currentHP - bonusMaxHP) / baseMaxHP;
            }
            else
            {
                // Bar is empty, heart is draining
                targetBarFill = 0f;
                targetHeartFill = (float)currentHP / bonusMaxHP;
            }
        }
        else
        {
            // No bonus HP - just bar
            targetHeartFill = 0f;
            targetBarFill = (float)currentHP / baseMaxHP;
        }
        
        // Update text
        if (healthText != null)
            healthText.text = $"{currentHP}/{maxHP}";
    }
    
    /// <summary>
    /// Called when player takes damage - can trigger effects.
    /// </summary>
    public void PlayDamageEffect()
    {
        // Optional: Add shake or flash effect here
        // If reusing the old Shake script logic:
        // GetComponent<Animator>()?.SetTrigger("Shake");
    }

    public void SetOverrideColor(Color c)
    {
        overrideColor = c;
    }

    public void ClearOverrideColor()
    {
        overrideColor = null;
    }
}
