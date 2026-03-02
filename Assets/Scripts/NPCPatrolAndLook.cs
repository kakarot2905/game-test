using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(SpriteRenderer))]
public class NPCPatrolAndLook : MonoBehaviour
{
    private Transform player;
    private bool playerInRange = false;

    [Header("Avatar")]
    public Sprite avatarSprite;


    [Header("Talk Prompt")]
    public string promptText = "Press Enter to Talk";
    public float textHeight = 1.6f;

    [Header("Font")]
    public TMP_FontAsset fontAsset;

    [Header("Merchant Rewards")]
    public bool canGiveSkillPoints = false;   // enable only for merchants that give points

    [Header("Scene Transition")]
    public bool triggersSceneTransition = false; // enable ONLY on 2nd merchant
    public string nextSceneName;
    public float transitionDelay = 0.5f;

    [Header("Bounce Effect")]
    public float bounceAmplitude = 0.1f;
    public float bounceSpeed = 3f;

    [Header("Dialogue")]
    [TextArea(2, 6)]
    public string[] dialogueLines;

    // Per-merchant state
    private bool hasGivenSkillPoints = false;
    private bool dialogueInProgress = false;

    private SpriteRenderer sr;
    private TextMeshPro promptTMP;
    private Transform promptTransform;
    private Vector3 promptBaseLocalPos;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        CreatePrompt();
    }

    void Update()
    {
        if (!playerInRange || player == null) return;

        FacePlayer();
        HandlePromptBounce();

        if (Input.GetKeyDown(KeyCode.Return)
            && DialogueManager.Instance != null
            && !DialogueManager.Instance.IsOpen
            && !dialogueInProgress)
        {
            StartMerchantDialogue();
        }

        if (DialogueManager.Instance != null &&
            !DialogueManager.Instance.IsOpen &&
            !dialogueInProgress &&
            promptTransform != null &&
            !promptTransform.gameObject.activeSelf)
        {
            promptTransform.gameObject.SetActive(true);
        }
    }

    void StartMerchantDialogue()
    {
        dialogueInProgress = true;

        // Decide reward for THIS merchant
        DialogueManager.Instance.giveSkillPointsOnEnd =
            canGiveSkillPoints && !hasGivenSkillPoints;

        DialogueManager.Instance.StartDialogue(
    avatarSprite,
    new List<string>(dialogueLines)
);


        if (promptTransform != null)
            promptTransform.gameObject.SetActive(false);

        StartCoroutine(WaitForDialogueEnd());
    }

    IEnumerator WaitForDialogueEnd()
    {
        yield return new WaitUntil(() =>
            DialogueManager.Instance == null || !DialogueManager.Instance.IsOpen
        );

        // Mark reward used
        if (canGiveSkillPoints && !hasGivenSkillPoints)
        {
            hasGivenSkillPoints = true;
            Debug.Log("[Merchant] Skill points granted — merchant exhausted");
        }

        // Safety reset
        if (DialogueManager.Instance != null)
            DialogueManager.Instance.giveSkillPointsOnEnd = false;

        dialogueInProgress = false;

        // Scene transition (only for flagged merchant)
        if (triggersSceneTransition && !string.IsNullOrEmpty(nextSceneName))
        {
            Debug.Log($"[Merchant] Transitioning to scene: {nextSceneName}");
            yield return new WaitForSeconds(transitionDelay);
            SceneManager.LoadScene(nextSceneName);
        }
    }

    // -------------------------
    // Prompt creation
    // -------------------------
    void CreatePrompt()
    {
        GameObject go = new GameObject("TalkPrompt");
        go.transform.SetParent(transform);
        go.transform.localPosition = Vector3.up * textHeight;

        promptTMP = go.AddComponent<TextMeshPro>();
        promptTMP.text = promptText;
        promptTMP.fontSize = 3.5f;
        promptTMP.alignment = TextAlignmentOptions.Center;
        promptTMP.color = Color.white;

        var r = go.GetComponent<Renderer>();
        if (r != null) r.sortingOrder = 10;

        if (fontAsset != null)
            promptTMP.font = fontAsset;

        promptTransform = go.transform;
        promptBaseLocalPos = promptTransform.localPosition;

        go.SetActive(false);
    }

    void FacePlayer()
    {
        Vector3 scale = transform.localScale;
        scale.x = player.position.x < transform.position.x
            ? -Mathf.Abs(scale.x)
            : Mathf.Abs(scale.x);
        transform.localScale = scale;

        Vector3 textScale = promptTransform.localScale;
        textScale.x = Mathf.Sign(transform.localScale.x) * Mathf.Abs(textScale.x);
        promptTransform.localScale = textScale;
    }

    void HandlePromptBounce()
    {
        float yOffset = Mathf.Sin(Time.time * bounceSpeed) * bounceAmplitude;
        promptTransform.localPosition = promptBaseLocalPos + Vector3.up * yOffset;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        player = other.transform;
        playerInRange = true;
        promptTransform.gameObject.SetActive(true);
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        player = null;
        playerInRange = false;
        promptTransform.gameObject.SetActive(false);
    }
}
