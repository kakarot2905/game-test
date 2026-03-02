using UnityEngine;

public class HealPickup : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D other)
    {
        PlayerPowerupBuffer buffer = other.GetComponent<PlayerPowerupBuffer>();
        if (buffer == null) return;

        if (buffer.healCount >= 1)
        {
            Debug.Log("[HealPickup] Heal slot full, pickup ignored");
            return;
        }

        buffer.healCount = 1;
        Debug.Log("[HealPickup] Heal stored (count = 1)");
        Destroy(gameObject);
    }
}
