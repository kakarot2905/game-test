using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;
using System.Linq;

/// <summary>
/// Setup script for Boss (The Construct) Animator Controller
/// Configures Boss-specific parameters and transitions
/// 
/// HOW TO USE:
/// 1. Create an Animator Controller "BossController" and assign it to the Boss GameObject.
/// 2. Select the Boss GameObject.
/// 3. Go to menu: Tools > Setup Boss Animator
/// </summary>
public class BossAnimatorSetup : EditorWindow
{
    private AnimatorController controller;
    
    [MenuItem("Tools/Setup Boss Animator")]
    static void SetupAnimator()
    {
        BossAnimatorSetup window = GetWindow<BossAnimatorSetup>();
        window.titleContent = new GUIContent("Boss Setup");
        window.Show();
    }

    void OnGUI()
    {
        GUILayout.Label("Boss Animator Setup", EditorStyles.boldLabel);
        GUILayout.Space(10);

        controller = (AnimatorController)EditorGUILayout.ObjectField(
            "Animator Controller", 
            controller, 
            typeof(AnimatorController), 
            false
        );

        GUILayout.Space(10);

        if (GUILayout.Button("Auto-Find Boss Controller", GUILayout.Height(30)))
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
            Debug.Log("<color=green>[Boss Setup] Complete! All parameters and transitions configured.</color>");
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
    }

    void SetupParameters()
    {
        if (controller == null) return;

        Debug.Log("[Boss Setup] Creating parameters...");

        // Bool Parameters
        AddParameter("isEnraged", AnimatorControllerParameterType.Bool);
        AddParameter("isImmune", AnimatorControllerParameterType.Bool);
        AddParameter("isDead", AnimatorControllerParameterType.Bool); // Keep standard

        // Trigger Parameters (Attacks & Events)
        AddParameter("MeleeAttack", AnimatorControllerParameterType.Trigger);
        AddParameter("RangeAttack", AnimatorControllerParameterType.Trigger); // "shoot"
        AddParameter("LaserAttack", AnimatorControllerParameterType.Trigger); // "laser_cast"
        AddParameter("ArmorBuff", AnimatorControllerParameterType.Trigger);   // "sheild_cast"
        AddParameter("Death", AnimatorControllerParameterType.Trigger);
        AddParameter("Summon", AnimatorControllerParameterType.Trigger);      // Optional if different from ArmorBuff

        EditorUtility.SetDirty(controller);
        AssetDatabase.SaveAssets();
        
        Debug.Log("<color=cyan>[Boss Setup] Parameters created successfully!</color>");
    }

    void AddParameter(string name, AnimatorControllerParameterType type)
    {
        if (controller.parameters.Any(p => p.name == name))
            return;

        controller.AddParameter(name, type);
        Debug.Log($"✓ Added parameter: {name} ({type})");
    }

    void SetupTransitions()
    {
        if (controller == null) return;

        Debug.Log("[Boss Setup] Creating transitions...");

        var layers = controller.layers;
        if (layers.Length == 0) return;

        var rootStateMachine = layers[0].stateMachine;

        // Find all states (Case-insensitive matching to be safe, or precise based on clip names)
        // User Clip Names: idle, melee, shoot, laser_cast, sheild_cast, immune, glow, death
        var idle = FindState(rootStateMachine, "idle");
        var melee = FindState(rootStateMachine, "melee");
        var shoot = FindState(rootStateMachine, "shoot"); // RangeAttack
        var laser = FindState(rootStateMachine, "laser_cast");
        var shield = FindState(rootStateMachine, "sheild_cast"); // ArmorBuff
        var immune = FindState(rootStateMachine, "immune");
        var glow = FindState(rootStateMachine, "glow"); // Enraged Idle
        var death = FindState(rootStateMachine, "death");

        // Clear existing transitions
        ClearTransitions(rootStateMachine);

        // === ANY STATE TRANSITIONS ===
        if (death != null)
        {
            var t = rootStateMachine.AddAnyStateTransition(death);
            t.AddCondition(AnimatorConditionMode.If, 0, "Death");
            t.duration = 0f;
            t.canTransitionToSelf = false;
        }

        if (immune != null)
        {
            // Enter Immune state from anywhere if variable is set
            var t = rootStateMachine.AddAnyStateTransition(immune);
            t.AddCondition(AnimatorConditionMode.If, 0, "isImmune");
            t.duration = 0.1f;
            t.canTransitionToSelf = false;
        }

        // === IMMUNE TRANSITIONS ===
        if (immune != null)
        {
            // Exit Immune -> Idle (Normal)
            if (idle != null)
            {
                var t = immune.AddTransition(idle);
                t.AddCondition(AnimatorConditionMode.IfNot, 0, "isImmune");
                t.AddCondition(AnimatorConditionMode.IfNot, 0, "isEnraged");
                t.duration = 0.2f;
            }
            // Exit Immune -> Glow (Enraged)
            if (glow != null)
            {
                var t = immune.AddTransition(glow);
                t.AddCondition(AnimatorConditionMode.IfNot, 0, "isImmune");
                t.AddCondition(AnimatorConditionMode.If, 0, "isEnraged");
                t.duration = 0.2f;
            }
        }

        // === IDLE TRANSITIONS ===
        if (idle != null)
        {
            // Idle -> Glow (Enraged Toggle)
            if (glow != null)
            {
                var t = idle.AddTransition(glow);
                t.AddCondition(AnimatorConditionMode.If, 0, "isEnraged");
                t.duration = 0.5f; // Slowish transition to show power up?
            }

            // Attacks
            CreateAttackTransition(idle, melee, "MeleeAttack");
            CreateAttackTransition(idle, shoot, "RangeAttack");
            CreateAttackTransition(idle, laser, "LaserAttack");
            CreateAttackTransition(idle, shield, "ArmorBuff");
        }

        // === GLOW TRANSITIONS (Enraged Idle) ===
        if (glow != null)
        {
            // Glow -> Idle (Calm down?)
            if (idle != null)
            {
                var t = glow.AddTransition(idle);
                t.AddCondition(AnimatorConditionMode.IfNot, 0, "isEnraged");
                t.duration = 0.5f;
            }

            // Attacks (Enraged version)
            CreateAttackTransition(glow, melee, "MeleeAttack");
            CreateAttackTransition(glow, shoot, "RangeAttack");
            CreateAttackTransition(glow, laser, "LaserAttack");
            CreateAttackTransition(glow, shield, "ArmorBuff");
        }

        // === ATTACK RETURN TRANSITIONS ===
        // Melee -> Idle/Glow
        if (melee != null) CreateReturnTransitions(melee, idle, glow);
        if (shoot != null) CreateReturnTransitions(shoot, idle, glow);
        if (laser != null) CreateReturnTransitions(laser, idle, glow);
        if (shield != null) CreateReturnTransitions(shield, idle, glow);

        EditorUtility.SetDirty(controller);
        AssetDatabase.SaveAssets();
        
        Debug.Log("<color=cyan>[Boss Setup] Transitions created successfully!</color>");
    }

    void CreateAttackTransition(AnimatorState from, AnimatorState to, string trigger)
    {
        if (from == null || to == null) return;
        var t = from.AddTransition(to);
        t.AddCondition(AnimatorConditionMode.If, 0, trigger);
        t.duration = 0f; // Instant reaction
        t.hasExitTime = false;
    }

    void CreateReturnTransitions(AnimatorState attackState, AnimatorState idleState, AnimatorState glowState)
    {
        // Return to Glow if Enraged
        if (glowState != null)
        {
            var t = attackState.AddTransition(glowState);
            t.AddCondition(AnimatorConditionMode.If, 0, "isEnraged");
            t.hasExitTime = true; // Wait for attack to finish
            t.exitTime = 1f;
            t.duration = 0.15f;
        }

        // Return to Idle if NOT Enraged
        if (idleState != null)
        {
            var t = attackState.AddTransition(idleState);
            t.AddCondition(AnimatorConditionMode.IfNot, 0, "isEnraged");
            t.hasExitTime = true; // Wait for attack to finish
            t.exitTime = 1f;
            t.duration = 0.15f;
        }
    }

    AnimatorState FindState(AnimatorStateMachine stateMachine, string name)
    {
        foreach (var state in stateMachine.states)
        {
            if (state.state.name == name)
                return state.state;
        }
        return null; // Silent fail allowed
    }

    void ClearTransitions(AnimatorStateMachine stateMachine)
    {
        for (int i = stateMachine.anyStateTransitions.Length - 1; i >= 0; i--)
            stateMachine.RemoveAnyStateTransition(stateMachine.anyStateTransitions[i]);

        foreach (var childState in stateMachine.states)
        {
            for (int i = childState.state.transitions.Length - 1; i >= 0; i--)
                childState.state.RemoveTransition(childState.state.transitions[i]);
        }
    }
}
