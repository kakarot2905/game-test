using UnityEngine;

/// <summary>
/// NightBorne enemy with explosion on death.
/// Attach this to NightBorne prefabs INSTEAD of EnemyController.
/// Set up Animation Event on death animation to call DealExplosionDamage() at explosion frame.
/// </summary>
public class NightBorneController : EnemyController
{
    [Header("NightBorne - Explosion")]
    public float explosionRadius = 2.5f;
    public int explosionDamage = 35;
    public float destroyDelay = 3f;  // Time before destroy - set longer than death animation!
    
    [Header("Warning Indicator")]
    public bool showWarning = true;
    public string warningMessage = "DANGER!";
    
    private bool explosionTriggered = false;
    
    /// <summary>
    /// Override Die completely to use longer destroy delay for explosion animation.
    /// </summary>
    protected override void Die()
    {
        // Spawn warning popup at start of death (same style as DamagePopup)
        if (showWarning)
        {
            Vector3 popupPos = transform.position + Vector3.up * 1.5f;
            WarningPopup.Create(popupPos, warningMessage);
        }
        
        // Custom death handling - DO NOT call base.Die() as it uses shorter delay
        anim.SetTrigger("Death");
        rb.linearVelocity = Vector2.zero;
        rb.simulated = false;
        
        // Spawn XP particles (manually since we don't call base.Die())
        if (xpDropCount > 0)
        {
            Vector3 spawnPos = transform.position + Vector3.up * 0.5f;
            XPParticle.SpawnParticles(spawnPos, xpDropCount, xpParticlePrefab);
        }
        
        // Use longer delay to ensure animation event fires
        Destroy(gameObject, destroyDelay);
    }
    
    /// <summary>
    /// Called by Animation Event at the explosion frame of death animation.
    /// Deals area damage to player if within radius.
    /// </summary>
    public void DealExplosionDamage()
    {
        if (explosionTriggered) return;
        explosionTriggered = true;
        
        // Find player in explosion radius
        Collider2D hit = Physics2D.OverlapCircle(
            transform.position,
            explosionRadius,
            LayerMask.GetMask("Player")  // Make sure player is on "Player" layer
        );
        
        if (hit != null)
        {
            PlayerController player = hit.GetComponent<PlayerController>();
            if (player != null)
            {
                player.TakeDamage(explosionDamage);
                Debug.Log($"[NightBorne] EXPLOSION hit player for {explosionDamage} damage!");
            }
        }
        else
        {
            Debug.Log("[NightBorne] Explosion missed - player not in range");
        }
        
        // DISABLE ANIMATOR to prevent transitioning to Attack animation after death
        anim.enabled = false;
    }
    
    void OnDrawGizmosSelected()
    {
        // Draw explosion radius
        Gizmos.color = new Color(1f, 0.5f, 0f, 0.5f);  // Orange
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}
