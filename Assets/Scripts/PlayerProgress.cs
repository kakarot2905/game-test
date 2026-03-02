public static class PlayerProgress
{
    // Skills
    public static bool dashUnlocked = false;
    public static bool powerShotUnlocked = false;
    public static bool wallSlideUnlocked = false;
    public static bool doubleJumpUnlocked = false;
    public static bool meleeAttackUnlocked = false;
    public static bool meleeComboUnlocked = false;

    // Stats
    public static int currentHP = -1; // -1 indicates not initialized
    public static int maxHP = 100;

    // Experience
    public static int currentXP = 0;
    public static int totalXPEarned = 0;
    public static int skillPoints = 0;
}
