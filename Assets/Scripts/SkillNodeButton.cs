using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Handles skill node button clicks - connects to SkillTreeManager
/// Attach this to each diamond node (done automatically by builder)
/// </summary>
public class SkillNodeButton : MonoBehaviour
{
    public Skill skill;
    
    [Header("Visual References")]
    private Image nodeImage;
    private Outline nodeOutline;
    private TextMeshProUGUI iconText;
    private TextMeshProUGUI nameText;
    
    [Header("Colors")]
    public Color unlockedColor = new Color(0f, 1f, 1f, 1f);  // Cyan glow
    public Color availableColor = new Color(0.5f, 0.5f, 0.5f, 1f); // Gray
    public Color lockedColor = new Color(0.2f, 0.2f, 0.2f, 1f);   // Dark gray

    void Start()
    {
        // Get visual components
        nodeImage = GetComponent<Image>();
        nodeOutline = GetComponent<Outline>();
        iconText = GetComponentInChildren<TextMeshProUGUI>();
        
        // Find the name text (it's a sibling, not a child)
        Transform parent = transform.parent;
        if (parent != null)
        {
            string nameLabelName = "Name";
            Transform nameTransform = parent.Find(nameLabelName);
            if (nameTransform != null)
            {
                nameText = nameTransform.GetComponent<TextMeshProUGUI>();
            }
        }
        
        Button btn = GetComponent<Button>();
        if (btn != null)
        {
            btn.onClick.AddListener(OnClick);
        }
        
        // Refresh visual state on start
        RefreshVisual();
    }

    void OnClick()
    {
        if (SkillTreeManager.Instance != null)
        {
            SkillTreeManager.Instance.OnSkillClicked(skill);
            
            // Refresh visual after unlock attempt
            RefreshVisual();
        }
    }
    
    public void RefreshVisual()
    {
        if (SkillTreeManager.Instance == null) return;
        
        bool isUnlocked = SkillTreeManager.Instance.IsUnlocked(skill);
        
        if (isUnlocked)
        {
            // UNLOCKED: Add bright cyan outline (will glow with Bloom post-processing)
            if (nodeOutline == null)
            {
                nodeOutline = gameObject.AddComponent<Outline>();
            }
            
            nodeOutline.enabled = true;
            // Use BRIGHT cyan for bloom to pick up
            nodeOutline.effectColor = new Color(0f, 2f, 2f, 1f); // HDR color - extra bright!
            nodeOutline.effectDistance = new Vector2(4, 4);
            
            Debug.Log($"✨ {skill} unlocked - outline ready for bloom!");
        }
        else
        {
            // LOCKED: Disable outline
            if (nodeOutline != null)
            {
                nodeOutline.enabled = false;
            }
        }
    }
}
