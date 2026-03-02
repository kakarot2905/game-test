using UnityEngine;
using System;

/// <summary>
/// Singleton managing player experience points and level ups.
/// Awards skill points via SkillTreeManager when XP bar fills.
/// </summary>
public class ExperienceManager : MonoBehaviour
{
    public static ExperienceManager Instance { get; private set; }
    
    [Header("XP Settings")]
    public int xpPerLevel = 20;
    public int skillPointsPerLevel = 1;
    
    [Header("Current State")]
    public int currentXP = 0;
    public int totalXPEarned = 0;
    
    // Event for UI to subscribe to
    public event Action<int, int> OnXPChanged;  // currentXP, xpPerLevel
    public event Action<int> OnLevelUp;         // skill points earned
    
    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void Start()
    {
        // Load persist state
        currentXP = PlayerProgress.currentXP;
        totalXPEarned = PlayerProgress.totalXPEarned;
        
        Debug.Log($"[XP] Loaded Persistence | XP: {currentXP}, Total: {totalXPEarned}");
        OnXPChanged?.Invoke(currentXP, xpPerLevel);
    }
    
    /// <summary>
    /// Add XP to the player. Called when XP particles are absorbed.
    /// </summary>
    public void GainXP(int amount)
    {
        currentXP += amount;
        totalXPEarned += amount;
        
        // Save persist state
        PlayerProgress.currentXP = currentXP;
        PlayerProgress.totalXPEarned = totalXPEarned;
        
        Debug.Log($"[XP] Gained {amount} XP. Progress: {currentXP}/{xpPerLevel}");
        
        // Check for level up
        while (currentXP >= xpPerLevel)
        {
            currentXP -= xpPerLevel;
            PlayerProgress.currentXP = currentXP; // Save after level deduction
            LevelUp();
        }
        
        // Notify UI
        OnXPChanged?.Invoke(currentXP, xpPerLevel);
    }
    
    void LevelUp()
    {
        // Award skill points
        if (SkillTreeManager.Instance != null)
        {
            SkillTreeManager.Instance.GivePoints(skillPointsPerLevel);
            Debug.Log($"[XP] LEVEL UP! Awarded {skillPointsPerLevel} skill point(s)");
        }
        else
        {
            Debug.LogWarning("[XP] SkillTreeManager not found - could not award skill points");
        }
        
        OnLevelUp?.Invoke(skillPointsPerLevel);
    }
    
    /// <summary>
    /// Get current XP progress as a 0-1 value for UI.
    /// </summary>
    public float GetXPProgress()
    {
        return (float)currentXP / xpPerLevel;
    }
}
