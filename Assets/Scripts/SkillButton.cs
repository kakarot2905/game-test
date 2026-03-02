using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SkillButton : MonoBehaviour
{
    public Skill skill;                      // set in inspector for each button
    public Image frameImage;                 // optional to tint when unlocked
    public Color unlockedColor = new Color(1f, 0.84f, 0f, 1f);
    public Color defaultColor = Color.gray;

    Button btn;

    void Awake()
    {
        btn = GetComponent<Button>();
        if (btn != null)
            btn.onClick.AddListener(OnClicked);
    }

    void Start()
    {
        Refresh();
    }

    public void OnClicked()
    {
        SkillTreeManager.Instance.OnSkillClicked(skill);
        Refresh();
    }

    public void Refresh()
    {
        if (frameImage != null)
            frameImage.color = SkillTreeManager.Instance.IsUnlocked(skill) ? unlockedColor : defaultColor;
    }
}
