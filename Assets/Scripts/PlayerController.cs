using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{


    [Header("Form Stats")]
    public float moveSpeed_Movement = 8f;
    public float jumpForce_Movement = 16f;

    public float moveSpeed_Attack = 4f;
    public float jumpForce_Attack = 10f;

    public bool inputLocked = false;




    [Header("Player Health")]
    public int maxHP = 100;
    public int currentHP;
    bool isDead = false;

    // Event for death (subscribed by PlayerOverpowerAbility etc.)
    public event System.Action OnPlayerDeath;

    [Header("Skill Unlocks")]
    public bool dashUnlocked = false;
    public bool powerShotUnlocked = false;
    public bool wallSlideUnlocked = false;  // Wall slide skill for Movement Cat only
    public bool doubleJumpUnlocked = false;  // Double jump for Movement Cat only
    public bool meleeAttackUnlocked = false;  // Melee attack for Attack Cat
    public bool meleeComboUnlocked = false;   // Melee combo (Melee_Attack2) for Attack Cat


    [Header("Movement")]
    public float moveSpeed;
    public float jumpForce;

    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundRadius = 0.2f;
    public LayerMask groundLayer;

    [Header("Wall Check")]
    public Transform wallCheckLeft;
    public Transform wallCheckRight;
    public float wallCheckDistance = 0.1f;  // Kept for legacy or reference
    public Vector2 wallCheckBoxSize = new Vector2(0.2f, 0.8f); // Width, Height

    bool isTouchingWall;
    bool isTouchingLeftWall;
    bool isTouchingRightWall;
    float wallJumpCooldown = 0f;  // Cooldown to prevent re-grabbing wall immediately after wall jump



    [Header("Combat")]
    public Transform firePoint;
    public float attackCooldown = 0.4f;
    private float attackTimer = 0f;
    private bool comboWindowActive = false;
    
    // Ranged
    public GameObject projectilePrefab;
    public float projectileSpeed = 10f;
    public float fireRate = 0.5f;
    public int rangedDamage = 12; // Scalable ranged damage

    [Header("Melee Attack")]
    public Transform meleeAttackPoint;
    public float meleeAttackRange = 1.2f;
    public int meleeDamage = 25;
    public int meleeComboDamage = 45;
    public LayerMask enemyLayer;
    [Header("Character Switching")]
    public GameObject movementCatPrefab;
    public GameObject attackCatPrefab;

    [Header("Dash (Movement Form Only)")]
    public float dashSpeed = 20f;
    public float dashDuration = 0.15f;
    public float dashCooldown = 1f;

    [Header("Wall Slide")]
    public bool wallSlideEnabled = true;
    public float wallSlideSpeed = 2f;
    public float wallJumpForceX = 12f;
    public float wallJumpForceY = 16f;

    [Header("Hit Feedback")]
    public float invincibleTime = 0.6f;
    public float flashSpeed = 0.08f;



    bool isInvincible = false;
    private Coroutine flashCoroutine;

    public PlayerFormUI formUI;

    bool isDashing = false;
    float dashTimer;
    float nextDashTime;

    [Header("Double Jump VFX")]
    public GameObject doubleJumpVFX;  // Smoke/dust sprite animation prefab

    private int jumpCount = 0;  // Track number of jumps used
    private bool isWallJumping = false; // prevents input from overriding wall jump velocity


    private GameObject currentCat;
    private bool isAttackMode = false;
    private bool isSwitching = false;


    private Rigidbody2D rb;
    private Animator anim;
    private SpriteRenderer sr;

    private BoxCollider2D col;
    private Vector2 originalColliderSize;
    private Vector2 originalColliderOffset;


    private bool isGrounded;


    void Start()
    {

        // Start in Movement form
        moveSpeed = moveSpeed_Movement;
        jumpForce = jumpForce_Movement;
        // Initialize HP from PlayerProgress if available
        if (PlayerProgress.currentHP > 0)
        {
            maxHP = PlayerProgress.maxHP;
            currentHP = PlayerProgress.currentHP;
            Debug.Log($"[Player] Loaded Persisted Data | HP: {currentHP}/{maxHP}");
        }
        else
        {
            // If currentHP is 0 (death) or uninitialized, reset to Max
            maxHP = PlayerProgress.maxHP > 0 ? PlayerProgress.maxHP : 100; // Keep MaxHP upgrade if exists
            currentHP = maxHP;
            
            PlayerProgress.maxHP = maxHP;
            PlayerProgress.currentHP = currentHP;
            Debug.Log("[Player] Respawned/New Game | HP Reset to Max");
        }

        col = GetComponent<BoxCollider2D>();
        originalColliderSize = col.size;
        originalColliderOffset = col.offset;

        rb = GetComponent<Rigidbody2D>();

        // Start in Movement Mode
        movementCatPrefab.SetActive(true);
        attackCatPrefab.SetActive(false);

        anim = movementCatPrefab.GetComponent<Animator>();
        sr = movementCatPrefab.GetComponent<SpriteRenderer>();

        // ✅ APPLY PERSISTED SKILLS
        dashUnlocked = PlayerProgress.dashUnlocked;
        powerShotUnlocked = PlayerProgress.powerShotUnlocked;
        wallSlideUnlocked = PlayerProgress.wallSlideUnlocked;
        doubleJumpUnlocked = PlayerProgress.doubleJumpUnlocked;
        meleeAttackUnlocked = PlayerProgress.meleeAttackUnlocked;
        meleeComboUnlocked = PlayerProgress.meleeComboUnlocked;

        Debug.Log($"[Player] Skills Loaded | Dash: {dashUnlocked}, PowerShot: {powerShotUnlocked}, WallSlide: {wallSlideUnlocked}, DoubleJump: {doubleJumpUnlocked}, Melee: {meleeAttackUnlocked}, Combo: {meleeComboUnlocked}");
    }


    void Update()
    {

        if (inputLocked)
        {
            // Stop horizontal movement cleanly
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            UpdateAnimations();
            return;
        }

        Move();
        Jump();
        WallSlideAndJump();
        Attack();
        Spin();
        Duck();
        Dash();
        UpdateAnimations();

    }

    void FixedUpdate()
    {
        CheckGrounded();
        CheckWall();
    }

    void Move()
    {
        if (isDashing) return;
        if (isWallJumping) return; // Don't process movement input during wall jump lock (preserves momentum)

        float move = Input.GetAxisRaw("Horizontal");

        // Prevent sticking to wall (due to physics friction) when pushing INTO it while airborne
        if (!isGrounded && isTouchingWall)
        {
            // Check if player is trying to move INTO the wall
            bool pushingIntoWall = (isTouchingLeftWall && move < 0) || (isTouchingRightWall && move > 0);

            if (pushingIntoWall)
            {
                // Stop horizontal movement so we don't stick to the wall's friction
                // We still let vertical velocity happen (regulated by WallSlide or Gravity)
                rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
                return;
            }
        }

        if (!anim.GetBool("isDucking"))
            rb.linearVelocity = new Vector2(move * moveSpeed, rb.linearVelocity.y);
        else
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);

        if (move != 0)
        {
            sr.flipX = move < 0;
            UpdateFirePoint();
            UpdateMeleeAttackPoint();
        }
    }




    public int GetHP()
    {
        return currentHP;
    }

    public void TakeDamage(int dmg)
    {
        if (isDead || isInvincible) return;

        // Shield check: if a shield is active, it absorbs this hit and breaks.
        PlayerPowerupBuffer buffer = GetComponent<PlayerPowerupBuffer>();
        if (buffer != null && buffer.shieldActive)
        {
            // Consume shield without reducing HP
            buffer.shieldActive = false;

            // Optional: play shield break VFX / SFX here
            Debug.Log("[Player] Shield absorbed damage!");
            return;
        }

        int hpBefore = currentHP;
        currentHP -= dmg;
        currentHP = Mathf.Max(currentHP, 0);  // Clamp to 0
        PlayerProgress.currentHP = currentHP; // Save state
        
        float hpPercent = (float)currentHP / maxHP * 100f;
        Debug.Log($"[Player] DAMAGE TAKEN: {dmg} | HP: {hpBefore} -> {currentHP}/{maxHP} ({hpPercent:F0}%)");

        FindObjectOfType<PlayerHealthUI>()?.PlayDamageEffect();

        // Camera shake on damage
        if (CameraShake.Instance != null)
            CameraShake.Instance.Shake(0.15f, 0.3f);

        anim.SetTrigger("Hit");

        float dir = sr.flipX ? -1f : 1f;
        rb.linearVelocity = new Vector2(dir * 5f, 4f);

        if (flashCoroutine != null)
        {
            StopCoroutine(flashCoroutine);
            sr.color = Color.white; // Reset immediately
        }
        flashCoroutine = StartCoroutine(HitFlash());

        if (currentHP <= 0)
        {
            Debug.Log("[Player] HP DEPLETED - DYING!");
            Die();
        }
    }

    public void IncreaseMaxHP(int amount)
    {
        int oldMaxHP = maxHP;
        int oldCurrentHP = currentHP;
        
        maxHP += amount;          // Increase max HP
        currentHP += amount;       // Heal player by the same amount
        currentHP = Mathf.Min(currentHP, maxHP);  // Cap at new max (safety)
        
        // Save state
        PlayerProgress.maxHP = maxHP;
        PlayerProgress.currentHP = currentHP;
        
        Debug.Log($"[Player] Max HP increased! {oldMaxHP} → {maxHP} | HP: {oldCurrentHP} → {currentHP} (+{amount} HP healed)");
    }


    void Die()
    {
        if (isDead) return;

        isDead = true;
        anim.SetTrigger("Death");
        
        // Notify listeners
        OnPlayerDeath?.Invoke();

        rb.linearVelocity = Vector2.zero;
        rb.simulated = false;

        // Disable visuals & controls instead of destroying
        movementCatPrefab.SetActive(false);
        attackCatPrefab.SetActive(false);

        inputLocked = true;

        // Delay game over to let health UI animation complete
        StartCoroutine(DelayedGameOver());
    }
    
    IEnumerator DelayedGameOver()
    {
        // Wait for UI to animate to 0 (unscaled so it works even if time is paused)
        yield return new WaitForSecondsRealtime(0.8f);
        GameOverManager.Instance.ShowGameOver();
    }





    void Jump()
    {
        // Ground jump
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            anim.SetTrigger("Jump");
            jumpCount = 1;  // First jump used
        }
        // Double jump (air jump)
        else if (Input.GetButtonDown("Jump") && !isGrounded && doubleJumpUnlocked 
                 && !isAttackMode && jumpCount < 2 && !isTouchingWall)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            anim.SetTrigger("Jump");
            jumpCount = 2;  // Second jump used
            
            // Spawn smoke/dust VFX
            if (doubleJumpVFX != null)
            {
                Instantiate(doubleJumpVFX, transform.position, Quaternion.identity);
            }
            
            Debug.Log("🚀 DOUBLE JUMP!");
        }
    }

    void WallSlideAndJump()
    {
        // Wall slide disabled for Attack Cat (black cat)
        if (isAttackMode) return;
        
        // Wall slide requires unlock for Movement Cat (white cat)
        if (!wallSlideUnlocked) return;
        
        if (!wallSlideEnabled) return;

        // Decrease wall jump cooldown
        if (wallJumpCooldown > 0)
        {
            wallJumpCooldown -= Time.deltaTime;
        }

        // Get horizontal input
        float horizontalInput = Input.GetAxis("Horizontal");

        // Check if player is pressing toward the wall
        bool pressingTowardWall = false;
        if (isTouchingLeftWall && horizontalInput < -0.1f) // Pressing left into left wall
        {
            pressingTowardWall = true;
        }
        else if (isTouchingRightWall && horizontalInput > 0.1f) // Pressing right into right wall
        {
            pressingTowardWall = true;
        }

        // Wall slide: only when actively pressing toward the wall
        // Don't wall slide if we just wall jumped (cooldown active)
        if (isTouchingWall && !isGrounded && rb.linearVelocity.y < 0 && wallJumpCooldown <= 0 && pressingTowardWall)
        {
            // Fix sprite flip based on which wall we're touching
            if (isTouchingLeftWall)
            {
                sr.flipX = false; // Face right when on left wall
            }
            else if (isTouchingRightWall)
            {
                sr.flipX = true; // Face left when on right wall
            }
            
            // Directly set velocity for instant response (not just clamping)
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, -wallSlideSpeed);
        }
        // If touching wall but NOT pressing toward it, allow normal fall (no wall slide)
        // This ensures instant release when player lets go of the key

        // Wall jump: jump away from wall with diagonal trajectory
        // Only allow if not in cooldown
        if (isTouchingWall && !isGrounded && Input.GetButtonDown("Jump") && wallJumpCooldown <= 0)
        {
            // Determine which direction to jump based on which wall we're touching
            float jumpDirX;
            if (isTouchingLeftWall)
            {
                jumpDirX = 1f;  // Jump right from left wall
            }
            else // isTouchingRightWall
            {
                jumpDirX = -1f; // Jump left from right wall
            }
            
            // Apply diagonal jump force (horizontal + vertical for platformer feel)
            rb.linearVelocity = new Vector2(jumpDirX * wallJumpForceX, wallJumpForceY);
            anim.SetTrigger("Jump");
            
            // Set cooldown to prevent immediately re-grabbing the wall
            wallJumpCooldown = 0.3f;
            
            // Lock input briefly to prevent player from fighting against wall jump
            StartCoroutine(WallJumpInputLock(0.2f));
            
            Debug.Log($"Wall Jump! Direction: X={jumpDirX * wallJumpForceX}, Y={wallJumpForceY}");
        }
    }
    
    IEnumerator WallJumpInputLock(float duration)
    {
        isWallJumping = true; // Disable horizontal input
        yield return new WaitForSeconds(duration);
        isWallJumping = false; // Re-enable input
    }



    void Attack()
    {
        if (!isAttackMode) return;          // must be Attack Cat

        attackTimer -= Time.deltaTime;

        // Update combo window based on animation state
        UpdateComboWindow();

        // Range Attack (Fire1 / Left Click)
        if (powerShotUnlocked && Input.GetButtonDown("Fire1") && attackTimer <= 0)
        {
            attackTimer = attackCooldown;
            anim.SetTrigger("RangeAttack");
            comboWindowActive = false;  // Close combo window on range attack
            Debug.Log("Range Attack triggered!");
        }

        // Melee Attack (Fire2 / Right Click or dedicated key)
        if (meleeAttackUnlocked && Input.GetButtonDown("Fire2"))
        {
            // PRIORITY: Check for combo FIRST (using flag-based system)
            if (meleeComboUnlocked && comboWindowActive)
            {
                // Trigger combo (second hit) - ignore cooldown, we're in combo window
                anim.SetTrigger("MeleeAttack2");
                attackTimer = attackCooldown;
                comboWindowActive = false;  // Close window after combo
                Debug.Log("🔥 MELEE COMBO! (MeleeAttack2)");
            }
            // Only start new melee attack if cooldown expired
            else if (attackTimer <= 0)
            {
                // Trigger first melee attack
                attackTimer = attackCooldown;
                anim.SetTrigger("MeleeAttack");
                comboWindowActive = true;  // Open combo window
                Debug.Log("⚔️ Melee Attack triggered! Combo window OPEN");
            }
            else
            {
                Debug.Log("⏱️ Melee attack on cooldown, wait...");
            }
        }
    }

    void UpdateComboWindow()
    {
        // Check if we're in the Melee_Attack animation
        AnimatorStateInfo stateInfo = anim.GetCurrentAnimatorStateInfo(0);
        
        if (stateInfo.IsName("Melee_Attack"))
        {
            // Combo window: 50% to 100% of animation (wider window)
            if (stateInfo.normalizedTime >= 0.5f && stateInfo.normalizedTime < 1.0f)
            {
                // Keep window open
                if (!comboWindowActive)
                {
                    comboWindowActive = true;
                    Debug.Log("✅ Combo window OPENED at " + (stateInfo.normalizedTime * 100) + "%");
                }
            }
            else if (stateInfo.normalizedTime >= 1.0f)
            {
                // Animation finished, close window
                if (comboWindowActive)
                {
                    comboWindowActive = false;
                    Debug.Log("❌ Combo window CLOSED (animation ended)");
                }
            }
        }
        else if (comboWindowActive)
        {
            // Not in melee attack animation, close window
            comboWindowActive = false;
            Debug.Log("❌ Combo window CLOSED (left melee state)");
        }
    }



    public void ShootProjectile()
    {
        if (!isAttackMode) return;

        float dir = sr.flipX ? -1f : 1f;

        GameObject p = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);

        // Set damage dynamically
        Projectile projScript = p.GetComponent<Projectile>();
        if (projScript != null)
        {
            projScript.damage = rangedDamage;
        }

        Rigidbody2D prb = p.GetComponent<Rigidbody2D>();
        prb.linearVelocity = new Vector2(dir * projectileSpeed, 0);
    }

    // Called by animation event at hit frame
    public void DealMeleeDamage()
    {
        if (!isAttackMode || meleeAttackPoint == null) return;

        Collider2D[] hits = Physics2D.OverlapCircleAll(
            meleeAttackPoint.position,
            meleeAttackRange,
            enemyLayer
        );

        if (hits.Length > 0)
        {
            foreach (Collider2D hit in hits)
            {
                EnemyController enemy = hit.GetComponent<EnemyController>();
                if (enemy != null)
                {
                    enemy.TakeDamage(meleeDamage);
                    Debug.Log($"MELEE HIT! Dealt {meleeDamage} damage to {hit.name}");
                }
            }
        }
        else
        {
            Debug.Log("MELEE MISSED");
        }
    }

    // Called by animation event at hit frame for COMBO attack
    public void DealMeleeComboDamage()
    {
        if (!isAttackMode || meleeAttackPoint == null) return;

        Collider2D[] hits = Physics2D.OverlapCircleAll(
            meleeAttackPoint.position,
            meleeAttackRange,
            enemyLayer
        );

        if (hits.Length > 0)
        {
            foreach (Collider2D hit in hits)
            {
                EnemyController enemy = hit.GetComponent<EnemyController>();
                if (enemy != null)
                {
                    enemy.TakeDamage(meleeComboDamage);
                    Debug.Log($"COMBO HIT! Dealt {meleeComboDamage} damage to {hit.name}");
                }
            }
        }
    }


    void Dash()
    {
        if (!dashUnlocked) return;   // 🔒 LOCKED UNTIL UNLOCKED
        if (isAttackMode) return;
        if (Time.time < nextDashTime) return;

        if (!isDashing && Input.GetKeyDown(KeyCode.LeftShift))
        {
            StartCoroutine(DoDash());
        }
    }



    void Duck()
    {
        bool ducking = Input.GetKey(KeyCode.S);

        anim.SetBool("isDucking", ducking);

        if (ducking)
        {
            col.size = new Vector2(originalColliderSize.x, originalColliderSize.y * 0.6f);
            col.offset = new Vector2(originalColliderOffset.x, originalColliderOffset.y - originalColliderSize.y * 0.2f);
        }
        else
        {
            col.size = originalColliderSize;
            col.offset = originalColliderOffset;
        }
    }



    void UpdateAnimations()
    {
        anim.SetBool("isRunning", Mathf.Abs(rb.linearVelocity.x) > 0.1f);
        anim.SetBool("isGrounded", isGrounded);
        anim.SetBool("isTouchingWall", isTouchingWall);
        anim.SetBool("isDead", isDead);
        anim.SetBool("isDashing", isDashing);

        float yVel = rb.linearVelocity.y;

        // Prevent tiny negative gravity values from triggering fall animation while grounded
        if (isGrounded && yVel < 0.5f)
            yVel = 0f;

        anim.SetFloat("yVelocity", yVel);
    }


    void Spin()
    {
        // Can switch forms in mid-air or on ground
        if (Input.GetKeyDown(KeyCode.E) && !isSwitching)
        {
            StartCoroutine(SwitchCharacter());
        }
    }







    void UpdateFirePoint()
    {
        if (firePoint == null) return;

        Vector3 local = firePoint.localPosition;
        local.x = Mathf.Abs(local.x) * (sr.flipX ? -1 : 1);
        firePoint.localPosition = local;
    }

    void UpdateMeleeAttackPoint()
    {
        if (meleeAttackPoint == null) return;

        Vector3 local = meleeAttackPoint.localPosition;
        local.x = Mathf.Abs(local.x) * (sr.flipX ? -1 : 1);
        meleeAttackPoint.localPosition = local;
    }


    IEnumerator SwitchCharacter()
    {
        isSwitching = true;

        anim.SetTrigger("Spin");

        yield return new WaitForSeconds(0.25f);

        isAttackMode = !isAttackMode;

        movementCatPrefab.SetActive(!isAttackMode);
        attackCatPrefab.SetActive(isAttackMode);

        // Switch animator reference
        if (isAttackMode)
        {
            anim = attackCatPrefab.GetComponent<Animator>();
            moveSpeed = moveSpeed_Attack;
            jumpForce = jumpForce_Attack;
        }
        else
        {
            anim = movementCatPrefab.GetComponent<Animator>();
            moveSpeed = moveSpeed_Movement;
            jumpForce = jumpForce_Movement;
        }


        sr = anim.GetComponent<SpriteRenderer>();

        isSwitching = false;
    }

    IEnumerator DoDash()
    {
        isDashing = true;
        nextDashTime = Time.time + dashCooldown;

        // play animation first
        anim.SetTrigger("Dash");

        yield return null; // wait 1 frame so animation starts

        float originalGravity = rb.gravityScale;
        rb.gravityScale = 0;

        float dir = sr.flipX ? -1f : 1f;
        rb.linearVelocity = new Vector2(dir * dashSpeed, 0);

        yield return new WaitForSeconds(dashDuration);

        rb.gravityScale = originalGravity;
        isDashing = false;
    }

    IEnumerator HitFlash()
    {
        isInvincible = true;

        for (int i = 0; i < 6; i++)
        {
            sr.color = new Color(1, 1, 1, 0.3f);
            yield return new WaitForSeconds(flashSpeed);
            sr.color = Color.white;
            yield return new WaitForSeconds(flashSpeed);
        }

        isInvincible = false;
        flashCoroutine = null;
        sr.color = Color.white; // Double safety
    }

    void CheckGrounded()
    {
        // Use OverlapCircle for more reliable ground detection
        Collider2D hit = Physics2D.OverlapCircle(
            groundCheck.position,
            groundRadius,
            groundLayer
        );

        bool wasGrounded = isGrounded;
        isGrounded = hit != null;
        
        // Reset jump count when landing
        if (isGrounded && !wasGrounded)
        {
            jumpCount = 0;
        }
    }

    // Add inside PlayerController class
    public void Heal(int amount)
    {
        if (isDead) return;

        currentHP = Mathf.Min(currentHP + amount, maxHP);
        PlayerProgress.currentHP = currentHP; // Save state

        // Update heart UI (your existing PlayerHealthUI polls GetHP in Update,
        // but call PlayDamageEffect or an update if you prefer immediate feedback)
        PlayerHealthUI ui = FindObjectOfType<PlayerHealthUI>();
        if (ui != null)
            ui.PlayDamageEffect(); // optional visual shake
    }

    void CheckWall()
    {
        // Use OverlapBox for better reliability than single raycast
        Collider2D hitLeft = Physics2D.OverlapBox(
            wallCheckLeft.position,
            wallCheckBoxSize,
            0f,
            groundLayer
        );

        Collider2D hitRight = Physics2D.OverlapBox(
            wallCheckRight.position,
            wallCheckBoxSize,
            0f,
            groundLayer
        );

        // Track which wall we're touching
        isTouchingLeftWall = hitLeft != null;
        isTouchingRightWall = hitRight != null;
        isTouchingWall = isTouchingLeftWall || isTouchingRightWall;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        if (meleeAttackPoint != null)
        {
            Gizmos.DrawWireSphere(meleeAttackPoint.position, meleeAttackRange);
        }

        Gizmos.color = Color.blue;
        if (groundCheck != null)
            Gizmos.DrawWireSphere(groundCheck.position, groundRadius);

        Gizmos.color = Color.yellow;
        if (wallCheckLeft != null)
            Gizmos.DrawWireCube(wallCheckLeft.position, wallCheckBoxSize);
        
        if (wallCheckRight != null)
            Gizmos.DrawWireCube(wallCheckRight.position, wallCheckBoxSize);
    }





}


