using UnityEngine;

public class ShieldPickup : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D other)
    {
        PlayerPowerupBuffer buffer = other.GetComponent<PlayerPowerupBuffer>();
        if (buffer == null) return;

        if (buffer.shieldCount >= 1)
        {
            Debug.Log("[ShieldPickup] Shield slot full, pickup ignored");
            return;
        }

        buffer.shieldCount = 1;
        Debug.Log("[ShieldPickup] Shield stored (count = 1)");
        Destroy(gameObject);
    }
}
