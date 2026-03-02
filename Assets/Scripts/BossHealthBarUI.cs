using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BossHealthBarUI : MonoBehaviour
{
    [Header("UI References")]
    public Image fillBar;
    public TextMeshProUGUI nameText;
    // public GameObject panel; // REMOVED: Using CanvasGroup prevents disabling the script!
    
    private CanvasGroup canvasGroup;

    [Header("Settings")]
    public float fillSpeed = 5f;
    public Color healthyColor = new Color(0.8f, 0.2f, 0.2f); // Red
    public Color criticalColor = new Color(0.5f, 0.0f, 0.0f); // Dark Red

    private BossController boss;
    private float targetFill = 1f;
    private float displayFill = 1f;

    void Awake()
    {
        // 1. Setup CanvasGroup for visibility control
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        // 2. Auto-find "Bar" child (The fill image)
        if (fillBar == null)
        {
            Transform barTrans = transform.Find("Bar");
            if (barTrans != null)
            {
                fillBar = barTrans.GetComponent<Image>();
                Debug.Log($"[BossHealthBarUI] Found Fill Bar: {fillBar.name}");
            }
            else
            {
                // Fallback to first image in children that isn't the background (self)
                Image[] images = GetComponentsInChildren<Image>();
                foreach (var img in images)
                {
                    if (img.gameObject != gameObject)
                    {
                        fillBar = img;
                        break;
                    }
                }
            }
        }

        // 3. Force Image Settings
        if (fillBar != null)
        {
            if (fillBar.type != Image.Type.Filled)
            {
                fillBar.type = Image.Type.Filled;
                fillBar.fillMethod = Image.FillMethod.Horizontal;
                Debug.Log("[BossHealthBarUI] Forced Image Type to FILLED");
            }
        }
        else
        {
            Debug.LogError("[BossHealthBarUI] Could not find Child 'Bar' Image!");
        }
    }

    void Start()
    {
        // Hide initially
        Hide();
        
        // Safety check
        if (fillBar == null) Debug.LogError("Boss Health Bar: FILL BAR IMAGE IS MISSING!");
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
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
            canvasGroup.blocksRaycasts = true; // Optional
        }
        else
        {
            // Fallback if something went wrong
            transform.localScale = Vector3.one; 
        }
    }

    public void Hide()
    {
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.blocksRaycasts = false;
        }
        else
        {
            // Fallback
            transform.localScale = Vector3.zero;
        }
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
