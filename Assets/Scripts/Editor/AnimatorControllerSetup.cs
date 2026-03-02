using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;
using System.Linq;

/// <summary>
/// One-time setup script to configure the Player Animator Controller
/// with all required parameters and transitions.
/// 
/// HOW TO USE:
/// 1. Select your Player prefab or GameObject in the scene
/// 2. Go to menu: Tools > Setup Player Animator
/// 3. The script will automatically configure all parameters and transitions
/// 4. Check the Console for confirmation messages
/// </summary>
public class AnimatorControllerSetup : EditorWindow
{
    private AnimatorController controller;
    
    [MenuItem("Tools/Setup Player Animator")]
    static void SetupAnimator()
    {
        AnimatorControllerSetup window = GetWindow<AnimatorControllerSetup>();
        window.titleContent = new GUIContent("Animator Setup");
        window.Show();
    }

    void OnGUI()
    {
        GUILayout.Label("Player Animator Controller Setup", EditorStyles.boldLabel);
        GUILayout.Space(10);

        controller = (AnimatorController)EditorGUILayout.ObjectField(
            "Animator Controller", 
            controller, 
            typeof(AnimatorController), 
            false
        );

        GUILayout.Space(10);

        if (GUILayout.Button("Auto-Find Player Controller", GUILayout.Height(30)))
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
            Debug.Log("<color=green>[Animator Setup] Complete! All parameters and transitions configured.</color>");
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
        // Try to find selected GameObject first
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

        // Search in Assets folder
        string[] guids = AssetDatabase.FindAssets("t:AnimatorController Player");
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            AnimatorController ac = AssetDatabase.LoadAssetAtPath<AnimatorController>(path);
            if (ac != null)
            {
                controller = ac;
                Debug.Log($"Found animator controller at: {path}");
                return;
            }
        }

        Debug.LogWarning("Could not auto-find controller. Please assign manually.");
    }

    void SetupParameters()
    {
        if (controller == null) return;

        Debug.Log("[Animator Setup] Creating parameters...");

        // Bool Parameters
        AddParameter("isRunning", AnimatorControllerParameterType.Bool);
        AddParameter("isGrounded", AnimatorControllerParameterType.Bool);
        AddParameter("isDucking", AnimatorControllerParameterType.Bool);
        AddParameter("isTouchingWall", AnimatorControllerParameterType.Bool);
        AddParameter("isDead", AnimatorControllerParameterType.Bool);
        AddParameter("isDashing", AnimatorControllerParameterType.Bool);

        // Float Parameters
        AddParameter("yVelocity", AnimatorControllerParameterType.Float);

        // Trigger Parameters
        AddParameter("Jump", AnimatorControllerParameterType.Trigger);
        AddParameter("Hit", AnimatorControllerParameterType.Trigger);
        AddParameter("Death", AnimatorControllerParameterType.Trigger);
        AddParameter("Dash", AnimatorControllerParameterType.Trigger);
        AddParameter("Spin", AnimatorControllerParameterType.Trigger);

        EditorUtility.SetDirty(controller);
        AssetDatabase.SaveAssets();
        
        Debug.Log("<color=cyan>[Animator Setup] Parameters created successfully!</color>");
    }

    void AddParameter(string name, AnimatorControllerParameterType type)
    {
        // Check if parameter already exists
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

        Debug.Log("[Animator Setup] Creating transitions...");

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
        var dash = FindState(rootStateMachine, "Dash");
        var wallSlide = FindState(rootStateMachine, "Wall_Slide");
        var takeDamage = FindState(rootStateMachine, "Take_Damage");
        var death = FindState(rootStateMachine, "Death");
        var spin = FindState(rootStateMachine, "Spin");

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

        if (dash != null)
        {
            var dashTrans = rootStateMachine.AddAnyStateTransition(dash);
            dashTrans.AddCondition(AnimatorConditionMode.If, 0, "Dash");
            dashTrans.duration = 0f;
            dashTrans.canTransitionToSelf = false;
            Debug.Log("✓ Any State → Dash");
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

        // === DASH TRANSITIONS ===
        if (dash != null)
        {
            Debug.Log("Setting up Dash transitions...");
            
            if (idle != null)
            {
                var dashToIdle = dash.AddTransition(idle);
                dashToIdle.AddCondition(AnimatorConditionMode.If, 0, "isGrounded");
                dashToIdle.AddCondition(AnimatorConditionMode.IfNot, 0, "isRunning");
                dashToIdle.duration = 0f;
                dashToIdle.hasExitTime = true;
                dashToIdle.exitTime = 0.8f;
                Debug.Log("✓ Dash → Idle");
            }

            if (run != null)
            {
                var dashToRun = dash.AddTransition(run);
                dashToRun.AddCondition(AnimatorConditionMode.If, 0, "isGrounded");
                dashToRun.AddCondition(AnimatorConditionMode.If, 0, "isRunning");
                dashToRun.duration = 0f;
                dashToRun.hasExitTime = true;
                dashToRun.exitTime = 0.8f;
                Debug.Log("✓ Dash → Run");
            }

            if (fall != null)
            {
                var dashToFall = dash.AddTransition(fall);
                dashToFall.AddCondition(AnimatorConditionMode.IfNot, 0, "isGrounded");
                dashToFall.duration = 0f;
                dashToFall.hasExitTime = true;
                dashToFall.exitTime = 0.8f;
                Debug.Log("✓ Dash → Fall");
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
        
        Debug.Log("<color=cyan>[Animator Setup] Transitions created successfully!</color>");
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
