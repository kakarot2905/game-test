using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BossHealthBarUI : MonoBehaviour
{
    [Header("UI References")]
    public Image fillBar;
    public TextMeshProUGUI nameText;
    public GameObject panel; // The visual parent (to hide/show)

    [Header("Settings")]
    public float fillSpeed = 5f;
    public Color healthyColor = new Color(0.8f, 0.2f, 0.2f); // Red
    public Color criticalColor = new Color(0.5f, 0.0f, 0.0f); // Dark Red

    private BossController boss;
    private float targetFill = 1f;
    private float displayFill = 1f;

    void Start()
    {
        // Hide initially if no boss assigned or fight hasn't started
        if (panel != null) panel.SetActive(false);
    }

    public void SetBoss(BossController newBoss)
    {
        boss = newBoss;
        
        if (boss != null)
        {
            if (nameText != null) nameText.text = boss.bossName;
            
            // Initial update
            UpdateTargetFill();
            displayFill = targetFill;
            if (fillBar != null) fillBar.fillAmount = displayFill;
        }
    }

    public void Show()
    {
        if (panel != null) panel.SetActive(true);
    }

    public void Hide()
    {
        if (panel != null) panel.SetActive(false);
    }

    void Update()
    {
        if (boss == null) return;

        UpdateTargetFill();

        // Smooth fill
        displayFill = Mathf.Lerp(displayFill, targetFill, fillSpeed * Time.deltaTime);

        if (fillBar != null)
        {
            fillBar.fillAmount = displayFill;
            
            // Optional: Color shift based on HP
            fillBar.color = Color.Lerp(criticalColor, healthyColor, displayFill);
        }
    }

    void UpdateTargetFill()
    {
        if (boss.maxHP > 0)
            targetFill = (float)boss.CurrentHP / boss.maxHP;
        else
            targetFill = 0;
    }
}
