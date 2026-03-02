using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// UI bar showing XP progress toward next skill point.
/// Subscribes to ExperienceManager events.
/// </summary>
public class XPBarUI : MonoBehaviour
{
    [Header("UI Elements")]
    public Image fillBar;                    // Assign a UI Image with Image Type = Filled
    public TextMeshProUGUI xpText;           // Optional: "15/20 XP"
    public TextMeshProUGUI levelUpText;      // Optional: shows "+1 Skill Point!" briefly
    
    [Header("Animation")]
    public float fillSpeed = 5f;             // How fast bar fills (lerp speed)
    public float levelUpDisplayTime = 1.5f;
    
    private float targetFill = 0f;
    private float displayFill = 0f;
    
    void Start()
    {
        // Subscribe to XP events
        if (ExperienceManager.Instance != null)
        {
            ExperienceManager.Instance.OnXPChanged += OnXPChanged;
            ExperienceManager.Instance.OnLevelUp += OnLevelUp;
            
            // Initialize with current values
            UpdateDisplay(ExperienceManager.Instance.currentXP, ExperienceManager.Instance.xpPerLevel);
        }
        else
        {
            // Fallback if Manager isn't ready yet or missing
            UpdateDisplay(0, 20);
        }
        
        if (levelUpText != null)
            levelUpText.gameObject.SetActive(false);
    }
    
    void OnDestroy()
    {
        // Unsubscribe
        if (ExperienceManager.Instance != null)
        {
            ExperienceManager.Instance.OnXPChanged -= OnXPChanged;
            ExperienceManager.Instance.OnLevelUp -= OnLevelUp;
        }
    }
    
    void Update()
    {
        // Smooth fill animation
        displayFill = Mathf.Lerp(displayFill, targetFill, fillSpeed * Time.deltaTime);
        
        if (fillBar != null)
            fillBar.fillAmount = displayFill;
    }
    
    void OnXPChanged(int currentXP, int xpPerLevel)
    {
        UpdateDisplay(currentXP, xpPerLevel);
    }
    
    void UpdateDisplay(int currentXP, int xpPerLevel)
    {
        targetFill = (float)currentXP / xpPerLevel;
        
        if (xpText != null)
            xpText.text = $"{currentXP}/{xpPerLevel}";
    }
    
    void OnLevelUp(int skillPoints)
    {
        // Flash level up message
        if (levelUpText != null)
        {
            levelUpText.text = $"+{skillPoints} Skill Point!";
            levelUpText.gameObject.SetActive(true);
            CancelInvoke(nameof(HideLevelUpText));
            Invoke(nameof(HideLevelUpText), levelUpDisplayTime);
        }
        
        // Reset fill to show new progress from 0
        displayFill = 0f;
    }
    
    void HideLevelUpText()
    {
        if (levelUpText != null)
            levelUpText.gameObject.SetActive(false);
    }
}
