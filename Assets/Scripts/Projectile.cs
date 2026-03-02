using UnityEngine;

public class Projectile : MonoBehaviour
{
    public int damage = 12;
    public float maxRange = 8f;  // Maximum travel distance before projectile disappears
    
    private Vector3 startPosition;
    
    void Start()
    {
        startPosition = transform.position;
        
        // Fallback safety destroy (in case range check fails)
        Destroy(gameObject, 5f);
    }
    
    void Update()
    {
        // Check if projectile has exceeded max range
        float distanceTraveled = Vector3.Distance(startPosition, transform.position);
        if (distanceTraveled >= maxRange)
        {
            Destroy(gameObject);
        }
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        if (col.CompareTag("Enemy"))
        {
            col.GetComponent<EnemyController>()?.SendMessage("TakeDamage", damage, SendMessageOptions.DontRequireReceiver);
            Destroy(gameObject);
        }
    }
}
