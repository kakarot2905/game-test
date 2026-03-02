using UnityEngine;

public class DeathZone : MonoBehaviour
{
    public Transform player;
    public Transform disableAfterPoint;   // empty object marker

    private bool triggered = false;

    void Update()
    {
        if (triggered || player == null) return;

        // If player passed the disable point, DeathZone stops working
        if (disableAfterPoint != null && player.position.x > disableAfterPoint.position.x)
            return;

        // Normal death check
        if (player.position.y < transform.position.y)
        {
            triggered = true;

            // Use PlayerController to handle death (triggers events, music stop, etc.)
            PlayerController pc = player.GetComponent<PlayerController>();
            if (pc != null)
            {
                // Fix: Use ForceDie to bypass invincibility (e.g. after taking damage)
                pc.ForceDie();
                // pc.TakeDamage(9999); // OLD WAY - blocked by isInvincible
            }
            else
            {
                // Fallback if no controller found
                Destroy(player.gameObject);
                if (GameOverManager.Instance != null)
                    GameOverManager.Instance.ShowGameOver();
            }
        }
    }
}
