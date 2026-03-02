using System;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public enum Skill
{
    PowerShot,
    Dash,
    WallSlide,      // Wall slide for Movement Cat
    DoubleJump,     // Double jump for Movement Cat
    MeleeAttack,    // Melee attack for Attack Cat
    MeleeCombo,     // Melee combo (second hit) for Attack Cat
    EnergyShield,
    SwiftStrike,
    CosmicBurst,
    PhaseShift,
    PrecisionAim,
    TwinStrike,
    IncreaseMaxHP,
    IncreaseHealSlot,   // reserved
    IncreaseShieldSlot  // reserved
}

public class SkillTreeManager : MonoBehaviour
{
    public static SkillTreeManager Instance { get; private set; }

    [Header("UI")]
    public GameObject skillTreeCanvas;    // assign your SkillTreeCanvas
    public TextMeshProUGUI pointsText;    // assign a TMP text in the skill tree showing points
    public TextMeshProUGUI messageText;   // optional small message for "coming soon" / not enough points

    [Header("Player")]
    public PlayerController player;       // assign player (or Find in Awake)

    [Header("State")]
    public int skillPoints = 0;
    public List<Skill> unlocked = new List<Skill>();

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(this); return; }
        Instance = this;

        if (player == null)
            player = FindObjectOfType<PlayerController>();

        // We load in Awake to ensure data is ready before UI Start() runs
        skillPoints = PlayerProgress.skillPoints;
        LoadSkillsFromPersistence();
        Debug.Log($"[SkillTree] Loaded Persistence in Awake | Points: {skillPoints}");
    }

    void Start()
    {
        UpdatePointsUI();
    }

    void LoadSkillsFromPersistence()
    {
        // Clear list and reload from static data
        unlocked.Clear();

        if (PlayerProgress.dashUnlocked) unlocked.Add(Skill.Dash);
        if (PlayerProgress.powerShotUnlocked) unlocked.Add(Skill.PowerShot);
        if (PlayerProgress.wallSlideUnlocked) unlocked.Add(Skill.WallSlide);
        if (PlayerProgress.doubleJumpUnlocked) unlocked.Add(Skill.DoubleJump);
        if (PlayerProgress.meleeAttackUnlocked) unlocked.Add(Skill.MeleeAttack);
        if (PlayerProgress.meleeComboUnlocked) unlocked.Add(Skill.MeleeCombo);
        
        // Note: Stat upgrades like MaxHP don't need to be in the list for logic, 
        // but if we want UI to show them as unlocked, we should add them too if we track them.
        // Currently PlayerProgress only tracks the booleans for abilities.
    }

    public void GivePoints(int amount)
    {
        skillPoints += amount;
        PlayerProgress.skillPoints = skillPoints; // Save state
        Debug.Log($"[SkillTree] Given {amount} points. Total = {skillPoints}");
        UpdatePointsUI();
    }

    void UpdatePointsUI()
    {
        if (pointsText != null)
            pointsText.text = $"Points: {skillPoints}";
    }

    public bool IsUnlocked(Skill s)
    {
        return unlocked.Contains(s);
    }

    // Called by SkillButton (UI)
    public void OnSkillClicked(Skill s)
    {
        // Already unlocked -> show message
        if (IsUnlocked(s))
        {
            ShowMessage("Already unlocked");
            return;
        }

        // Only implement unlocking for implemented skills; others: show coming soon
        if (s != Skill.PowerShot && s != Skill.Dash && s != Skill.WallSlide && s != Skill.MeleeAttack && s != Skill.MeleeCombo && s != Skill.DoubleJump && s != Skill.IncreaseMaxHP)
        {
            ShowMessage("Coming soon");
            return;
        }

        // Check prerequisites
        if (!CheckPrerequisites(s, out string prerequisiteMessage))
        {
            ShowMessage(prerequisiteMessage);
            return;
        }

        // Get cost from SkillNodeTooltip component
        int cost = GetSkillCost(s);

        // Check if player has enough points
        if (skillPoints < cost)
        {
            ShowMessage($"Not enough skill points (need {cost})");
            return;
        }

        // Spend points and unlock
        skillPoints -= cost;
        PlayerProgress.skillPoints = skillPoints; // Save state
        unlocked.Add(s);
        ApplySkillToPlayer(s);

        UpdatePointsUI();
        ShowMessage($"{s} unlocked!");
        Debug.Log($"[SkillTree] Unlocked {s} for {cost} points");
    }

    int GetSkillCost(Skill s)
    {
        // Find all SkillNodeTooltip components in the scene
        SkillNodeTooltip[] tooltips = FindObjectsOfType<SkillNodeTooltip>();
        
        foreach (var tooltip in tooltips)
        {
            // Check if this tooltip's button matches the skill
            SkillNodeButton button = tooltip.GetComponent<SkillNodeButton>();
            if (button != null && button.skill == s)
            {
                Debug.Log($"[SkillTree] Found cost for {s}: {tooltip.cost} points");
                return tooltip.cost;
            }
        }

        // Default to 1 if not found
        Debug.LogWarning($"[SkillTree] No tooltip found for {s}, defaulting to cost 1");
        return 1;
    }

    bool CheckPrerequisites(Skill s, out string message)
    {
        message = "";
        
        // Define layers based on the skill tree structure
        // Layer 1: Dash, Power Shot
        // Layer 2: Wall Slide, Max HP, Melee Attack
        // Layer 3: Double Jump, Heal+, Shield+, Melee Combo
        // Layer 4: Air Dash, Regen, Homing
        
        switch (s)
        {
            // ===== LAYER 1 (No prerequisites) =====
            case Skill.Dash:
            case Skill.PowerShot:
                return true;
            
            // ===== LAYER 2 (Requires ALL of Layer 1) =====
            case Skill.WallSlide:
            case Skill.IncreaseMaxHP:
            case Skill.MeleeAttack:
                if (!IsUnlocked(Skill.Dash))
                {
                    message = "Complete Layer 1 first: Requires Dash";
                    return false;
                }
                if (!IsUnlocked(Skill.PowerShot))
                {
                    message = "Complete Layer 1 first: Requires Power Shot";
                    return false;
                }
                return true;
            
            // ===== LAYER 3 (Requires ALL of Layer 2) =====
            case Skill.DoubleJump:
            case Skill.MeleeCombo:
            case Skill.IncreaseHealSlot:    // Heal+
            case Skill.IncreaseShieldSlot:  // Shield+
                // Check Layer 1 complete
                if (!IsUnlocked(Skill.Dash) || !IsUnlocked(Skill.PowerShot))
                {
                    message = "Complete Layer 1 first";
                    return false;
                }
                // Check Layer 2 complete
                if (!IsUnlocked(Skill.WallSlide))
                {
                    message = "Complete Layer 2: Requires Wall Slide";
                    return false;
                }
                if (!IsUnlocked(Skill.IncreaseMaxHP))
                {
                    message = "Complete Layer 2: Requires Max HP";
                    return false;
                }
                if (!IsUnlocked(Skill.MeleeAttack))
                {
                    message = "Complete Layer 2: Requires Melee Attack";
                    return false;
                }
                return true;
            
            // ===== LAYER 4 (Requires ALL of Layer 3) =====
            // Air Dash, Regen, Homing - coming soon
            
            default:
                // Unknown skill - no prerequisites
                return true;
        }
    }

    void ApplySkillToPlayer(Skill s)
    {
        if (player == null) return;

        switch (s)
        {
            case Skill.Dash:
                player.dashUnlocked = true;

                // ✅ STEP 2 — SAVE GLOBALLY
                PlayerProgress.dashUnlocked = true;
                break;

            case Skill.PowerShot:
                player.powerShotUnlocked = true;

                // ✅ STEP 2 — SAVE GLOBALLY
                PlayerProgress.powerShotUnlocked = true;
                break;

            case Skill.WallSlide:
                player.wallSlideUnlocked = true;

                // ✅ STEP 2 — SAVE GLOBALLY
                PlayerProgress.wallSlideUnlocked = true;
                break;

            case Skill.DoubleJump:
                player.doubleJumpUnlocked = true;

                // ✅ STEP 2 — SAVE GLOBALLY
                PlayerProgress.doubleJumpUnlocked = true;
                break;

            case Skill.MeleeAttack:
                player.meleeAttackUnlocked = true;

                // ✅ STEP 2 — SAVE GLOBALLY
                PlayerProgress.meleeAttackUnlocked = true;
                break;

            case Skill.MeleeCombo:
                player.meleeComboUnlocked = true;

                // ✅ STEP 2 — SAVE GLOBALLY
                PlayerProgress.meleeComboUnlocked = true;
                break;

            case Skill.IncreaseMaxHP:
                // Increase max HP by 20 (bonus heart in new UI system)
                player.IncreaseMaxHP(20);
                
                Debug.Log("[SkillTree] Max HP increased! New max: " + player.maxHP);
                break;
        }
    }



    void ShowMessage(string msg)
    {
        if (messageText != null)
        {
            messageText.text = msg;
            CancelInvoke(nameof(ClearMessage));
            Invoke(nameof(ClearMessage), 2f);
        }
        Debug.Log($"[SkillTree] {msg}");
    }

    void ClearMessage()
    {
        if (messageText != null) messageText.text = "";
    }

    // Merchant friendly helper
    public void GivePointsFromMerchant(int amount)
    {
        GivePoints(amount);
        // optionally auto-open skill tree or show some UI
    }
}
