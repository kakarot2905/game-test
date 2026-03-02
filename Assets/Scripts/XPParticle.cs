using UnityEngine;

/// <summary>
/// XP particle that drifts randomly then flows toward player.
/// On reaching player, adds XP to ExperienceManager.
/// </summary>
public class XPParticle : MonoBehaviour
{
    [Header("Movement")]
    public float driftDuration = 0.5f;        // Time to drift randomly before seeking player
    public float driftSpeed = 2f;             // Random drift speed
    public float seekSpeed = 8f;              // Speed when chasing player
    public float seekAcceleration = 15f;      // How fast to accelerate toward player
    public float absorptionRadius = 0.3f;     // Distance to player for absorption
    
    [Header("Visual")]
    public float pulseSpeed = 5f;
    public float pulseAmount = 0.2f;
    
    private Transform player;
    private Vector2 driftDirection;
    private float driftTimer;
    private float currentSpeed;
    private bool seeking = false;
    
    void Start()
    {
        // Find player
        GameObject playerObj = GameObject.FindWithTag("Player");
        if (playerObj != null)
            player = playerObj.transform;
        
        // Set random drift direction
        driftDirection = Random.insideUnitCircle.normalized;
        driftTimer = driftDuration;
        currentSpeed = driftSpeed;
        
        // Safety destroy
        Destroy(gameObject, 10f);
    }
    
    void Update()
    {
        if (player == null) return;
        
        // Pulse effect
        float pulse = 1f + Mathf.Sin(Time.time * pulseSpeed) * pulseAmount;
        transform.localScale = Vector3.one * pulse * 0.5f;
        
        if (driftTimer > 0)
        {
            // Drift phase - random movement
            driftTimer -= Time.deltaTime;
            transform.position += (Vector3)(driftDirection * driftSpeed * Time.deltaTime);
        }
        else
        {
            // Seek phase - flow toward player
            seeking = true;
            Vector2 toPlayer = (player.position - transform.position).normalized;
            
            // Accelerate toward player
            currentSpeed = Mathf.MoveTowards(currentSpeed, seekSpeed, seekAcceleration * Time.deltaTime);
            transform.position += (Vector3)(toPlayer * currentSpeed * Time.deltaTime);
            
            // Check if absorbed
            float dist = Vector2.Distance(transform.position, player.position);
            if (dist < absorptionRadius)
            {
                Absorb();
            }
        }
    }
    
    void Absorb()
    {
        // Add XP
        if (ExperienceManager.Instance != null)
        {
            ExperienceManager.Instance.GainXP(1);
        }
        
        Destroy(gameObject);
    }
    
    /// <summary>
    /// Spawn multiple XP particles at a position.
    /// </summary>
    public static void SpawnParticles(Vector3 position, int count, GameObject prefab = null)
    {
        for (int i = 0; i < count; i++)
        {
            GameObject particle;
            
            if (prefab != null)
            {
                particle = Instantiate(prefab, position, Quaternion.identity);
            }
            else
            {
                // Create simple particle if no prefab
                particle = CreateDefaultParticle(position);
            }
        }
    }
    
    static GameObject CreateDefaultParticle(Vector3 position)
    {
        GameObject obj = new GameObject("XPParticle");
        obj.transform.position = position;
        
        // Add sprite renderer
        SpriteRenderer sr = obj.AddComponent<SpriteRenderer>();
        sr.sprite = CreatePixelSprite();
        sr.color = new Color(0.5f, 0.7f, 1f, 1f);  // Green glow
        sr.sortingOrder = 50;
        
        // Add XPParticle component
        obj.AddComponent<XPParticle>();
        
        return obj;
    }
    
    static Sprite CreatePixelSprite()
    {
        // Create a simple 4x4 pixel texture
        Texture2D tex = new Texture2D(4, 4);
        Color[] colors = new Color[16];
        for (int i = 0; i < 16; i++)
            colors[i] = Color.white;
        tex.SetPixels(colors);
        tex.Apply();
        tex.filterMode = FilterMode.Point;
        
        return Sprite.Create(tex, new Rect(0, 0, 4, 4), new Vector2(0.5f, 0.5f), 16);
    }
}
