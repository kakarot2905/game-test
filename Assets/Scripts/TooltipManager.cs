using UnityEngine;
using TMPro;

/// <summary>
/// Manages the tooltip GameObject - shows/hides with descriptions
/// Attach this to the Tooltip GameObject (done automatically by builder)
/// </summary>
public class TooltipManager : MonoBehaviour
{
    private TextMeshProUGUI tooltipText;
    private RectTransform rectTransform;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        FindTooltipText();
        gameObject.SetActive(false);
    }

    void Start()
    {
        // Double-check in Start() in case Awake didn't find it
        if (tooltipText == null)
        {
            Debug.LogWarning("⚠️ TooltipText not found in Awake, trying again in Start...");
            FindTooltipText();
        }
    }

    void FindTooltipText()
    {
        // Try multiple ways to find the text component
        tooltipText = GetComponentInChildren<TextMeshProUGUI>();
        
        if (tooltipText == null)
        {
            // Try finding by name
            Transform textChild = transform.Find("TooltipText");
            if (textChild == null)
                textChild = transform.Find("Text");
            
            if (textChild != null)
                tooltipText = textChild.GetComponent<TextMeshProUGUI>();
        }
        
        if (tooltipText == null)
        {
            Debug.LogError("❌ TooltipManager: TextMeshProUGUI not found! Make sure there's a child with TextMeshProUGUI component.");
        }
        else
        {
            Debug.Log($"✅ TooltipManager: Found text component on '{tooltipText.gameObject.name}'");
        }
    }

    public void Show(string text, Vector2 nodeAnchoredPosition)
    {
        if (tooltipText == null)
        {
            Debug.LogError("❌ tooltipText is null! Cannot show tooltip.");
            return;
        }
        
        tooltipText.text = text;
        gameObject.SetActive(true);
        
        // Simple positioning: place tooltip above the node
        rectTransform.anchoredPosition = nodeAnchoredPosition + new Vector2(0, 120);
        
        Debug.Log($"✅ Tooltip shown: '{text}' at position {rectTransform.anchoredPosition}");
    }

    public void Hide()
    {
        gameObject.SetActive(false);
        Debug.Log("🙈 Tooltip hidden");
    }
}
