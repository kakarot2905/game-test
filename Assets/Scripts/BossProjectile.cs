using UnityEngine;

public class BossProjectile : MonoBehaviour
{
    public int damage = 20;
    public float speed = 8f;
    public float lifeTime = 5f;

    void Start()
    {
        Destroy(gameObject, lifeTime);
        
        // Initial velocity might be set by spawner, but ensure it moves if logic is self-contained
        // Rigidbody2D rb = GetComponent<Rigidbody2D>();
        // if (rb != null && rb.linearVelocity == Vector2.zero) 
        //    rb.linearVelocity = transform.right * speed;
    }

    void OnTriggerEnter2D(Collider2D hit)
    {
        if (hit.CompareTag("Player"))
        {
            PlayerController player = hit.GetComponent<PlayerController>();
            if (player != null)
            {
                player.TakeDamage(damage);
            }
            Destroy(gameObject);
        }
        else if (hit.CompareTag("Ground")) // Optional: Destroy on wall hit
        {
            Destroy(gameObject);
        }
    }
}
