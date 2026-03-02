using UnityEngine;
using TMPro;

/// <summary>
/// Warning popup that floats up and fades out, similar to DamagePopup.
/// Used for NightBorne explosion warning.
/// </summary>
public class WarningPopup : MonoBehaviour
{
    [Header("Animation Settings")]
    public float floatSpeed = 0.5f;
    public float lifetime = 1.2f;
    
    [Header("Font")]
    public TMP_FontAsset fontAsset;
    
    private TextMeshPro textMesh;
    private Color textColor;
    private float timer;
    
    public static WarningPopup Create(Vector3 position, string message = "DANGER!")
    {
        // Try to find a prefab
        GameObject prefab = Resources.Load<GameObject>("WarningPopup");
        
        if (prefab == null)
        {
            // Create dynamically if no prefab exists
            GameObject popupObj = new GameObject("WarningPopup");
            popupObj.transform.position = position;
            
            TextMeshPro tmp = popupObj.AddComponent<TextMeshPro>();
            tmp.text = message;
            tmp.fontSize = 6;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.sortingOrder = 100;
            
            WarningPopup popup = popupObj.AddComponent<WarningPopup>();
            popup.Setup(message);
            
            return popup;
        }
        
        GameObject instance = Instantiate(prefab, position, Quaternion.identity);
        WarningPopup warningPopup = instance.GetComponent<WarningPopup>();
        warningPopup.Setup(message);
        
        return warningPopup;
    }
    
    void Awake()
    {
        textMesh = GetComponent<TextMeshPro>();
        
        if (fontAsset != null && textMesh != null)
            textMesh.font = fontAsset;
    }
    
    public void Setup(string message)
    {
        if (textMesh == null)
            textMesh = GetComponent<TextMeshPro>();
        
        if (fontAsset != null)
            textMesh.font = fontAsset;
        
        textMesh.text = message;
        
        // Orange-red warning color
        textColor = new Color(1f, 0.4f, 0.1f, 1f);
        textMesh.color = textColor;
        
        timer = lifetime;
    }
    
    void Update()
    {
        // Float upward slowly
        transform.position += Vector3.up * floatSpeed * Time.deltaTime;
        
        timer -= Time.deltaTime;
        
        // Pulse effect (scale oscillation)
        float pulse = 1f + Mathf.Sin(Time.time * 10f) * 0.1f;
        transform.localScale = Vector3.one * pulse;
        
        // Fade out in the last third of lifetime
        if (timer < lifetime * 0.33f)
        {
            float alpha = timer / (lifetime * 0.33f);
            textColor.a = alpha;
            textMesh.color = textColor;
        }
        
        if (timer <= 0)
        {
            Destroy(gameObject);
        }
    }
}
