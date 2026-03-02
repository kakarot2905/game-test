using UnityEngine;
using TMPro;

public class DamagePopup : MonoBehaviour
{
    [Header("Animation Settings")]
    public float floatSpeed = 1.5f;
    public float fadeSpeed = 1f;
    public float lifetime = 0.8f;
    public float randomOffsetX = 0.5f;

    [Header("Font")]
    public TMP_FontAsset fontAsset;   // ← ASSIGN TMP FONT ASSET HERE

    private TextMeshPro textMesh;
    private Color textColor;
    private float timer;
    private Vector3 moveDirection;

    public static DamagePopup Create(Vector3 position, int damageAmount, bool isCritical = false)
    {
        // Try to find the prefab
        GameObject prefab = Resources.Load<GameObject>("DamagePopup");

        if (prefab == null)
        {
            // Create a simple damage popup if no prefab exists
            GameObject popupObj = new GameObject("DamagePopup");
            popupObj.transform.position = position;

            TextMeshPro tmp = popupObj.AddComponent<TextMeshPro>();
            tmp.text = damageAmount.ToString();
            tmp.fontSize = 8;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.sortingOrder = 100;

            DamagePopup popup = popupObj.AddComponent<DamagePopup>();
            popup.Setup(damageAmount, isCritical);

            return popup;
        }

        GameObject instance = Instantiate(prefab, position, Quaternion.identity);
        DamagePopup damagePopup = instance.GetComponent<DamagePopup>();
        damagePopup.Setup(damageAmount, isCritical);

        return damagePopup;
    }

    public static DamagePopup Create(Vector3 position, string text, Color color)
    {
        // Try to find the prefab
        GameObject prefab = Resources.Load<GameObject>("DamagePopup");

        if (prefab == null)
        {
            // Create a simple popup if no prefab exists
            GameObject popupObj = new GameObject("DamagePopup");
            popupObj.transform.position = position;

            TextMeshPro tmp = popupObj.AddComponent<TextMeshPro>();
            tmp.text = text;
            tmp.fontSize = 10;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.sortingOrder = 100;

            DamagePopup popup = popupObj.AddComponent<DamagePopup>();
            popup.Setup(text, color);

            return popup;
        }

        GameObject instance = Instantiate(prefab, position, Quaternion.identity);
        DamagePopup damagePopup = instance.GetComponent<DamagePopup>();
        damagePopup.Setup(text, color);

        return damagePopup;
    }

    void Awake()
    {
        textMesh = GetComponent<TextMeshPro>();

        // Apply custom font if assigned
        if (fontAsset != null && textMesh != null)
            textMesh.font = fontAsset;
    }

    public void Setup(int damageAmount, bool isCritical = false)
    {
        if (textMesh == null)
            textMesh = GetComponent<TextMeshPro>();

        // Apply font again (important for prefab/runtime creation)
        if (fontAsset != null)
            textMesh.font = fontAsset;

        textMesh.text = damageAmount.ToString();

        // Set color based on damage type
        if (isCritical)
        {
            textColor = new Color(1f, 0.8f, 0f, 1f); // Gold for critical
            textMesh.fontSize = 10;
        }
        else
        {
            textColor = new Color(1f, 0.2f, 0.2f, 1f); // Red for normal damage
        }

        textMesh.color = textColor;

        // Random horizontal offset
        float randomX = Random.Range(-randomOffsetX, randomOffsetX);
        moveDirection = new Vector3(randomX, 1f, 0f).normalized;

        timer = lifetime;
    }

    public void Setup(string text, Color color)
    {
        if (textMesh == null)
            textMesh = GetComponent<TextMeshPro>();

        // Apply font again (important for prefab/runtime creation)
        if (fontAsset != null)
            textMesh.font = fontAsset;

        textMesh.text = text;
        textColor = color;
        textMesh.color = textColor;
        textMesh.fontSize = 10; // Default size for text popups

        // Random horizontal offset
        float randomX = Random.Range(-randomOffsetX, randomOffsetX);
        moveDirection = new Vector3(randomX, 1f, 0f).normalized;

        timer = lifetime;
    }

    void Update()
    {
        // Move upward
        transform.position += moveDirection * floatSpeed * Time.deltaTime;

        // Countdown
        timer -= Time.deltaTime;

        // Fade out in the second half of lifetime
        if (timer < lifetime * 0.5f)
        {
            float alpha = timer / (lifetime * 0.5f);
            textColor.a = alpha;
            textMesh.color = textColor;
        }

        // Destroy when done
        if (timer <= 0)
        {
            Destroy(gameObject);
        }
    }
}
