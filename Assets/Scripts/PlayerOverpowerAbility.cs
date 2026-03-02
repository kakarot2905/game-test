using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class PlayerOverpowerAbility : MonoBehaviour
{
    [Header("Dependencies")]
    public PlayerController player;
    public Volume globalVolume;
    public GameObject circleVisualPrefab; // Assign a simple white circle sprite prefab
    
    [Header("Audio")]
    [Header("Audio")]
    public AudioClip opMusicClip; // Assign your OP music file here
    
    [Header("Health Drain")]
    public float hpDrainRate = 10f; // HP lost per second
    public Color opHealthBarColor = new Color(0.6f, 0f, 1f); // Purple
    
    private float hpDrainTimer;

    [Header("Settings")]
    public float maxScale = 50f; // Scale of the circle to cover the screen
    public float transitionDuration = 1.0f;

    [Header("Overpower Stats")]
    public float moveSpeedMultiplier = 2.0f;
    public float jumpForceMultiplier = 1.3f;

    public int attackDamageMultiplier = 10;
    public int rangedDamageMultiplier = 5; // New multiplier for projectiles
    
    // Internal State
    private bool isOverpowered = false;
    private bool isTransitioning = false;

    // Stored Original Values
    
    private float originalMoveSpeedMovement;
    private float originalJumpForceMovement;
    
    private int originalMeleeDamage;
    private int originalMeleeComboDamage;
    private int originalRangedDamage;
    
    // Stored Unlock States
    private bool storedDash;
    private bool storedPowerShot;
    private bool storedWallSlide;
    private bool storedDoubleJump;
    private bool storedMelee;
    private bool storedCombo;

    private GameObject currentCircle;
    private ColorAdjustments colorAdjustments;
    
    // Ghost Trails
    private GhostTrail ghostTrailMovement;
    private GhostTrail ghostTrailAttack;

    void Start()
    {
        if (player == null) player = GetComponent<PlayerController>();
        
        // Setup Ghost Trails on child objects
        if (player != null)
        {
            if (player.movementCatPrefab != null)
            {
                ghostTrailMovement = player.movementCatPrefab.AddComponent<GhostTrail>();
                ghostTrailMovement.enabled = false; 
            }
            
            if (player.attackCatPrefab != null)
            {
                ghostTrailAttack = player.attackCatPrefab.AddComponent<GhostTrail>();
                ghostTrailAttack.enabled = false;
            }
        }
        
        // Late hook because Start runs after OnEnable
        if (player != null)
        {
             player.OnPlayerDeath -= HandlePlayerDeath; // Prevent double sub
             player.OnPlayerDeath += HandlePlayerDeath;
        }

        if (globalVolume == null)
        {
            globalVolume = FindFirstObjectByType<Volume>();
            if (globalVolume == null)
            {
                Debug.LogError("[Overpower] No Global Volume found in scene! Black & White effect will NOT work.");
            }
        }

        // Try to FORCE ENABLE Post Processing on Main Camera
        if (Camera.main != null)
        {
            var camData = Camera.main.GetUniversalAdditionalCameraData();
            if (camData != null)
            {
                camData.renderPostProcessing = true;
                Debug.Log("[Overpower] Enforced Post Processing on Main Camera");
            }
        }

        // Try to get ColorAdjustments from volume
        if (globalVolume != null && globalVolume.profile != null)
        {
            if (!globalVolume.profile.TryGet(out colorAdjustments))
            {
                // Component doesn't exist?
                Debug.LogWarning("[Overpower] ColorAdjustments not found in Volume Profile. Grayscale effect may not work.");
            }
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P) && !isTransitioning)
        {
            if (isOverpowered)
            {
                // Manual Deactivate
                ToggleOverpower();
            }
            else if (player.currentHP > 1) 
            {
                // Only activate if we have > 1 HP to spend
                ToggleOverpower();
            }
        }
        
        // Health Drain Logic
        if (isOverpowered && !isTransitioning)
        {
            hpDrainTimer += Time.deltaTime;
            if (hpDrainTimer >= (1f / hpDrainRate))
            {
                hpDrainTimer = 0;
                player.currentHP--; // Direct modification to avoid "TakeDamage" shake/flash
                
                // Safety check: Auto-deactivate at 1 HP
                if (player.currentHP <= 1)
                {
                    player.currentHP = 1;
                    Deactivate();
                }
            }
        }
    }

    void OnEnable()
    {
        // We need player ref. If it's assigned in inspector, this works.
        // If assigned in Start, we might need to hook there too.
        if (player != null) 
        {
            player.OnPlayerDeath += HandlePlayerDeath;
        }
    }

    void OnDisable()
    {
        if (player != null)
        {
            player.OnPlayerDeath -= HandlePlayerDeath;
        }
    }

    void HandlePlayerDeath()
    {
        if (isOverpowered)
        {
            Deactivate();
        }
    }

    void ToggleOverpower()
    {
        if (isOverpowered)
            Deactivate();
        else
            Activate();
    }

    void Activate()
    {
        isOverpowered = true;
        
        // 1. Store Original Stats
        StoreOriginalStats();

        // 2. Apply Overpower Stats
        ApplyOverpowerStats();

        // 3. Unlock All Skills
        UnlockAllSkills(true);

        // 4. Trigger Visuals (Expand)
        StartCoroutine(AnimateTransition(true));

        // 5. Play Audio (via Music Manager)
        if (MusicManager.instance != null && opMusicClip != null)
        {
             MusicManager.instance.PlayOverrideMusic(opMusicClip);
        }
        
        // 6. Override Health Bar Color
        HealthBarUI healthUI = FindObjectOfType<HealthBarUI>();
        if (healthUI != null)
            healthUI.SetOverrideColor(opHealthBarColor);
            
        // 7. Enable Ghost Trails
        if (ghostTrailMovement != null) ghostTrailMovement.enabled = true;
        if (ghostTrailAttack != null) ghostTrailAttack.enabled = true;
        
        // 8. Spawn Status Text
        DamagePopup.Create(transform.position + Vector3.up, "Overpowered!", Color.yellow);
        
        Debug.Log("⚡ OVERPOWER MODE ACTIVATED! (Health draining...)");
    }

    void Deactivate()
    {
        isOverpowered = false;

        // 1. Restore Stats
        RestoreStats();

        // 2. Restore Skill States
        RestoreSkillStates();

        // 3. Trigger Visuals (Shrink)
        StartCoroutine(AnimateTransition(false));

        // 4. Stop Audio (Resume background)
        if (MusicManager.instance != null)
        {
             MusicManager.instance.StopOverrideMusic();
        }
        
        // 5. Clear Health Bar Color
        HealthBarUI healthUI = FindObjectOfType<HealthBarUI>();
        if (healthUI != null)
            healthUI.ClearOverrideColor();
            
        // 6. Disable Ghost Trails
        if (ghostTrailMovement != null) ghostTrailMovement.enabled = false;
        if (ghostTrailAttack != null) ghostTrailAttack.enabled = false;
        
        Debug.Log("💤 Overpower Mode Deactivated.");
    }

    void StoreOriginalStats()
    {
        // HP is NOT stored because it drains and isn't restored
        
        originalMoveSpeedMovement = player.moveSpeed_Movement;
        originalJumpForceMovement = player.jumpForce_Movement;

        originalMeleeDamage = player.meleeDamage;
        originalMeleeComboDamage = player.meleeComboDamage;
        originalRangedDamage = player.rangedDamage;

        // Store Flags
        storedDash = player.dashUnlocked;
        storedPowerShot = player.powerShotUnlocked;
        storedWallSlide = player.wallSlideUnlocked;
        storedDoubleJump = player.doubleJumpUnlocked;
        storedMelee = player.meleeAttackUnlocked;
        storedCombo = player.meleeComboUnlocked;
    }

    void ApplyOverpowerStats()
    {
        // NO HP MODIFICATION - Health Drain handled in Update
        
        // Movement Speed Buff
        player.moveSpeed_Movement *= moveSpeedMultiplier;
        player.jumpForce_Movement *= jumpForceMultiplier;

        // Apply immediately if in movement form
        if (player.moveSpeed == originalMoveSpeedMovement) 
            player.moveSpeed = player.moveSpeed_Movement;
            
        // Damage Buff
        player.meleeDamage *= attackDamageMultiplier;
        player.meleeComboDamage *= attackDamageMultiplier;
        player.rangedDamage *= rangedDamageMultiplier;
    }

    void UnlockAllSkills(bool state)
    {
        player.dashUnlocked = state;
        player.powerShotUnlocked = state;
        player.wallSlideUnlocked = state;
        player.doubleJumpUnlocked = state;
        player.meleeAttackUnlocked = state;
        player.meleeComboUnlocked = state;
    }

    void RestoreStats()
    {
        // NO HP RESTORE - Health spent is gone
        
        player.moveSpeed_Movement = originalMoveSpeedMovement;
        player.jumpForce_Movement = originalJumpForceMovement;

        // Reset current active speed if needed
        if (player.moveSpeed > originalMoveSpeedMovement)
            player.moveSpeed = player.moveSpeed_Movement;

        player.meleeDamage = originalMeleeDamage;
        player.meleeComboDamage = originalMeleeComboDamage;
        player.rangedDamage = originalRangedDamage;
    }

    void RestoreSkillStates()
    {
        player.dashUnlocked = storedDash;
        player.powerShotUnlocked = storedPowerShot;
        player.wallSlideUnlocked = storedWallSlide;
        player.doubleJumpUnlocked = storedDoubleJump;
        player.meleeAttackUnlocked = storedMelee;
        player.meleeComboUnlocked = storedCombo;
    }

    IEnumerator AnimateTransition(bool activating)
    {
        isTransitioning = true;
        
        // Spawn Volume/Circle if needed
        if (currentCircle == null && circleVisualPrefab != null)
        {
            currentCircle = Instantiate(circleVisualPrefab, transform.position, Quaternion.identity, transform);
            currentCircle.transform.localScale = Vector3.zero;
        }

        if (currentCircle != null) currentCircle.SetActive(true);

        float t = 0f;
        float startSat = activating ? 0f : -100f;
        float endSat = activating ? -100f : 0f;
        
        Vector3 startScale = activating ? Vector3.zero : Vector3.one * maxScale;
        Vector3 endScale = activating ? Vector3.one * maxScale : Vector3.zero;

        while (t < transitionDuration)
        {
            t += Time.deltaTime;
            float p = t / transitionDuration;

            // 1. Animate Saturation
            if (colorAdjustments != null)
                colorAdjustments.saturation.value = Mathf.Lerp(startSat, endSat, p);

            // 2. Animate Circle Scale
            if (currentCircle != null)
                currentCircle.transform.localScale = Vector3.Lerp(startScale, endScale, p);

            yield return null;
        }

        // Finalize
        if (colorAdjustments != null)
            colorAdjustments.saturation.value = endSat;
            
        if (!activating && currentCircle != null)
            currentCircle.SetActive(false);

        isTransitioning = false;
    }
}
