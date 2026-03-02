using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BossController : EnemyController
{
    public enum BossPhase { Phase1, Phase2, Phase3 }

    [Header("Boss Settings")]
    public BossPhase currentPhase = BossPhase.Phase1;
    public string bossName = "The Construct";

    [Header("Attacks")]
    public GameObject projectilePrefab;
    public Transform firePoint;
    public float projectileSpeed = 8f;
    
    public GameObject laserPrefab;
    public Transform laserOrigin;
    
    public GameObject minionPrefab; // Golem
    public Transform[] minionSpawnPoints;

    [Header("Phase Settings")]
    public int phase2Threshold = 50; // Percentage
    
    [Header("Combat Logic")]
    public float minRangeAttackDistance = 3.5f; // If player is further than this, use Ranged
    public float meleeHitRange = 2.5f; // Actual radius of the punch hitbox

    
    [Header("State")]
    public bool isEnraged = false;
    public bool isImmune = false;
    private bool isSummoning = false;
    
    private List<EnemyController> activeMinions = new List<EnemyController>();

    // State Machine
    private enum State { Idle, Chase, Attack, Cooldown, Summoning }
    private State currentState = State.Idle;

    protected override void Start()
    {
        base.Start();
        // Custom Boss Init
        // Custom Boss Init
        currentHP = maxHP;
        // attackRange = 4f; // REMOVED: Respect Inspector value!
        moveSpeed = 2f;   // Slower, heavier movement

    }

    protected override void Update()
    {
        base.Update();
        
        if (isSummoning)
        {
            CheckMinions();
        }
    }

    protected override void HandleAI()
    {
        if (isDead) return;
        if (player == null) return;

        // If summoning, do nothing (or specific summoning behavior)
        if (currentState == State.Summoning) return;

        // Use transform.position for general engagement (as requested)
        float dist = Vector2.Distance(player.position, transform.position);

        switch (currentState)
        {
            case State.Idle:
                if (dist < wakeRange) 
                {
                    currentState = State.Chase;
                    Debug.Log("Boss: Woke up! Chasing.");
                }
                break;

            case State.Chase:
                ChasePlayer();
                if (dist < attackRange)
                {
                    Debug.Log($"Boss: In Range ({dist} < {attackRange}). Attacking!");
                    currentState = State.Attack;
                    StartCoroutine(AttackRoutine());
                }
                break;
                
            case State.Attack:
                // Handled by Coroutine
                break;

            case State.Cooldown:
                // Waiting
                break;
        }
    }

    IEnumerator AttackRoutine()
    {
        rb.linearVelocity = Vector2.zero;
        
        // Face Player
        if (player.position.x < transform.position.x) sr.flipX = true;
        else sr.flipX = false;

        UpdateAttackPoint(); // From EnemyController
        UpdateFirePoint();   // New method
        UpdateLaserPoint();  // Fix Laser Origin

        // Use transform.position for Attack Decision (Melee vs Ranged)
        float dist = Vector2.Distance(player.position, transform.position);

        // Choose Attack based on Phase and Distance
        // Phase 3 has Laser priority
        if (currentPhase == BossPhase.Phase3 && Random.value < 0.4f)
        {
            anim.SetTrigger("LaserAttack");
            yield return new WaitForSeconds(2f);
        }
        else
        {
            // Decision: Melee vs Ranged
            if (dist > minRangeAttackDistance)
            {
                // Player is far -> Ranged Attack
                anim.SetTrigger("RangeAttack");
            }
            else
            {
                // Player is close -> Melee Attack
                anim.SetTrigger("MeleeAttack");
            }
        }

        // Wait for attack to finish
        yield return new WaitForSeconds(1.5f);
        
        currentState = State.Cooldown;
        yield return new WaitForSeconds(isEnraged ? 0.5f : 1.5f);
        currentState = State.Idle;
    }

    void UpdateFirePoint()
    {
        if (firePoint == null) return;
        Vector3 local = firePoint.localPosition;
        // Flip x based on sprite flip (sr.flipX is true when facing LEFT)
        // If facing Left (flipX=true), local.x should be NEGATIVE
        local.x = Mathf.Abs(local.x) * (sr.flipX ? -1f : 1f);
        firePoint.localPosition = local;
    }

    void UpdateLaserPoint()
    {
        if (laserOrigin == null) return;
        Vector3 local = laserOrigin.localPosition;
        local.x = Mathf.Abs(local.x) * (sr.flipX ? -1f : 1f);
        laserOrigin.localPosition = local;
    }

    // Called by Animation Event
    public void ShootProjectile()
    {
        if (projectilePrefab != null && firePoint != null)
        {
            GameObject p = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);
            
            // Calc direction
            Vector2 dir = (player.position - firePoint.position).normalized;
            p.GetComponent<Rigidbody2D>().linearVelocity = dir * projectileSpeed;
            
            // Set damage?
        }
    }

    // Called by Animation Event
    // Called by Animation Event
    public void FireLaser()
    {
        Debug.Log("Boss: Animation Event 'FireLaser' CALLED!");
        if (laserPrefab != null && laserOrigin != null)
        {
            // Spawn attached to Boss? Let's detach it so it doesn't rotate WITH the boss if the boss turns
            // Actually, keep it attached but use world rotation
            GameObject l = Instantiate(laserPrefab, laserOrigin.position, Quaternion.identity, transform);
            
            // Validate Script
            if (l.GetComponent<LaserBeam>() == null)
            {
                 Debug.LogWarning("Boss: Laser Prefab missing 'LaserBeam' script! Adding temporarily.");
                 l.AddComponent<LaserBeam>();
            }

            // AIM AT PLAYER
            if (player != null)
            {
                Vector3 dir = player.position - laserOrigin.position;
                float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
                
                // If the laser sprite faces RIGHT by default:
                l.transform.rotation = Quaternion.Euler(0, 0, angle);
                
                // If the laser sprite faces LEFT by default, add 180:
                // l.transform.rotation = Quaternion.Euler(0, 0, angle + 180);
            }
            else
            {
                 // Fallback if no player found (just shoot straight)
                 l.transform.localRotation = sr.flipX ? Quaternion.Euler(0, 180, 0) : Quaternion.identity;
            }
        }
        else
        {
             Debug.LogWarning("Boss: Cannot fire laser! Missing Prefab or Origin.");
        }
    }

    public override void DealDamage()
    {
        if (player == null || attackPoint == null) return;

        // Use distinct meleeHitRange instead of the large attackRange
        Collider2D hit = Physics2D.OverlapCircle(attackPoint.position, meleeHitRange, playerLayer);

        if (hit != null)
        {
            hit.GetComponent<PlayerController>()?.TakeDamage(damage);
            Debug.Log($"{name} HIT PLAYER with Melee");
        }
    }

    public override void TakeDamage(int dmg)
    {
        if (isImmune)
        {
            DamagePopup.Create(transform.position + Vector3.up * 2, "IMMUNE", Color.gray);
            return;
        }

        base.TakeDamage(dmg);

        CheckPhaseTransition();
    }

    void CheckPhaseTransition()
    {
        float hpPercent = (float)currentHP / maxHP * 100f;

        // Phase 1 -> Phase 2 (Summoning)
        if (currentPhase == BossPhase.Phase1 && hpPercent <= phase2Threshold)
        {
            StartSummoningPhase();
        }
    }

    void StartSummoningPhase()
    {
        if (currentPhase != BossPhase.Phase1) return; // Safety check

        currentPhase = BossPhase.Phase2;
        currentState = State.Summoning;
        isSummoning = true;
        isImmune = true;
        
        anim.SetBool("isImmune", true);
        anim.SetTrigger("Summon"); // Or ArmorBuff

        DamagePopup.Create(transform.position + Vector3.up * 3, "SHIELD UP!", Color.cyan);
        Debug.Log("Boss: Entering Phase 2 (Summoning). Shield UP!");

        // Spawn Golem(s)
        if (minionPrefab != null && minionSpawnPoints.Length > 0)
        {
            foreach (Transform sp in minionSpawnPoints)
            {
                if (sp == null) continue;
                GameObject minion = Instantiate(minionPrefab, sp.position, Quaternion.identity);
                EnemyController ec = minion.GetComponent<EnemyController>();
                if (ec != null) activeMinions.Add(ec);
            }
            Debug.Log($"Boss: Spawned {activeMinions.Count} minions.");
        }
        else
        {
            Debug.LogWarning("Boss: No Minion Prefab or Spawn Points assigned! Phase 2 will end immediately.");
        }
    }

    void CheckMinions()
    {
        // Remove dead minions
        activeMinions.RemoveAll(m => m == null || m.gameObject == null || m.IsDefeated);

        if (activeMinions.Count == 0)
        {
            Debug.Log("Boss: All minions defeated. Ending Phase 2.");
            EndSummoningPhase();
        }
    }

    void EndSummoningPhase()
    {
        isSummoning = false;
        isImmune = false;
        anim.SetBool("isImmune", false);
        
        // Enter Phase 3
        currentPhase = BossPhase.Phase3;
        isEnraged = true;
        anim.SetBool("isEnraged", true);
        
        currentState = State.Idle;
        DamagePopup.Create(transform.position + Vector3.up * 3, "ENRAGED!", Color.red);
    }
    // =============================
    // DEATH EVENT
    // =============================
    public event System.Action OnDeath;

    protected override void Die()
    {
        OnDeath?.Invoke();
        base.Die();
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        if (firePoint != null) Gizmos.DrawWireSphere(firePoint.position, 0.2f);
        
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, minRangeAttackDistance);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange); // Engagement Range (Blue?)
        
        Gizmos.color = Color.cyan;
        if (attackPoint != null)
            Gizmos.DrawWireSphere(attackPoint.position, meleeHitRange); // Actual Punch Size
    }
}
