using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialManager : MonoBehaviour
{
    [System.Serializable]
    public class TutorialStep
    {
        [TextArea(3, 5)]
        public string instructionText;
        public TutorialAction requiredAction;
        public TutorialForm requiredForm; 
        public float requiredDuration = 0f; 
        public float postActionDelay = 0f;  
        public GameObject[] tutorialProps; // Changed to Array
        public Sprite specificAvatar;       
    }

    public enum TutorialForm
    {
        Any,
        MovementCat, // White Cat
        AttackCat    // Black Cat
    }

    public enum TutorialAction
    {
        None,           
        Move,           
        Jump,           
        Attack,         
        Melee,          
        SwitchForm,     
        Dash,           
        Duck,           
        WallSlide,      
        ToggleSkillTree,
        Overpower,
        WaitForLowHealth // New: Wait for HP <= 1
    }

    [Header("Tutorial Sequence")]
    public List<TutorialStep> steps = new List<TutorialStep>();
    public Sprite defaultAvatar;
    
    [Header("Settings")]
    public float stepDelay = 0.5f; 
    public string nextSceneName; 

    private int currentStepIndex = -1;
    private float actionTimer = 0f;
    private bool isTutorialActive = false;
    private bool waitingForDelay = false; 
    private PlayerController playerRef; 

    void Start()
    {
        playerRef = FindObjectOfType<PlayerController>();
        
        if (steps.Count > 0)
        {
             StartTutorial();
        }
    }

    public void StartTutorial()
    {
        if (playerRef == null) playerRef = FindObjectOfType<PlayerController>();
        
        // UNLOCK ALL SKILLS TEMPORARILY
        SetAllSkills(true);

        currentStepIndex = 0;
        isTutorialActive = true;
        ShowCurrentStep();
    }

    void Update()
    {
        if (!isTutorialActive || currentStepIndex >= steps.Count || waitingForDelay) return;
        
        // CRITICAL FIX: Constantly enforce unlocked skills during tutorial
        if (playerRef != null && (!playerRef.dashUnlocked || !playerRef.powerShotUnlocked))
        {
            SetAllSkills(true);
        }

        CheckStepCompletion();
    }

    void ShowCurrentStep()
    {
        if (currentStepIndex >= steps.Count)
        {
            CompleteTutorial();
            return;
        }

        TutorialStep step = steps[currentStepIndex];
        
        // Enable Props
        if (step.tutorialProps != null)
        {
            foreach (var prop in step.tutorialProps)
            {
                if (prop != null) prop.SetActive(true);
            }
        }
        
        Sprite avatarToShow = step.specificAvatar != null ? step.specificAvatar : defaultAvatar;
        DialogueManager.Instance.ShowTutorialMessage(step.instructionText, avatarToShow);
        
        actionTimer = 0f; 
    }

    void CheckStepCompletion()
    {
        TutorialStep step = steps[currentStepIndex];
        
        // 1. Check Form Constraint
        if (playerRef != null)
        {
            if (step.requiredForm == TutorialForm.MovementCat && playerRef.isAttackMode) return;
            if (step.requiredForm == TutorialForm.AttackCat && !playerRef.isAttackMode) return;
        }

        bool actionDetected = false;

        switch (step.requiredAction)
        {
            case TutorialAction.None:
                actionTimer += Time.deltaTime;
                if (actionTimer >= (step.requiredDuration > 0 ? step.requiredDuration : 3f))
                    actionDetected = true;
                if (Input.GetKeyDown(KeyCode.Return)) 
                    actionDetected = true;
                break;

            case TutorialAction.Move:
                if (Mathf.Abs(Input.GetAxis("Horizontal")) > 0.1f)
                {
                    actionTimer += Time.deltaTime;
                    if (actionTimer >= step.requiredDuration) actionDetected = true;
                }
                break;

            case TutorialAction.Jump:
                if (Input.GetButtonDown("Jump")) actionDetected = true;
                break;

            case TutorialAction.Duck:
                if (Input.GetKey(KeyCode.S))
                {
                    actionTimer += Time.deltaTime;
                    if (actionTimer >= (step.requiredDuration > 0 ? step.requiredDuration : 0.2f)) 
                        actionDetected = true;
                }
                break;

            case TutorialAction.SwitchForm:
                if (Input.GetKeyDown(KeyCode.E)) actionDetected = true;
                break;

            case TutorialAction.Attack:
                if (playerRef.isAttackMode && Input.GetButtonDown("Fire1")) actionDetected = true;
                break;
                
            case TutorialAction.Melee:
                if (playerRef.isAttackMode && Input.GetButtonDown("Fire2")) actionDetected = true;
                break;
                
            case TutorialAction.Dash:
                if (!playerRef.isAttackMode && Input.GetKeyDown(KeyCode.LeftShift)) actionDetected = true;
                // If isAttackMode, ignore dash input
                break;
                
            case TutorialAction.WallSlide:
                if (playerRef != null && !playerRef.isAttackMode) // Wall Slide is Movement Cat feature
                {
                    bool looksLikeSliding = playerRef.GetComponent<Rigidbody2D>().linearVelocity.y < 0 
                                            && (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.D));
                    
                    if (looksLikeSliding)
                    {
                         actionTimer += Time.deltaTime;
                         if (actionTimer >= (step.requiredDuration > 0 ? step.requiredDuration : 0.5f))
                            actionDetected = true;
                    }
                }
                break;

            case TutorialAction.ToggleSkillTree:
                if (Input.GetKeyDown(KeyCode.T)) actionDetected = true;
                break;

            case TutorialAction.Overpower:
                if (Input.GetKeyDown(KeyCode.P)) actionDetected = true;
                break;

            case TutorialAction.WaitForLowHealth:
                if (playerRef != null && playerRef.currentHP <= 1) 
                    actionDetected = true;
                break;
        }

        if (actionDetected)
        {
            StartCoroutine(AdvanceStepRoutine());
        }
    }

    System.Collections.IEnumerator AdvanceStepRoutine()
    {
        waitingForDelay = true; // Stop checking input
        TutorialStep currentStep = steps[currentStepIndex];

        // Wait for specific post-action delay
        if (currentStep.postActionDelay > 0)
        {
            yield return new WaitForSeconds(currentStep.postActionDelay);
        }
        else
        {
            yield return new WaitForSeconds(stepDelay);
        }

        // Disable Props
        if (currentStep.tutorialProps != null)
        {
            foreach (var prop in currentStep.tutorialProps)
            {
                if (prop != null) prop.SetActive(false);
            }
        }

        // Move Next
        currentStepIndex++;
        waitingForDelay = false;
        
        ShowCurrentStep();
    }

    void CompleteTutorial()
    {
        isTutorialActive = false;
        DialogueManager.Instance.HideTutorialMessage();
        
        // LOCK ALL SKILLS & RESET
        SetAllSkills(false);
        
        Debug.Log("Tutorial Completed! Skills Reset.");
        
        if (!string.IsNullOrEmpty(nextSceneName))
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(nextSceneName);
        }
        else
        {
            Debug.Log("Tutorial Finished but no Next Scene set!");
        }
    }
    
    void SetAllSkills(bool unlocked)
    {
        if (playerRef != null)
        {
            playerRef.dashUnlocked = unlocked;
            playerRef.powerShotUnlocked = unlocked;
            playerRef.wallSlideUnlocked = unlocked;
            playerRef.doubleJumpUnlocked = unlocked;
            playerRef.meleeAttackUnlocked = unlocked;
            playerRef.meleeComboUnlocked = unlocked;
        }
        
        // Also update persistence so it sticks after tutorial
        PlayerProgress.dashUnlocked = unlocked;
        PlayerProgress.powerShotUnlocked = unlocked;
        PlayerProgress.wallSlideUnlocked = unlocked;
        PlayerProgress.doubleJumpUnlocked = unlocked;
        PlayerProgress.meleeAttackUnlocked = unlocked;
        PlayerProgress.meleeComboUnlocked = unlocked;
    }

    // EDITOR HELPER
    [ContextMenu("Generate All Controls Tutorial")]
    public void GenerateAllControlsTutorial()
    {
        steps.Clear();
        
        steps.Add(new TutorialStep { instructionText = "Use A / D to Move.", requiredAction = TutorialAction.Move, requiredDuration = 1.0f });
        steps.Add(new TutorialStep { instructionText = "Press SPACE to Jump.", requiredAction = TutorialAction.Jump });
        steps.Add(new TutorialStep { instructionText = "Hold S to Duck.", requiredAction = TutorialAction.Duck, requiredDuration = 1.0f });
        steps.Add(new TutorialStep { instructionText = "Press E to Switch Forms.", requiredAction = TutorialAction.SwitchForm });
        
        // Form-specific (assuming player swaps)
        steps.Add(new TutorialStep { instructionText = "As Black Cat: Right Click for Melee.", requiredAction = TutorialAction.Melee, requiredForm = TutorialForm.AttackCat });
        steps.Add(new TutorialStep { instructionText = "As Black Cat: Left Click for Range.", requiredAction = TutorialAction.Attack, requiredForm = TutorialForm.AttackCat });
        
        steps.Add(new TutorialStep { instructionText = "Press Shift to Dash (White Cat).", requiredAction = TutorialAction.Dash, requiredForm = TutorialForm.MovementCat });
        steps.Add(new TutorialStep { instructionText = "Press T to Open Skills.", requiredAction = TutorialAction.ToggleSkillTree });
        
        // Advanced
        steps.Add(new TutorialStep { instructionText = "Jump against a wall to Wall Slide!", requiredAction = TutorialAction.WallSlide, requiredDuration = 0.5f, requiredForm = TutorialForm.MovementCat });
        steps.Add(new TutorialStep { instructionText = "Press P to Unleash Overpower!", requiredAction = TutorialAction.Overpower });
        
        Debug.Log("Tutorial Generated! Check Inspector.");
    }
}
