using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance;

    // Event invoked when dialogue ends.
    // The bool parameter indicates whether skill points were awarded during that dialogue.
    public static event Action<bool> OnDialogueEnded;

    [Header("UI References")]
    public GameObject dialoguePanel;
    public Image avatarImage;                  // optional, can be left unassigned
    public TextMeshProUGUI dialogueText;

    [Header("Rewards")]
    public bool giveSkillPointsOnEnd = false;

    [Header("Typing")]
    public float typeSpeed = 0.03f;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip[] gibberishClips;

    // Internals
    private Queue<string> lines = new Queue<string>();
    private bool isTyping = false;
    private string currentLine = "";
    public bool IsOpen { get; private set; } = false;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }

        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);
            
        // Auto-add AudioSource if missing
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();
    }

    void Update()
    {
        if (!IsOpen) return;

        // ESC — force close dialogue (no rewards)
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ForceCloseDialogue();
            return;
        }

        // Enter — skip typing or advance
        if (Input.GetKeyDown(KeyCode.Return))
        {
            if (isTyping)
            {
                StopAllCoroutines();
                dialogueText.text = currentLine;
                isTyping = false;
                
                // Stop audio immediately if skipping
                if (audioSource != null) audioSource.Stop();
            }
            else
            {
                DisplayNextLine();
            }
        }
    }

    // Audio
    private AudioClip[] overrideClips; // If set, use these instead of default

    // =============================
    // START DIALOGUE
    // =============================
    public void StartDialogue(Sprite avatar, List<string> dialogueLines, AudioClip[] customAudio = null)
    {
        if (dialoguePanel == null || dialogueText == null)
            return;

        // Pause Music
        if (MusicManager.instance != null)
            MusicManager.instance.PauseMusic();

        dialoguePanel.SetActive(true);
        IsOpen = true;

        // Set custom audio if provided
        this.overrideClips = customAudio;

        SetPlayerLock(true);

        // ✅ Avatar handling
        if (avatarImage != null)
        {
            if (avatar != null)
            {
                avatarImage.sprite = avatar;
                avatarImage.enabled = true;
            }
            else
            {
                avatarImage.enabled = false;
            }
        }

        lines.Clear();
        foreach (var line in dialogueLines)
            lines.Enqueue(line);

        DisplayNextLine();
    }


    // =============================
    // LINE FLOW
    // =============================
    void DisplayNextLine()
    {
        if (lines.Count == 0)
        {
            EndDialogue();
            return;
        }

        currentLine = lines.Dequeue();
        
        // Ensure everything is stopped before starting new line
        StopAllCoroutines();
        if (audioSource != null) audioSource.Stop();
        
        StartCoroutine(TypeLine(currentLine));
    }

    IEnumerator TypeLine(string line)
    {
        isTyping = true;
        dialogueText.text = "";

        // Start playing audio
        if (audioSource != null)
        {
            // Case 0: specific character audio (Boss, etc.)
            if (overrideClips != null && overrideClips.Length > 0)
            {
                AudioClip clip = overrideClips[UnityEngine.Random.Range(0, overrideClips.Length)];
                audioSource.clip = clip;
                audioSource.loop = true;
                audioSource.pitch = UnityEngine.Random.Range(0.8f, 1.0f); // Lower pitch for boss/custom
                audioSource.time = UnityEngine.Random.Range(0f, clip.length);
                audioSource.Play();
            }
            // Case 1: Use random clip from default array if available
            else if (gibberishClips != null && gibberishClips.Length > 0)
            {
                AudioClip clip = gibberishClips[UnityEngine.Random.Range(0, gibberishClips.Length)];
                audioSource.clip = clip;
                audioSource.loop = true;
                audioSource.pitch = UnityEngine.Random.Range(0.9f, 1.1f); // Normal pitch
                audioSource.time = UnityEngine.Random.Range(0f, clip.length); 
                audioSource.Play();
            }
            // Case 2: Fallback to the clip assigned on the AudioSource component
            else if (audioSource.clip != null)
            {
                audioSource.loop = true;
                audioSource.pitch = UnityEngine.Random.Range(0.9f, 1.1f);
                audioSource.time = UnityEngine.Random.Range(0f, audioSource.clip.length); 
                audioSource.Play();
            }
        }

        foreach (char c in line)
        {
            dialogueText.text += c;
            yield return new WaitForSeconds(typeSpeed);
        }

        // Stop audio when typing finishes
        if (audioSource != null)
        {
            audioSource.Stop();
            audioSource.loop = false;
        }

        isTyping = false;
    }

    // =============================
    // END / FORCE CLOSE
    // =============================
    void EndDialogue()
    {
        IsOpen = false;

        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);

        // Stop audio if playing
        if (audioSource != null)
        {
            audioSource.Stop();
            audioSource.loop = false;
        }

        // Resume Music
        if (MusicManager.instance != null)
            MusicManager.instance.ResumeMusic();

        SetPlayerLock(false);

        // Capture value before it gets reset in ResolveRewards
        bool gaveRewards = giveSkillPointsOnEnd;
        ResolveRewards();
        
        overrideClips = null; // Reset custom audio

        OnDialogueEnded?.Invoke(gaveRewards);
    }


    void ForceCloseDialogue()
    {
        StopAllCoroutines();
        lines.Clear();
        isTyping = false;

        // Stop audio if playing
        if (audioSource != null)
        {
            audioSource.Stop();
            audioSource.loop = false;
        }

        IsOpen = false;

        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);

        // Resume Music
        if (MusicManager.instance != null)
            MusicManager.instance.ResumeMusic();

        SetPlayerLock(false);

        // ✅ ESC NOW ALSO GIVES POINTS
        bool gaveRewards = giveSkillPointsOnEnd;
        ResolveRewards();

        overrideClips = null; // Reset custom audio

        Debug.Log("[DialogueManager] Dialogue force-closed");
        OnDialogueEnded?.Invoke(gaveRewards);
    }


    // =============================
    // PLAYER LOCK
    // =============================
    void SetPlayerLock(bool locked)
    {
        PlayerController player = FindObjectOfType<PlayerController>();
        if (player != null)
            player.inputLocked = locked;
    }

    void ResolveRewards()
    {
        // Only give points if the flag is true AND we have a valid SkillTreeManager
        if (giveSkillPointsOnEnd && SkillTreeManager.Instance != null)
        {
            SkillTreeManager.Instance.GivePointsFromMerchant(2);
            Debug.Log("[Dialogue] Skill points awarded");
        }
        
        // Note: We don't need to reset the flag here if we want to preserve the state for the event,
        // BUT the original code reset it. 
        // My previous edit captured the value BEFORE calling this method, so we are safe.
        // I will just keep this method as is (resetting the flag) because the logic in EndDialogue/ForceClose 
        // already handles the capture correctly. This is just a no-op edit to confirm I checked it.
        giveSkillPointsOnEnd = false;
    }

    // =============================
    // TUTORIAL MODE (NON-BLOCKING)
    // =============================
    public void ShowTutorialMessage(string message, Sprite avatar = null)
    {
        if (dialoguePanel == null || dialogueText == null) return;

        // Ensure panel is open
        dialoguePanel.SetActive(true);
        
        // DO NOT set IsOpen = true (avoids input hijacking)
        // DO NOT pause music
        // DO NOT lock player input
        
        // Show avatar if provided
        if (avatarImage != null)
        {
            avatarImage.enabled = avatar != null;
            if (avatar != null) avatarImage.sprite = avatar;
        }

        // Display text immediately (no typing effect for snappy tutorials, or repurpose typing if desired)
        StopAllCoroutines();
        dialogueText.text = message;
    }

    public void HideTutorialMessage()
    {
        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);
            
        // No rewards, no event triggers, just hide UI
    }
}
