using UnityEngine;
using System.Collections;

public class BossZone : MonoBehaviour
{
    [Header("Setup")]
    public BossController boss;
    [Header("Arena Walls")]
    public GameObject leftWallObject;
    public GameObject rightWallObject;
    
    [Header("Camera")]
    public CameraFollow cameraFollow;

    [Header("UI & Dialogue")]
    public BossHealthBarUI healthBar;
    public Sprite bossAvatar;
    [TextArea] public System.Collections.Generic.List<string> introLines;
    public AudioClip[] bossAudioClips; // New field for custom boss sound

    private bool fightStarted = false;
    private bool dialoguePlayed = false;
    private Transform playerTransform;

    void Awake()
    {
        if (healthBar == null)
            healthBar = FindObjectOfType<BossHealthBarUI>();
    }

    void Start()
    {
        // Disable walls initially
        if (leftWallObject != null) leftWallObject.SetActive(false);
        if (rightWallObject != null) rightWallObject.SetActive(false);
        
        // Hide Health Bar initially
        if (healthBar != null) healthBar.Hide();

        if (boss != null)
        {
            boss.OnDeath += EndFight;
        }
    }

    void OnDestroy()
    {
        if (boss != null)
        {
            boss.OnDeath -= EndFight;
        }
        // Ensure we unsubscribe if destroyed mid-dialogue
        DialogueManager.OnDialogueEnded -= OnIntroDialogueEnded;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (fightStarted) return;

        if (other.CompareTag("Player"))
        {
            if (!dialoguePlayed && introLines.Count > 0)
            {
                playerTransform = other.transform; // Capture player reference
                StartDialogue();
            }
            else
            {
                playerTransform = other.transform; // Ensure relevant reference
                StartFight();
            }
        }
    }

    // Removed StartDialogueRoutine (delay) as requested

    void StartDialogue()
    {
        Debug.Log("BossZone: Starting Intro Dialogue...");
        dialoguePlayed = true;

        // LOCK ARENA IMMEDIATELY (Prevent external enemies)
        LockArena();
        
        // FOCUS CAMERA ON BOSS
        if (cameraFollow != null && boss != null)
        {
            cameraFollow.SetTarget(boss.transform);
        }

        // Subscribe to end event
        DialogueManager.OnDialogueEnded += OnIntroDialogueEnded;
        
        // Start Interaction with Custom Audio
        DialogueManager.Instance.StartDialogue(bossAvatar, introLines, bossAudioClips);
    }

    void OnIntroDialogueEnded(bool gaveRewards)
    {
        // Unsubscribe immediately
        DialogueManager.OnDialogueEnded -= OnIntroDialogueEnded;
        
        // RESET CAMERA TO PLAYER
        if (cameraFollow != null && playerTransform != null)
        {
            cameraFollow.SetTarget(playerTransform);
        }
        
        // NOW Start the fight
        StartFight();
    }

    void LockArena()
    {
        // 1. Lock Arena (Enable separate walls)
        if (leftWallObject != null) leftWallObject.SetActive(true);
        if (rightWallObject != null) rightWallObject.SetActive(true);

        // 2. Lock Camera
        if (cameraFollow != null)
        {
            // Use the wall objects' transforms as locks
            if (leftWallObject != null) cameraFollow.cameraLeftLock = leftWallObject.transform;
            if (rightWallObject != null) cameraFollow.cameraRightLock = rightWallObject.transform;
        }
    }

    void StartFight()
    {
        fightStarted = true;
        Debug.Log("BossZone: Fight Started!");

        // Ensure Arena is locked (redundant if coming from dialogue, but safe)
        LockArena();

        // 3. Wake Boss & Show HP
        if (boss != null)
        {
            // Optional: Force boss state if needed
            if (healthBar != null)
            {
                healthBar.SetBoss(boss);
                healthBar.Show();
            }
        }
    }




    void EndFight()
    {
        Debug.Log("BossZone: Fight Ended! Unlocking Arena.");
        
        // 1. Unlock Arena
        if (leftWallObject != null) leftWallObject.SetActive(false);
        if (rightWallObject != null) rightWallObject.SetActive(false);

        // 2. Unlock Camera
        if (cameraFollow != null)
        {
            cameraFollow.cameraLeftLock = null;
            cameraFollow.cameraRightLock = null;
        }

        // 3. Hide Health Bar
        if (healthBar != null) healthBar.Hide();
    }
}
