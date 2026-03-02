using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HealSlotUI : MonoBehaviour
{
    public int healAmount = 50;
    public TextMeshProUGUI countText;

    PlayerController player;
    PlayerPowerupBuffer buffer;
    Button button;

    void Awake()
    {
        player = FindObjectOfType<PlayerController>();
        buffer = FindObjectOfType<PlayerPowerupBuffer>();
        button = GetComponent<Button>();

        button.onClick.AddListener(UseHeal);
    }

    void Update()
    {
        countText.text = buffer.healCount > 0 ? buffer.healCount.ToString() : "";
    }

    void UseHeal()
    {
        if (buffer.healCount <= 0) return;
        if (player.GetHP() >= player.maxHP) return;

        int before = player.GetHP();

        buffer.healCount--;

        Debug.Log($"[HealSlot] Healing {healAmount} | HP before: {before}");

        player.Heal(healAmount);

        Debug.Log($"[HealSlot] HP after: {player.GetHP()}");
    }
}
