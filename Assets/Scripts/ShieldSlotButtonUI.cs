using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShieldSlotButtonUI : MonoBehaviour
{
    public TextMeshProUGUI countText;

    PlayerPowerupBuffer buffer;
    Button button;

    void Awake()
    {
        buffer = FindObjectOfType<PlayerPowerupBuffer>();
        button = GetComponent<Button>();

        button.onClick.AddListener(ActivateShield);
    }

    void Update()
    {
        countText.text = buffer.shieldCount > 0 ? buffer.shieldCount.ToString() : "";
    }

    void ActivateShield()
    {
        if (buffer.shieldCount <= 0) return;
        if (buffer.shieldActive) return;

        buffer.shieldCount--;
        buffer.shieldActive = true;

        Debug.Log("[ShieldSlot] Shield ACTIVATED");
    }
}
