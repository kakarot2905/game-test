using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 3f;
    public float changeDirTime = 2f;

    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundRadius = 0.2f;
    public LayerMask groundLayer;

    [Header("Combat")]
    public int maxHP = 100;

    public float knockbackForce = 6f;
    public float hitReactionCooldown = 0.4f; // Easier stun-lock for players
    private float lastHitTime = -10f;

    [Header("Melee Attack")]
    public float attackRange = 1.2f;
    public float attackCooldown = 1.5f;
    public Transform attackPoint;
    public int damage = 15;
    public LayerMask playerLayer;

    [Header("AI Behavior")]
    public bool startIdle = false;          // MimicChest = true, Golem = false
    public float wakeRange = 6f;            // Distance to notice player
    public float chaseRange = 8f;           // Stop chasing if too far

    [Header("Cliff Detection")]
    public Transform cliffCheck;
    public float cliffDistance = 0.4f;

    [Header("XP Drop")]
    public int xpDropCount = 3;               // Number of XP particles to spawn on death
    public GameObject xpParticlePrefab;       // Optional: assign prefab, or uses default

    bool isAwake = false;
    bool isChasing = false;


    float attackTimer;
    protected Transform player;

    float attackLockTimer;
    public float maxAttackLock = 1.2f;   // safety timeout


    protected int currentHP;
    protected bool isDead = false;
    protected bool isAttacking = false;
    
    public int CurrentHP => currentHP;

    public bool IsDefeated => isDead || currentHP <= 0;

    protected Rigidbody2D rb;
    protected Animator anim;
    protected SpriteRenderer sr;

    private bool isGrounded;
    private float moveDir = 1f;
    private float timer;

    // =============================
    // START
    // =============================
    protected virtual void Start()
    {
        player = GameObject.FindWithTag("Player")?.transform;

        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        sr = GetComponent<SpriteRenderer>();
        currentHP = maxHP;

        PickNewDirection();
    }

    // =============================
    // UPDATE LOOP
    // =============================
    protected virtual void Update()
    {
        if (isDead) return;

        attackTimer -= Time.deltaTime;

        if (isAttacking)
        {
            attackLockTimer -= Time.deltaTime;
            if (attackLockTimer <= 0)
                isAttacking = false;
        }

        HandleAI();
        UpdateAnimations();
    }


    // =============================
    // MOVEMENT
    // =============================
    protected virtual void ChasePlayer()
    {
        if (player == null) return;

        float dir = player.position.x > transform.position.x ? 1 : -1;

        // Face player
        sr.flipX = dir < 0;
        UpdateAttackPoint();

        // Cliff check
        if (!IsGroundAhead(dir))
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            return;
        }

        rb.linearVelocity = new Vector2(dir * moveSpeed, rb.linearVelocity.y);
    }

    bool IsGroundAhead(float dir)
    {
        if (cliffCheck == null) return true;

        Vector2 pos = cliffCheck.position;
        pos.x += dir * cliffDistance;

        RaycastHit2D hit = Physics2D.Raycast(pos, Vector2.down, 1f, groundLayer);

        return hit.collider != null;
    }


    protected virtual void HandleAI()
    {
        if (player == null) return;

        float dist = Mathf.Abs(player.position.x - transform.position.x);

        // WAKE UP
        if (!isAwake && dist < wakeRange)
            isAwake = true;

        // GO BACK TO IDLE
        if (isAwake && dist > chaseRange)
            isAwake = false;

        if (!isAwake)
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            return;
        }

        // Try attacking first
        DetectAndAttack();

        if (!isAttacking)
            ChasePlayer();
    }

    void PickNewDirection()
    {
        timer = Random.Range(1.5f, changeDirTime);

        int r = Random.Range(-1, 2);
        moveDir = r;

        if (moveDir == 0)
            moveDir = Random.value > 0.5f ? 1 : -1;
    }

    // =============================
    // ATTACK SYSTEM
    // =============================
    void DetectAndAttack()
    {
        if (player == null || isAttacking || attackTimer > 0 || attackPoint == null)
            return;

        Collider2D hit = Physics2D.OverlapCircle(
            attackPoint.position,
            attackRange,
            playerLayer
        );

        if (hit != null)
        {
            Debug.Log($"{name} REQUESTED ATTACK");

            attackTimer = attackCooldown;
            isAttacking = true;
            attackLockTimer = maxAttackLock;

            rb.linearVelocity = Vector2.zero;

            // Face the player
            sr.flipX = player.position.x < transform.position.x;
            UpdateAttackPoint();


            anim.ResetTrigger("Attack");
            anim.SetTrigger("Attack");
        }
    }


    // Called by animation event (LAST frame of attack)
    public void EndAttack()
    {
        isAttacking = false;
    }

    // Called by animation event at HIT frame
    public virtual void DealDamage()
    {
        if (player == null || attackPoint == null) return;

        Collider2D hit = Physics2D.OverlapCircle(
            attackPoint.position,
            attackRange,
            playerLayer
        );

        if (hit != null)
        {
            hit.GetComponent<PlayerController>()?.TakeDamage(damage);
            Debug.Log($"{name} HIT PLAYER");
        }
        else
        {
            Debug.Log($"{name} MISSED");
        }
    }

    protected virtual void UpdateAttackPoint()
    {
        if (attackPoint == null) return;

        Vector3 local = attackPoint.localPosition;
        local.x = Mathf.Abs(local.x) * (sr.flipX ? -1f : 1f);
        attackPoint.localPosition = local;
    }




    // =============================
    // DAMAGE & DEATH
    // =============================
    public virtual void TakeDamage(int dmg)
    {
        if (isDead) return;

        currentHP -= dmg;
        
        // Stun Lock Prevention
        if (Time.time >= lastHitTime + hitReactionCooldown)
        {
            lastHitTime = Time.time;
            anim.SetTrigger("Hit");
            
            // Only apply strong knockback if we trigger the hit reaction
            float dir = sr.flipX ? 1f : -1f;
            rb.linearVelocity = new Vector2(dir * knockbackForce, 3f);
        }

        // Spawn damage popup
        Vector3 popupPos = transform.position + Vector3.up * 1f;
        DamagePopup.Create(popupPos, dmg);

        // Spawn damage popup

        if (currentHP <= 0)
            Die();
    }

    protected virtual void Die()
    {
        isDead = true;
        anim.SetTrigger("Death");
        rb.linearVelocity = Vector2.zero;
        rb.simulated = false;
        
        // Spawn XP particles
        SpawnXPParticles();
        
        Destroy(gameObject, 1.5f);
    }
    
    void SpawnXPParticles()
    {
        if (xpDropCount <= 0) return;
        
        Vector3 spawnPos = transform.position + Vector3.up * 0.5f;
        XPParticle.SpawnParticles(spawnPos, xpDropCount, xpParticlePrefab);
    }

    // =============================
    // ANIMATIONS
    // =============================
    void UpdateAnimations()
    {
        anim.SetBool("isRunning", Mathf.Abs(rb.linearVelocity.x) > 0.1f);
        // Note: isGrounded and yVelocity parameters removed - enemies don't need them
    }

    // =============================
    // DEBUG
    // =============================
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        if (attackPoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(attackPoint.position, attackRange);
        }


        if (groundCheck != null)
            Gizmos.DrawWireSphere(groundCheck.position, groundRadius);
    }
}
