using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using System.Collections;

/// <summary>
/// Handles tooltip display on hover
/// Attach this to each diamond node (done automatically by builder)
/// </summary>
public class SkillNodeTooltip : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public string description;
    public int cost = 1; // Skill point cost
    
    private GameObject tooltipObj;
    private TextMeshProUGUI tooltipText;
    private RectTransform tooltipRect;
    private CanvasGroup tooltipCanvasGroup;
    private Coroutine fadeCoroutine;

    void Start()
    {
        // Find the tooltip in the canvas
        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas != null)
        {
            Transform tooltipTransform = canvas.transform.Find("Tooltip");
            if (tooltipTransform != null)
            {
                tooltipObj = tooltipTransform.gameObject;
                tooltipRect = tooltipObj.GetComponent<RectTransform>();
                tooltipText = tooltipObj.GetComponentInChildren<TextMeshProUGUI>();
                
                // Get or add CanvasGroup for fade animation
                tooltipCanvasGroup = tooltipObj.GetComponent<CanvasGroup>();
                if (tooltipCanvasGroup == null)
                {
                    tooltipCanvasGroup = tooltipObj.AddComponent<CanvasGroup>();
                }
                
                // Make sure CanvasGroup doesn't block raycasts
                tooltipCanvasGroup.interactable = false;
                tooltipCanvasGroup.blocksRaycasts = false;
                tooltipCanvasGroup.alpha = 0f; // Start invisible
                
                if (tooltipText != null)
                {
                    Debug.Log($"✅ Found tooltip for {gameObject.name}");
                }
                else
                {
                    Debug.LogError($"❌ Tooltip found but no text! Check Tooltip GameObject.");
                }
            }
            else
            {
                Debug.LogError("❌ Tooltip GameObject not found in canvas!");
            }
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (tooltipObj == null || tooltipText == null) return;
        
        Debug.Log($"🖱️ Hovering over skill: {gameObject.name}");
        
        RectTransform nodeRect = GetComponent<RectTransform>();
        
        // Format tooltip with description and cost on separate lines
        string formattedText = $"{description}\n\n<color=#FFD700>Cost: {cost}</color>";
        tooltipText.text = formattedText;
        
        tooltipRect.anchoredPosition = nodeRect.anchoredPosition + new Vector2(0, 120);
        
        // Stop any existing fade animation
        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
        }
        
        // Start fade-in animation
        tooltipObj.SetActive(true);
        fadeCoroutine = StartCoroutine(FadeIn());
        
        Debug.Log($"✅ Tooltip shown with fade-in: '{description}'");
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (tooltipObj == null) return;
        
        // Don't stop coroutine - let it finish naturally
        // Just hide the tooltip immediately
        tooltipCanvasGroup.alpha = 0f;
        tooltipObj.SetActive(false);
        Debug.Log($"👋 Stopped hovering over: {gameObject.name}");
    }
    
    IEnumerator FadeIn()
    {
        if (tooltipCanvasGroup == null)
        {
            Debug.LogError("❌ CanvasGroup is null! Can't fade.");
            yield break;
        }
        
        float duration = 0.15f; // Faster fade
        float elapsed = 0f;
        
        tooltipCanvasGroup.alpha = 0f;
        Debug.Log("🎬 Starting fade-in animation...");
        
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime; // Use unscaled time
            tooltipCanvasGroup.alpha = Mathf.Clamp01(elapsed / duration);
            yield return null;
        }
        
        tooltipCanvasGroup.alpha = 1f;
        Debug.Log($"✅ Fade-in complete! Alpha: {tooltipCanvasGroup.alpha}");
    }
}
