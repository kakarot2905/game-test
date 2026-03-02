using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Handles the close button functionality
/// Attach this to the close button (done automatically by builder)
/// </summary>
public class CloseButtonHandler : MonoBehaviour
{
    void Start()
    {
        Button btn = GetComponent<Button>();
        if (btn != null)
        {
            btn.onClick.AddListener(OnClick);
        }
    }

    void OnClick()
    {
        // Close the skill tree via SkillTreeController
        if (SkillTreeController.Instance != null)
        {
            SkillTreeController.Instance.CloseSkillTree();
        }
        else
        {
            // Fallback: just hide this canvas
            transform.root.gameObject.SetActive(false);
        }
    }
}
