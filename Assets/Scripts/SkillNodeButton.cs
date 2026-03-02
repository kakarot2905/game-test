using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Handles skill node button clicks - connects to SkillTreeManager
/// Attach this to each diamond node (done automatically by builder)
/// </summary>
public class SkillNodeButton : MonoBehaviour, UnityEngine.EventSystems.IPointerDownHandler, UnityEngine.EventSystems.IPointerUpHandler
{
    public Skill skill;
    
    [Header("Visual References")]
    private Image nodeImage;
    private Outline nodeOutline;
    private TextMeshProUGUI iconText;
    private TextMeshProUGUI nameText;
    
    [Header("Hold to Unlock")]
    public Image radialIcon; // Drag the radial icon here!
    public GameObject unlockedGlowVFX; // Assign a Glow/Particle object here!
    private bool isHolding = false;
    private float holdTimer = 0f;
    private const float HOLD_DURATION = 1.0f;
    
    [Header("Scaling")]
    private Vector3 originalScale;
    public float scalePunchAmount = 1.15f; // 15% bigger
    public float scaleSpeed = 10f;

    [Header("Colors")]
    public Color unlockedColor = new Color(0f, 1f, 1f, 1f);  // Cyan glow
    public Color availableColor = new Color(0.5f, 0.5f, 0.5f, 1f); // Gray
    public Color lockedColor = new Color(0.2f, 0.2f, 0.2f, 1f);   // Dark gray

    void Start()
    {
        // Store original scale
        originalScale = transform.localScale;

        // Get visual components
        nodeImage = GetComponent<Image>();
        nodeOutline = GetComponent<Outline>();
        iconText = GetComponentInChildren<TextMeshProUGUI>();
        
        Debug.Log($"[SkillNodeButton] Initializing {skill}...");

        // If NOT assigned in Inspector, try to find it automatically
        if (radialIcon == null)
        {
            // Find the radial icon (Hierarchy: Node_Dash -> IconContainer -> IconContainerIcon)
            Transform iconContainer = transform.Find("IconContainer");
            if (iconContainer != null)
            {
                // Disable Raycast on Container to prevent blocking inputs
                Image containerImg = iconContainer.GetComponent<Image>();
                if (containerImg != null) containerImg.raycastTarget = false;

                Transform iconTr = iconContainer.Find("IconContainerIcon");
                if (iconTr != null)
                {
                    radialIcon = iconTr.GetComponent<Image>();
                }
            }
            
            if (radialIcon == null)
            {
                 // Fallback search
                 Transform iconTr = transform.Find("IconContainerIcon");
                 if (iconTr != null) radialIcon = iconTr.GetComponent<Image>();
            }
        }
        
        // SETUP: Regardless of how we found it (Inspector or Auto), set it up correctly
        if (radialIcon != null)
        {
            radialIcon.type = Image.Type.Filled;
            radialIcon.fillMethod = Image.FillMethod.Radial360;
            radialIcon.fillOrigin = (int)Image.Origin360.Top;
            
            // CRITICAL: Disable raycast on the icon so the button parent gets the clicks
            radialIcon.raycastTarget = false; 
            Debug.Log($"[SkillNodeButton] Radial Icon setup complete for {skill}");
        }
        else
        {
             Debug.LogError($"[SkillNodeButton] Could NOT find radial icon for {skill}! Please assign it in Inspector.");
        }
        
        // Find the name text
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
        
        // Refresh visual state on start
        RefreshVisual();
    }

    void Update()
    {
        // Scale Animation (Pop Effect)
        // Lerp towards target scale based on holding state
        Vector3 targetScale = isHolding ? originalScale * scalePunchAmount : originalScale;
        transform.localScale = Vector3.Lerp(transform.localScale, targetScale, Time.unscaledDeltaTime * scaleSpeed);

        if (SkillTreeManager.Instance == null) return;
        
        bool isUnlocked = SkillTreeManager.Instance.IsUnlocked(skill);

        if (isUnlocked)
        {
            // Already unlocked - ensure icon is fully visible
            if (radialIcon != null) radialIcon.fillAmount = 1f;
            return;
        }

        // HOLD LOGIC
        if (isHolding)
        {
            // Use UNSCALED time so it works even if game is paused in skill menu!
            holdTimer += Time.unscaledDeltaTime;
            // Debug.Log($"[SkillNodeButton] Holding {skill}... {holdTimer:F2}/{HOLD_DURATION}");

            // Update Visuals
            float progress = Mathf.Clamp01(holdTimer / HOLD_DURATION);
            if (radialIcon != null) radialIcon.fillAmount = progress;
            
            // Check Completion
            if (holdTimer >= HOLD_DURATION)
            {
                Debug.Log($"[SkillNodeButton] Hold Complete! Unlocking {skill}");
                TryUnlock();
                isHolding = false; // Reset hold state
                holdTimer = 0;
            }
        }
        else
        {
            // Reset if released early or just idle
            holdTimer = 0;
            if (radialIcon != null) radialIcon.fillAmount = 1f; 
        }
    }

    public void OnPointerDown(UnityEngine.EventSystems.PointerEventData eventData)
    {
        Debug.Log($"[SkillNodeButton] Pointer DOWN on {skill}");
        
        if (SkillTreeManager.Instance.IsUnlocked(skill))
        {
            SkillTreeManager.Instance.ShowMessage("Already Unlocked");
            return;
        }

        isHolding = true;
        holdTimer = 0f;
        if (radialIcon != null) radialIcon.fillAmount = 0f; 
    }

    public void OnPointerUp(UnityEngine.EventSystems.PointerEventData eventData)
    {
        Debug.Log($"[SkillNodeButton] Pointer UP on {skill} (Held: {holdTimer:F2}s)");
        
        // If released early (Tap), we can show why it's locked or status
        // Check hold time using UNSCALED time logic (consistent with Update)
        
        isHolding = false;
        holdTimer = 0;
        
        if (radialIcon != null && !SkillTreeManager.Instance.IsUnlocked(skill))
        {
            radialIcon.fillAmount = 1f; 
        }
    }

    public void OnPointerExit(UnityEngine.EventSystems.PointerEventData eventData)
    {
        // Cancel hold if dragging off the button
        if (isHolding)
        {
            Debug.Log($"[SkillNodeButton] Pointer EXIT on {skill} - Cancelling Hold");
            isHolding = false;
            holdTimer = 0;
            
            if (radialIcon != null && !SkillTreeManager.Instance.IsUnlocked(skill))
            {
                radialIcon.fillAmount = 1f; 
            }
        }
    }

    void TryUnlock()
    {
        if (SkillTreeManager.Instance != null)
        {
            SkillTreeManager.Instance.OnSkillClicked(skill); // Re-using existing method which handles logic
            RefreshVisual();
        }
    }
    
    public void RefreshVisual()
    {
        if (SkillTreeManager.Instance == null) return;
        
        bool isUnlocked = SkillTreeManager.Instance.IsUnlocked(skill);
        
        // Also tint based on state if desired, matching declared colors
        if (nodeImage != null)
        {
            if (isUnlocked)
            {
                nodeImage.color = unlockedColor;
            }
            else
            {
                nodeImage.color = lockedColor; 
            }
        }

        if (isUnlocked)
        {
            // UNLOCKED: Show Glow VFX if assigned
            if (unlockedGlowVFX != null)
            {
                unlockedGlowVFX.SetActive(true);
                
                // Disable outline if using custom glow to avoid clash (or keep as subtle border?)
                // Let's rely entirely on the custom glow object
                if (nodeOutline != null) nodeOutline.enabled = false;
            }
            else
            {
                // Fallback: Add bright cyan outline if NO custom glow assigned
                if (nodeOutline == null)
                {
                    nodeOutline = gameObject.AddComponent<Outline>();
                }
                
                nodeOutline.enabled = true;
                nodeOutline.effectColor = new Color(0f, 2f, 2f, 1f); // HDR color
                nodeOutline.effectDistance = new Vector2(4, 4);
            }
            
            if (radialIcon) radialIcon.fillAmount = 1f; 
        }
        else
        {
            // LOCKED: Hide Glow VFX
            if (unlockedGlowVFX != null)
            {
                unlockedGlowVFX.SetActive(false);
            }
            
            // Disable outline
            if (nodeOutline != null)
            {
                nodeOutline.enabled = false;
            }
            
            if (radialIcon) radialIcon.fillAmount = 1f; // User requested: Locked = 100% Visible
        }
    }
}
