using UnityEngine;
using System.Collections;

[RequireComponent(typeof(BoxCollider2D))]
[RequireComponent(typeof(Rigidbody2D))] // Ensure physics engine tracks it
public class LaserBeam : MonoBehaviour
{
    // Removed LineRenderer reference as user provides sprite animation
    public BoxCollider2D damageCollider;
    
    [Header("Timing")]
    public float chargeTime = 0.5f;     // Time for "growth" animation to play before damage starts
    public float activeTime = 1.0f;     // How long the laser deals damage
    public int damage = 35;

    void Start()
    {
        Debug.Log($"LaserBeam: Script Initialized on {gameObject.name}");

        // Fix Rendering Order (Front of Boss)
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null) sr.sortingOrder = 100; // Force high sorting order

        if (damageCollider == null) damageCollider = GetComponent<BoxCollider2D>();
        
        // RigidBody Setup (Force Kinematic if not set)
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
             rb.bodyType = RigidbodyType2D.Kinematic;
             rb.useFullKinematicContacts = true; // Ensure collision detection works
        }

        // Ensure collider is off initially (while laser "grows")
        if (damageCollider != null) 
        {
            damageCollider.enabled = false;
            damageCollider.isTrigger = true; // Force Trigger
        }
        else
        {
             Debug.LogError("LaserBeam: Missing BoxCollider2D!");
        }
            
        StartCoroutine(LaserRoutine());
    }

    IEnumerator LaserRoutine()
    {
        Debug.Log("LaserBeam: Starting Routine...");
        // 1. Wait for "growth" animation
        yield return new WaitForSeconds(chargeTime);

        // 2. Enable Damage Collider
        if (damageCollider != null)
        {
            damageCollider.enabled = true;
            Debug.Log("LaserBeam: FIRED! Collider Active.");
        }

        // 3. Wait for active duration
        yield return new WaitForSeconds(activeTime);

        // 4. Destroy Laser
        Debug.Log("LaserBeam: Time's up. Destroying.");
        Destroy(gameObject);
    }

    // Ensure damage works on Jump-In and Stay
    void OnTriggerEnter2D(Collider2D hit) => DealDamage(hit);
    void OnTriggerStay2D(Collider2D hit) => DealDamage(hit);

    void DealDamage(Collider2D hit)
    {
        if (damageCollider != null && !damageCollider.enabled) return;

        if (hit.CompareTag("Player"))
        {
            PlayerController p = hit.GetComponent<PlayerController>();
            if (p != null) 
            {
                p.TakeDamage(damage);
                // Debug.Log("LaserBeam: Hit Player!"); // Intentionally commented to avoid spam, uncomment if needed
            }
        }
    }
}
