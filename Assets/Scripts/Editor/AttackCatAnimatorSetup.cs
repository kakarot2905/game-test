using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;
using System.Linq;

/// <summary>
/// Setup script for Attack Cat (Black Cat) Animator Controller
/// Configures combat-focused animations with skill progression system
/// 
/// HOW TO USE:
/// 1. Select your Black Cat prefab or GameObject in the scene
/// 2. Go to menu: Tools > Setup Attack Cat Animator
/// 3. The script will configure all parameters and transitions
/// 4. Check Console for confirmation
/// </summary>
public class AttackCatAnimatorSetup : EditorWindow
{
    private AnimatorController controller;
    
    [MenuItem("Tools/Setup Attack Cat Animator")]
    static void SetupAnimator()
    {
        AttackCatAnimatorSetup window = GetWindow<AttackCatAnimatorSetup>();
        window.titleContent = new GUIContent("Attack Cat Setup");
        window.Show();
    }

    void OnGUI()
    {
        GUILayout.Label("Attack Cat Animator Setup", EditorStyles.boldLabel);
        GUILayout.Space(10);

        controller = (AnimatorController)EditorGUILayout.ObjectField(
            "Animator Controller", 
            controller, 
            typeof(AnimatorController), 
            false
        );

        GUILayout.Space(10);

        if (GUILayout.Button("Auto-Find Black Cat Controller", GUILayout.Height(30)))
        {
            AutoFindController();
        }

        GUILayout.Space(10);

        if (controller == null)
        {
            EditorGUILayout.HelpBox("Please assign the Animator Controller or use Auto-Find.", MessageType.Warning);
            return;
        }

        EditorGUILayout.HelpBox($"Controller: {controller.name}", MessageType.Info);
        
        GUILayout.Space(10);

        if (GUILayout.Button("Setup All Parameters & Transitions", GUILayout.Height(40)))
        {
            SetupParameters();
            SetupTransitions();
            Debug.Log("<color=green>[Attack Cat Setup] Complete! All parameters and transitions configured.</color>");
        }

        GUILayout.Space(10);

        if (GUILayout.Button("Setup Parameters Only", GUILayout.Height(30)))
        {
            SetupParameters();
        }

        if (GUILayout.Button("Setup Transitions Only", GUILayout.Height(30)))
        {
            SetupTransitions();
        }
    }

    void AutoFindController()
    {
        // Try selected GameObject first
        GameObject selected = Selection.activeGameObject;
        
        if (selected != null)
        {
            Animator animator = selected.GetComponentInChildren<Animator>();
            if (animator != null && animator.runtimeAnimatorController != null)
            {
                controller = animator.runtimeAnimatorController as AnimatorController;
                Debug.Log($"Found animator controller: {controller.name}");
                return;
            }
        }

        // Search for Black Cat / Attack Cat controller
        string[] guids = AssetDatabase.FindAssets("t:AnimatorController");
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            if (path.Contains("Black") || path.Contains("Attack"))
            {
                AnimatorController ac = AssetDatabase.LoadAssetAtPath<AnimatorController>(path);
                if (ac != null)
                {
                    controller = ac;
                    Debug.Log($"Found animator controller at: {path}");
                    return;
                }
            }
        }

        Debug.LogWarning("Could not auto-find controller. Please assign manually.");
    }

    void SetupParameters()
    {
        if (controller == null) return;

        Debug.Log("[Attack Cat Setup] Creating parameters...");

        // Bool Parameters (Movement - same as Movement Cat)
        AddParameter("isRunning", AnimatorControllerParameterType.Bool);
        AddParameter("isGrounded", AnimatorControllerParameterType.Bool);
        AddParameter("isDucking", AnimatorControllerParameterType.Bool);
        AddParameter("isTouchingWall", AnimatorControllerParameterType.Bool);
        AddParameter("isDead", AnimatorControllerParameterType.Bool);

        // Float Parameters
        AddParameter("yVelocity", AnimatorControllerParameterType.Float);

        // Trigger Parameters (Movement)
        AddParameter("Jump", AnimatorControllerParameterType.Trigger);
        AddParameter("Hit", AnimatorControllerParameterType.Trigger);
        AddParameter("Death", AnimatorControllerParameterType.Trigger);
        AddParameter("Spin", AnimatorControllerParameterType.Trigger);

        // Trigger Parameters (Combat - Attack Cat specific)
        AddParameter("RangeAttack", AnimatorControllerParameterType.Trigger);
        AddParameter("MeleeAttack", AnimatorControllerParameterType.Trigger);
        AddParameter("MeleeAttack2", AnimatorControllerParameterType.Trigger);

        EditorUtility.SetDirty(controller);
        AssetDatabase.SaveAssets();
        
        Debug.Log("<color=cyan>[Attack Cat Setup] Parameters created successfully!</color>");
    }

    void AddParameter(string name, AnimatorControllerParameterType type)
    {
        if (controller.parameters.Any(p => p.name == name))
        {
            Debug.Log($"Parameter '{name}' already exists, skipping...");
            return;
        }

        controller.AddParameter(name, type);
        Debug.Log($"✓ Added parameter: {name} ({type})");
    }

    void SetupTransitions()
    {
        if (controller == null) return;

        Debug.Log("[Attack Cat Setup] Creating transitions...");

        var layers = controller.layers;
        if (layers.Length == 0)
        {
            Debug.LogError("No layers found in animator controller!");
            return;
        }

        var rootStateMachine = layers[0].stateMachine;

        // Find all states
        var idle = FindState(rootStateMachine, "Idle");
        var run = FindState(rootStateMachine, "Run");
        var jump = FindState(rootStateMachine, "Jump");
        var fall = FindState(rootStateMachine, "Fall");
        var wallSlide = FindState(rootStateMachine, "Wall_Slide");
        var takeDamage = FindState(rootStateMachine, "Take_Damage");
        var death = FindState(rootStateMachine, "Death");
        var spin = FindState(rootStateMachine, "Spin");
        var rangeAttack = FindState(rootStateMachine, "Range_Attack");
        var meleeAttack = FindState(rootStateMachine, "Melee_Attack");
        var meleeAttack2 = FindState(rootStateMachine, "Melee_Attack2");

        // Clear existing transitions
        Debug.Log("Clearing old transitions...");
        ClearTransitions(rootStateMachine);

        // === ANY STATE TRANSITIONS ===
        Debug.Log("Setting up Any State transitions...");
        
        if (death != null)
        {
            var deathTrans = rootStateMachine.AddAnyStateTransition(death);
            deathTrans.AddCondition(AnimatorConditionMode.If, 0, "Death");
            deathTrans.duration = 0f;
            deathTrans.canTransitionToSelf = false;
            Debug.Log("✓ Any State → Death");
        }

        if (takeDamage != null)
        {
            var hitTrans = rootStateMachine.AddAnyStateTransition(takeDamage);
            hitTrans.AddCondition(AnimatorConditionMode.If, 0, "Hit");
            hitTrans.duration = 0f;
            hitTrans.canTransitionToSelf = false;
            Debug.Log("✓ Any State → Take_Damage");
        }

        if (spin != null)
        {
            var spinTrans = rootStateMachine.AddAnyStateTransition(spin);
            spinTrans.AddCondition(AnimatorConditionMode.If, 0, "Spin");
            spinTrans.duration = 0f;
            spinTrans.canTransitionToSelf = false;
            Debug.Log("✓ Any State → Spin");
        }

        // Attack transitions from Any State (can attack from anywhere including air)
        if (rangeAttack != null)
        {
            var rangeAttackTrans = rootStateMachine.AddAnyStateTransition(rangeAttack);
            rangeAttackTrans.AddCondition(AnimatorConditionMode.If, 0, "RangeAttack");
            rangeAttackTrans.duration = 0f;
            rangeAttackTrans.canTransitionToSelf = false;
            Debug.Log("✓ Any State → Range_Attack");
        }

        if (meleeAttack != null)
        {
            var meleeAttackTrans = rootStateMachine.AddAnyStateTransition(meleeAttack);
            meleeAttackTrans.AddCondition(AnimatorConditionMode.If, 0, "MeleeAttack");
            meleeAttackTrans.duration = 0f;
            meleeAttackTrans.canTransitionToSelf = false;
            Debug.Log("✓ Any State → Melee_Attack");
        }

        // === IDLE TRANSITIONS ===
        if (idle != null)
        {
            Debug.Log("Setting up Idle transitions...");
            
            if (run != null)
            {
                var idleToRun = idle.AddTransition(run);
                idleToRun.AddCondition(AnimatorConditionMode.If, 0, "isRunning");
                idleToRun.duration = 0f;
                idleToRun.hasExitTime = false;
                Debug.Log("✓ Idle → Run");
            }

            if (jump != null)
            {
                var idleToJump = idle.AddTransition(jump);
                idleToJump.AddCondition(AnimatorConditionMode.If, 0, "Jump");
                idleToJump.duration = 0f;
                idleToJump.hasExitTime = false;
                Debug.Log("✓ Idle → Jump");
            }

            if (fall != null)
            {
                var idleToFall = idle.AddTransition(fall);
                idleToFall.AddCondition(AnimatorConditionMode.IfNot, 0, "isGrounded");
                idleToFall.AddCondition(AnimatorConditionMode.Less, -0.1f, "yVelocity");
                idleToFall.duration = 0f;
                idleToFall.hasExitTime = false;
                Debug.Log("✓ Idle → Fall");
            }
        }

        // === RUN TRANSITIONS ===
        if (run != null)
        {
            Debug.Log("Setting up Run transitions...");
            
            if (idle != null)
            {
                var runToIdle = run.AddTransition(idle);
                runToIdle.AddCondition(AnimatorConditionMode.IfNot, 0, "isRunning");
                runToIdle.AddCondition(AnimatorConditionMode.If, 0, "isGrounded");
                runToIdle.duration = 0f;
                runToIdle.hasExitTime = false;
                Debug.Log("✓ Run → Idle");
            }

            if (jump != null)
            {
                var runToJump = run.AddTransition(jump);
                runToJump.AddCondition(AnimatorConditionMode.If, 0, "Jump");
                runToJump.duration = 0f;
                runToJump.hasExitTime = false;
                Debug.Log("✓ Run → Jump");
            }

            if (fall != null)
            {
                var runToFall = run.AddTransition(fall);
                runToFall.AddCondition(AnimatorConditionMode.IfNot, 0, "isGrounded");
                runToFall.duration = 0f;
                runToFall.hasExitTime = false;
                Debug.Log("✓ Run → Fall");
            }
        }

        // === JUMP TRANSITIONS ===
        if (jump != null)
        {
            Debug.Log("Setting up Jump transitions...");
            
            if (fall != null)
            {
                var jumpToFall = jump.AddTransition(fall);
                jumpToFall.AddCondition(AnimatorConditionMode.Less, 0f, "yVelocity");
                jumpToFall.duration = 0f;
                jumpToFall.hasExitTime = false;
                Debug.Log("✓ Jump → Fall");
            }

            if (wallSlide != null)
            {
                var jumpToWallSlide = jump.AddTransition(wallSlide);
                jumpToWallSlide.AddCondition(AnimatorConditionMode.If, 0, "isTouchingWall");
                jumpToWallSlide.AddCondition(AnimatorConditionMode.IfNot, 0, "isGrounded");
                jumpToWallSlide.duration = 0f;
                jumpToWallSlide.hasExitTime = false;
                Debug.Log("✓ Jump → Wall_Slide");
            }
        }

        // === FALL TRANSITIONS ===
        if (fall != null)
        {
            Debug.Log("Setting up Fall transitions...");
            
            if (idle != null)
            {
                var fallToIdle = fall.AddTransition(idle);
                fallToIdle.AddCondition(AnimatorConditionMode.If, 0, "isGrounded");
                fallToIdle.AddCondition(AnimatorConditionMode.IfNot, 0, "isRunning");
                fallToIdle.duration = 0f;
                fallToIdle.hasExitTime = false;
                Debug.Log("✓ Fall → Idle");
            }

            if (run != null)
            {
                var fallToRun = fall.AddTransition(run);
                fallToRun.AddCondition(AnimatorConditionMode.If, 0, "isGrounded");
                fallToRun.AddCondition(AnimatorConditionMode.If, 0, "isRunning");
                fallToRun.duration = 0f;
                fallToRun.hasExitTime = false;
                Debug.Log("✓ Fall → Run");
            }

            if (wallSlide != null)
            {
                var fallToWallSlide = fall.AddTransition(wallSlide);
                fallToWallSlide.AddCondition(AnimatorConditionMode.If, 0, "isTouchingWall");
                fallToWallSlide.AddCondition(AnimatorConditionMode.IfNot, 0, "isGrounded");
                fallToWallSlide.duration = 0f;
                fallToWallSlide.hasExitTime = false;
                Debug.Log("✓ Fall → Wall_Slide");
            }
        }

        // === WALL SLIDE TRANSITIONS ===
        if (wallSlide != null)
        {
            Debug.Log("Setting up Wall_Slide transitions...");
            
            if (fall != null)
            {
                var wallSlideToFall = wallSlide.AddTransition(fall);
                wallSlideToFall.AddCondition(AnimatorConditionMode.IfNot, 0, "isTouchingWall");
                wallSlideToFall.duration = 0f;
                wallSlideToFall.hasExitTime = false;
                Debug.Log("✓ Wall_Slide → Fall");
            }

            if (jump != null)
            {
                var wallSlideToJump = wallSlide.AddTransition(jump);
                wallSlideToJump.AddCondition(AnimatorConditionMode.If, 0, "Jump");
                wallSlideToJump.duration = 0f;
                wallSlideToJump.hasExitTime = false;
                Debug.Log("✓ Wall_Slide → Jump (Wall Jump)");
            }

            if (idle != null)
            {
                var wallSlideToIdle = wallSlide.AddTransition(idle);
                wallSlideToIdle.AddCondition(AnimatorConditionMode.If, 0, "isGrounded");
                wallSlideToIdle.duration = 0f;
                wallSlideToIdle.hasExitTime = false;
                Debug.Log("✓ Wall_Slide → Idle");
            }
        }

        // === RANGE ATTACK TRANSITIONS ===
        if (rangeAttack != null)
        {
            Debug.Log("Setting up Range_Attack transitions...");
            
            if (idle != null)
            {
                var rangeToIdle = rangeAttack.AddTransition(idle);
                rangeToIdle.AddCondition(AnimatorConditionMode.If, 0, "isGrounded");
                rangeToIdle.AddCondition(AnimatorConditionMode.IfNot, 0, "isRunning");
                rangeToIdle.duration = 0f;
                rangeToIdle.hasExitTime = true;
                rangeToIdle.exitTime = 0.8f;
                Debug.Log("✓ Range_Attack → Idle");
            }

            if (run != null)
            {
                var rangeToRun = rangeAttack.AddTransition(run);
                rangeToRun.AddCondition(AnimatorConditionMode.If, 0, "isGrounded");
                rangeToRun.AddCondition(AnimatorConditionMode.If, 0, "isRunning");
                rangeToRun.duration = 0f;
                rangeToRun.hasExitTime = true;
                rangeToRun.exitTime = 0.8f;
                Debug.Log("✓ Range_Attack → Run");
            }

            if (fall != null)
            {
                var rangeToFall = rangeAttack.AddTransition(fall);
                rangeToFall.AddCondition(AnimatorConditionMode.IfNot, 0, "isGrounded");
                rangeToFall.duration = 0f;
                rangeToFall.hasExitTime = true;
                rangeToFall.exitTime = 0.8f;
                Debug.Log("✓ Range_Attack → Fall");
            }
        }

        // === MELEE ATTACK TRANSITIONS ===
        if (meleeAttack != null)
        {
            Debug.Log("Setting up Melee_Attack transitions...");
            
            // Combo: Melee_Attack → Melee_Attack2 (if combo unlocked)
            if (meleeAttack2 != null)
            {
                var meleeToMelee2 = meleeAttack.AddTransition(meleeAttack2);
                meleeToMelee2.AddCondition(AnimatorConditionMode.If, 0, "MeleeAttack2");
                meleeToMelee2.duration = 0f;
                meleeToMelee2.hasExitTime = true;
                meleeToMelee2.exitTime = 0.7f; // Can combo at 70% through first attack
                Debug.Log("✓ Melee_Attack → Melee_Attack2 (combo)");
            }

            if (idle != null)
            {
                var meleeToIdle = meleeAttack.AddTransition(idle);
                meleeToIdle.AddCondition(AnimatorConditionMode.If, 0, "isGrounded");
                meleeToIdle.AddCondition(AnimatorConditionMode.IfNot, 0, "isRunning");
                meleeToIdle.duration = 0f;
                meleeToIdle.hasExitTime = true;
                meleeToIdle.exitTime = 0.9f;
                Debug.Log("✓ Melee_Attack → Idle");
            }

            if (run != null)
            {
                var meleeToRun = meleeAttack.AddTransition(run);
                meleeToRun.AddCondition(AnimatorConditionMode.If, 0, "isGrounded");
                meleeToRun.AddCondition(AnimatorConditionMode.If, 0, "isRunning");
                meleeToRun.duration = 0f;
                meleeToRun.hasExitTime = true;
                meleeToRun.exitTime = 0.9f;
                Debug.Log("✓ Melee_Attack → Run");
            }

            if (fall != null)
            {
                var meleeToFall = meleeAttack.AddTransition(fall);
                meleeToFall.AddCondition(AnimatorConditionMode.IfNot, 0, "isGrounded");
                meleeToFall.duration = 0f;
                meleeToFall.hasExitTime = true;
                meleeToFall.exitTime = 0.9f;
                Debug.Log("✓ Melee_Attack → Fall");
            }
        }

        // === MELEE ATTACK 2 TRANSITIONS ===
        if (meleeAttack2 != null)
        {
            Debug.Log("Setting up Melee_Attack2 transitions...");
            
            if (idle != null)
            {
                var melee2ToIdle = meleeAttack2.AddTransition(idle);
                melee2ToIdle.AddCondition(AnimatorConditionMode.If, 0, "isGrounded");
                melee2ToIdle.AddCondition(AnimatorConditionMode.IfNot, 0, "isRunning");
                melee2ToIdle.duration = 0f;
                melee2ToIdle.hasExitTime = true;
                melee2ToIdle.exitTime = 0.9f;
                Debug.Log("✓ Melee_Attack2 → Idle");
            }

            if (run != null)
            {
                var melee2ToRun = meleeAttack2.AddTransition(run);
                melee2ToRun.AddCondition(AnimatorConditionMode.If, 0, "isGrounded");
                melee2ToRun.AddCondition(AnimatorConditionMode.If, 0, "isRunning");
                melee2ToRun.duration = 0f;
                melee2ToRun.hasExitTime = true;
                melee2ToRun.exitTime = 0.9f;
                Debug.Log("✓ Melee_Attack2 → Run");
            }

            if (fall != null)
            {
                var melee2ToFall = meleeAttack2.AddTransition(fall);
                melee2ToFall.AddCondition(AnimatorConditionMode.IfNot, 0, "isGrounded");
                melee2ToFall.duration = 0f;
                melee2ToFall.hasExitTime = true;
                melee2ToFall.exitTime = 0.9f;
                Debug.Log("✓ Melee_Attack2 → Fall");
            }
        }

        // === TAKE DAMAGE TRANSITIONS ===
        if (takeDamage != null)
        {
            Debug.Log("Setting up Take_Damage transitions...");
            
            if (idle != null)
            {
                var takeDamageToIdle = takeDamage.AddTransition(idle);
                takeDamageToIdle.AddCondition(AnimatorConditionMode.If, 0, "isGrounded");
                takeDamageToIdle.duration = 0f;
                takeDamageToIdle.hasExitTime = true;
                takeDamageToIdle.exitTime = 0.9f;
                Debug.Log("✓ Take_Damage → Idle");
            }

            if (fall != null)
            {
                var takeDamageToFall = takeDamage.AddTransition(fall);
                takeDamageToFall.AddCondition(AnimatorConditionMode.IfNot, 0, "isGrounded");
                takeDamageToFall.duration = 0f;
                takeDamageToFall.hasExitTime = true;
                takeDamageToFall.exitTime = 0.9f;
                Debug.Log("✓ Take_Damage → Fall");
            }
        }

        // === SPIN TRANSITIONS ===
        if (spin != null)
        {
            Debug.Log("Setting up Spin transitions...");
            
            if (idle != null)
            {
                var spinToIdle = spin.AddTransition(idle);
                spinToIdle.duration = 0f;
                spinToIdle.hasExitTime = true;
                spinToIdle.exitTime = 0.95f;
                Debug.Log("✓ Spin → Idle");
            }
        }

        // Death has no exit transitions (permanent state)

        EditorUtility.SetDirty(controller);
        AssetDatabase.SaveAssets();
        
        Debug.Log("<color=cyan>[Attack Cat Setup] Transitions created successfully!</color>");
    }

    AnimatorState FindState(AnimatorStateMachine stateMachine, string name)
    {
        foreach (var state in stateMachine.states)
        {
            if (state.state.name == name)
                return state.state;
        }
        
        Debug.LogWarning($"State '{name}' not found in animator!");
        return null;
    }

    void ClearTransitions(AnimatorStateMachine stateMachine)
    {
        // Clear Any State transitions
        for (int i = stateMachine.anyStateTransitions.Length - 1; i >= 0; i--)
        {
            stateMachine.RemoveAnyStateTransition(stateMachine.anyStateTransitions[i]);
        }

        // Clear state transitions
        foreach (var childState in stateMachine.states)
        {
            for (int i = childState.state.transitions.Length - 1; i >= 0; i--)
            {
                childState.state.RemoveTransition(childState.state.transitions[i]);
            }
        }

        Debug.Log("Old transitions cleared.");
    }
}
