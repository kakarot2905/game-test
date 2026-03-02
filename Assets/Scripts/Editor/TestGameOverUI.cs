using UnityEditor;
using UnityEngine;

public class TestGameOverUI
{
    [MenuItem("Tools/Show Game Over UI")]
    public static void ShowGameOverUI()
    {
        var gameOverManager = Object.FindObjectOfType<GameOverManager>();
        if (gameOverManager != null)
        {
            gameOverManager.ShowGameOver();
            Debug.Log("Game Over UI shown for preview");
        }
        else
        {
            Debug.LogError("GameOverManager not found in scene");
        }
    }

    [MenuItem("Tools/Show Skill Tree")]
    public static void ShowSkillTree()
    {
        var skillTreeController = Object.FindObjectOfType<SkillTreeController>();
        if (skillTreeController != null)
        {
            skillTreeController.OpenSkillTree();
            Debug.Log("Skill Tree shown for preview");
        }
        else
        {
            Debug.LogError("SkillTreeController not found in scene");
        }
    }
}
