using UnityEngine;

public class SkillTreeController : MonoBehaviour
{
    public static SkillTreeController Instance { get; private set; }

    public GameObject skillTreeCanvas;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }
        Instance = this;
    }

    void Start()
    {
        // Make sure skill tree starts hidden
        if (skillTreeCanvas != null)
            skillTreeCanvas.SetActive(false);
    }

    void Update()
    {
        // Toggle skill tree with T key
        if (Input.GetKeyDown(KeyCode.T))
        {
            ToggleSkillTree();
        }
    }

    public void ToggleSkillTree()
    {
        if (skillTreeCanvas != null)
        {
            skillTreeCanvas.SetActive(!skillTreeCanvas.activeSelf);
            
            // Pause game when skill tree is open
            if (skillTreeCanvas.activeSelf)
                Time.timeScale = 0f;
            else
                Time.timeScale = 1f;
        }
    }

    public void OpenSkillTree()
    {
        if (skillTreeCanvas != null && !skillTreeCanvas.activeSelf)
        {
            skillTreeCanvas.SetActive(true);
            Time.timeScale = 0f;
        }
    }

    public void CloseSkillTree()
    {
        if (skillTreeCanvas != null && skillTreeCanvas.activeSelf)
        {
            skillTreeCanvas.SetActive(false);
            Time.timeScale = 1f;
        }
    }
}
