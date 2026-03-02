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
            }
            else
            {
                DisplayNextLine();
            }
        }
    }

    // =============================
    // START DIALOGUE
    // =============================
    public void StartDialogue(Sprite avatar, List<string> dialogueLines)
    {
        if (dialoguePanel == null || dialogueText == null)
            return;

        dialoguePanel.SetActive(true);
        IsOpen = true;

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
        StopAllCoroutines();
        StartCoroutine(TypeLine(currentLine));
    }

    IEnumerator TypeLine(string line)
    {
        isTyping = true;
        dialogueText.text = "";

        foreach (char c in line)
        {
            dialogueText.text += c;
            yield return new WaitForSeconds(typeSpeed);
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

        SetPlayerLock(false);

        // Capture value before it gets reset in ResolveRewards
        bool gaveRewards = giveSkillPointsOnEnd;
        ResolveRewards();
        
        OnDialogueEnded?.Invoke(gaveRewards);
    }


    void ForceCloseDialogue()
    {
        StopAllCoroutines();
        lines.Clear();
        isTyping = false;

        IsOpen = false;

        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);

        SetPlayerLock(false);

        // ✅ ESC NOW ALSO GIVES POINTS
        bool gaveRewards = giveSkillPointsOnEnd;
        ResolveRewards();

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

}
