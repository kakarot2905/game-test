using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class SkillTreeUI : MonoBehaviour
{
    [Header("Colors")]
    public Color unlockedColor = new Color(0f, 1f, 1f, 1f);      // Cyan glow
    public Color availableColor = new Color(0.5f, 0.5f, 0.5f, 1f); // Gray
    public Color lockedColor = new Color(0.2f, 0.2f, 0.2f, 1f);   // Dark gray
    public Color lineColor = new Color(0f, 1f, 1f, 0.6f);         // Cyan line

    [Header("Node Settings")]
    public float nodeSize = 100f;
    public float verticalSpacing = 180f;
    public float horizontalSpacing = 300f;

    private GameObject tooltipObj;
    private TextMeshProUGUI tooltipText;
    
    void Start()
    {
        Debug.Log("🌟 NEW SKILL TREE UI LOADING! 🌟");
        
        // CRITICAL: Destroy all old UI elements first
        ClearOldUI();
        
        SetupCanvas();
        CreateSkillTree();
        
        gameObject.SetActive(false);
        Debug.Log("✅ Skill tree created with diamond nodes!");
    }

    void ClearOldUI()
    {
        // Remove all existing children (old grid UI)
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }
        Debug.Log("🗑️ Cleared old skill tree UI");
    }

    void SetupCanvas()
    {
        Canvas canvas = GetComponent<Canvas>();
        if (canvas == null)
        {
            canvas = gameObject.AddComponent<Canvas>();
        }
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        CanvasScaler scaler = GetComponent<CanvasScaler>();
        if (scaler == null)
        {
            scaler = gameObject.AddComponent<CanvasScaler>();
        }
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
    }

    void CreateSkillTree()
    {
        CreateBackground();
        CreateTitle();
        CreateTooltip();
        
        // Define skill tree structure based on user's sketch
        CreateMovementBranch();
        CreateCommonBranch();
        CreateAttackBranch();
        
        CreateCloseButton();
    }

    void CreateBackground()
    {
        GameObject bg = new GameObject("Background");
        bg.transform.SetParent(transform, false);
        
        RectTransform rect = bg.AddComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        
        Image img = bg.AddComponent<Image>();
        img.color = new Color(0.05f, 0.05f, 0.15f, 0.95f);
    }

    void CreateTitle()
    {
        GameObject title = new GameObject("Title");
        title.transform.SetParent(transform, false);
        
        TextMeshProUGUI text = title.AddComponent<TextMeshProUGUI>();
        text.text = "⚡ SKILL TREE ⚡";
        text.fontSize = 60;
        text.alignment = TextAlignmentOptions.Center;
        text.color = new Color(0f, 1f, 1f, 1f);
        
        RectTransform rect = title.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 1f);
        rect.anchorMax = new Vector2(0.5f, 1f);
        rect.anchoredPosition = new Vector2(0, -50);
        rect.sizeDelta = new Vector2(800, 80);
        
        // Skill Points display
        GameObject points = new GameObject("SkillPoints");
        points.transform.SetParent(transform, false);
        
        TextMeshProUGUI pointsText = points.AddComponent<TextMeshProUGUI>();
        pointsText.text = "Skill Points: 0";
        pointsText.fontSize = 36;
        pointsText.alignment = TextAlignmentOptions.Center;
        pointsText.color = new Color(1f, 0.84f, 0f, 1f);
        
        RectTransform pointsRect = points.GetComponent<RectTransform>();
        pointsRect.anchorMin = new Vector2(0.5f, 1f);
        pointsRect.anchorMax = new Vector2(0.5f, 1f);
        pointsRect.anchoredPosition = new Vector2(0, -120);
        pointsRect.sizeDelta = new Vector2(400, 60);
        
        // Store reference for SkillTreeManager
        if (SkillTreeManager.Instance != null)
            SkillTreeManager.Instance.pointsText = pointsText;
    }

    void CreateMovementBranch()
    {
        Vector2 basePos = new Vector2(-horizontalSpacing, 100);
        
        // Branch label
        CreateLabel("Movement Cat", new Vector2(basePos.x, basePos.y + 80));
        
        // Tier 1 - Dash (already unlocked)
        bool dashUnlocked = SkillTreeManager.Instance != null && SkillTreeManager.Instance.IsUnlocked(Skill.Dash);
        GameObject dash = CreateDiamondNode("Dash", basePos, "Quick dash ability", Skill.Dash, dashUnlocked);
        
        // Tier 2
        Vector2 tier2Pos = basePos + new Vector2(0, -verticalSpacing);
        // Note: Skill 100 is not in Enum, assuming placeholder or mapped to existing. 
        // For safety, I will check if it's unlocked if it was a real skill. 
        // Logic remains as is for non-skills, BUT for actual skills we must check.
        GameObject speed = CreateDiamondNode("Speed Boost", tier2Pos, "Increases movement speed", (Skill)100, false);
        CreateConnectionLine(basePos, tier2Pos);
        
        // Tier 3
        Vector2 tier3Pos = tier2Pos + new Vector2(0, -verticalSpacing);
        bool wallUnlocked = SkillTreeManager.Instance != null && SkillTreeManager.Instance.IsUnlocked(Skill.WallSlide);
        GameObject wall = CreateDiamondNode("Wall Jump", tier3Pos, "Jump off walls", Skill.WallSlide, wallUnlocked);
        CreateConnectionLine(tier2Pos, tier3Pos);
        
        // Tier 4
        Vector2 tier4Pos = tier3Pos + new Vector2(0, -verticalSpacing);
        // Assuming (Skill)102 is Air Dash if implemented later, check real enum if exists.
        GameObject air = CreateDiamondNode("Air Dash", tier4Pos, "Dash in mid-air", (Skill)102, false);
        CreateConnectionLine(tier3Pos, tier4Pos);
    }

    void CreateCommonBranch()
    {
        Vector2 basePos = new Vector2(0, 100);
        
        // Branch label
        CreateLabel("Common", new Vector2(basePos.x, basePos.y + 80));
        
        // Tier 1
        bool hpUnlocked = SkillTreeManager.Instance != null && SkillTreeManager.Instance.IsUnlocked(Skill.IncreaseMaxHP);
        GameObject health = CreateDiamondNode("Max HP", basePos, "Increase maximum health", Skill.IncreaseMaxHP, hpUnlocked);
        
        // Tier 2 - Split into two
        Vector2 tier2Left = basePos + new Vector2(-80, -verticalSpacing);
        Vector2 tier2Right = basePos + new Vector2(80, -verticalSpacing);
        
        bool healUnlocked = SkillTreeManager.Instance != null && SkillTreeManager.Instance.IsUnlocked(Skill.IncreaseHealSlot);
        GameObject heal = CreateDiamondNode("Heal+", tier2Left, "More healing from pickups", Skill.IncreaseHealSlot, healUnlocked);
        
        bool shieldUnlocked = SkillTreeManager.Instance != null && SkillTreeManager.Instance.IsUnlocked(Skill.IncreaseShieldSlot);
        GameObject shield = CreateDiamondNode("Shield+", tier2Right, "Longer shield duration", Skill.IncreaseShieldSlot, shieldUnlocked);
        
        CreateConnectionLine(basePos, tier2Left);
        CreateConnectionLine(basePos, tier2Right);
        
        // Tier 3 - Merge back
        Vector2 tier3Pos = basePos + new Vector2(0, -verticalSpacing * 2);
        GameObject regen = CreateDiamondNode("Regen", tier3Pos, "Slowly regenerate health", (Skill)103, false);
        
        CreateConnectionLine(tier2Left, tier3Pos);
        CreateConnectionLine(tier2Right, tier3Pos);
    }

    void CreateAttackBranch()
    {
        Vector2 basePos = new Vector2(horizontalSpacing, 100);
        
        // Branch label
        CreateLabel("Attack Cat", new Vector2(basePos.x, basePos.y + 80));
        
        // Tier 1 - PowerShot (already unlocked)
        bool shotUnlocked = SkillTreeManager.Instance != null && SkillTreeManager.Instance.IsUnlocked(Skill.PowerShot);
        GameObject power = CreateDiamondNode("Power Shot", basePos, "Shoot projectiles", Skill.PowerShot, shotUnlocked);
        
        // Tier 2
        Vector2 tier2Pos = basePos + new Vector2(0, -verticalSpacing);
        bool meleeUnlocked = SkillTreeManager.Instance != null && SkillTreeManager.Instance.IsUnlocked(Skill.MeleeAttack);
        // Note: Renaming "Pierce" to "Melee Attack" based on ID mapping in Manager or just mapping Skill.MeleeAttack here
        GameObject pierce = CreateDiamondNode("Melee Attack", tier2Pos, "Close range attack", Skill.MeleeAttack, meleeUnlocked);
        CreateConnectionLine(basePos, tier2Pos);
        
        // Tier 3
        Vector2 tier3Pos = tier2Pos + new Vector2(0, -verticalSpacing);
        bool comboUnlocked = SkillTreeManager.Instance != null && SkillTreeManager.Instance.IsUnlocked(Skill.MeleeCombo);
        GameObject burst = CreateDiamondNode("Melee Combo", tier3Pos, "Double hit combo", Skill.MeleeCombo, comboUnlocked);
        CreateConnectionLine(tier2Pos, tier3Pos);
        
        // Tier 4
        Vector2 tier4Pos = tier3Pos + new Vector2(0, -verticalSpacing);
        GameObject homing = CreateDiamondNode("Homing", tier4Pos, "Projectiles track enemies", (Skill)106, false);
        CreateConnectionLine(tier3Pos, tier4Pos);
    }

    void CreateLabel(string text, Vector2 position)
    {
        GameObject label = new GameObject("Label_" + text);
        label.transform.SetParent(transform, false);
        
        TextMeshProUGUI labelText = label.AddComponent<TextMeshProUGUI>();
        labelText.text = text;
        labelText.fontSize = 28;
        labelText.alignment = TextAlignmentOptions.Center;
        labelText.color = new Color(0.7f, 0.7f, 0.7f, 1f);
        
        RectTransform rect = label.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = new Vector2(200, 50);
    }

    GameObject CreateDiamondNode(string skillName, Vector2 position, string description, Skill skill, bool isUnlocked)
    {
        GameObject node = new GameObject("Node_" + skillName);
        node.transform.SetParent(transform, false);
        
        RectTransform rect = node.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = new Vector2(nodeSize, nodeSize);
        rect.rotation = Quaternion.Euler(0, 0, 45); // Diamond rotation
        
        // Background
        Image img = node.AddComponent<Image>();
        img.color = isUnlocked ? unlockedColor : availableColor;
        
        // Button
        Button btn = node.AddComponent<Button>();
        btn.targetGraphic = img;
        
        // Add glow outline for unlocked
        if (isUnlocked)
        {
            Outline outline = node.AddComponent<Outline>();
            outline.effectColor = new Color(0f, 1f, 1f, 1f);
            outline.effectDistance = new Vector2(4, 4);
        }
        
        // Icon container (un-rotated)
        GameObject iconContainer = new GameObject("IconContainer");
        iconContainer.transform.SetParent(node.transform, false);
        
        RectTransform iconRect = iconContainer.AddComponent<RectTransform>();
        iconRect.anchorMin = Vector2.zero;
        iconRect.anchorMax = Vector2.one;
        iconRect.offsetMin = Vector2.zero;
        iconRect.offsetMax = Vector2.zero;
        iconRect.rotation = Quaternion.Euler(0, 0, -45); // Counter-rotate
        
        // Icon text
        TextMeshProUGUI iconText = iconContainer.AddComponent<TextMeshProUGUI>();
        iconText.text = "✦";
        iconText.fontSize = 40;
        iconText.alignment = TextAlignmentOptions.Center;
        iconText.color = isUnlocked ? Color.white : new Color(0.5f, 0.5f, 0.5f, 1f);
        
        // Skill name below node
        GameObject nameObj = new GameObject("Name");
        nameObj.transform.SetParent(transform, false);
        
        RectTransform nameRect = nameObj.AddComponent<RectTransform>();
        nameRect.anchorMin = new Vector2(0.5f, 0.5f);
        nameRect.anchorMax = new Vector2(0.5f, 0.5f);
        nameRect.anchoredPosition = position + new Vector2(0, -nodeSize);
        nameRect.sizeDelta = new Vector2(150, 40);
        
        TextMeshProUGUI nameText = nameObj.AddComponent<TextMeshProUGUI>();
        nameText.text = skillName;
        nameText.fontSize = 20;
        nameText.alignment = TextAlignmentOptions.Center;
        nameText.color = isUnlocked ? unlockedColor : Color.white;
        
        // Hover tooltip
        EventTrigger trigger = node.AddComponent<EventTrigger>();
        
        EventTrigger.Entry enter = new EventTrigger.Entry();
        enter.eventID = EventTriggerType.PointerEnter;
        enter.callback.AddListener((data) => ShowTooltip(description, position));
        trigger.triggers.Add(enter);
        
        EventTrigger.Entry exit = new EventTrigger.Entry();
        exit.eventID = EventTriggerType.PointerExit;
        exit.callback.AddListener((data) => HideTooltip());
        trigger.triggers.Add(exit);
        
        // Click handler
        btn.onClick.AddListener(() => OnNodeClicked(skill));
        
        return node;
    }

    void CreateConnectionLine(Vector2 from, Vector2 to)
    {
        GameObject line = new GameObject("ConnectionLine");
        line.transform.SetParent(transform, false);
        
        RectTransform rect = line.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        
        // Calculate line position and size
        Vector2 direction = to - from;
        float distance = direction.magnitude;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        
        rect.anchoredPosition = (from + to) / 2f;
        rect.sizeDelta = new Vector2(distance, 3);
        rect.rotation = Quaternion.Euler(0, 0, angle);
        
        Image img = line.AddComponent<Image>();
        img.color = lineColor;
        
        // Send to back
        line.transform.SetAsFirstSibling();
    }

    void CreateTooltip()
    {
        tooltipObj = new GameObject("Tooltip");
        tooltipObj.transform.SetParent(transform, false);
        tooltipObj.SetActive(false);
        
        RectTransform rect = tooltipObj.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = new Vector2(300, 100);
        
        Image bg = tooltipObj.AddComponent<Image>();
        bg.color = new Color(0f, 0f, 0f, 0.9f);
        
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(tooltipObj.transform, false);
        
        RectTransform textRect = textObj.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = new Vector2(10, 10);
        textRect.offsetMax = new Vector2(-10, -10);
        
        tooltipText = textObj.AddComponent<TextMeshProUGUI>();
        tooltipText.fontSize = 18;
        tooltipText.alignment = TextAlignmentOptions.Center;
        tooltipText.color = new Color(0f, 1f, 1f, 1f);
    }

    void ShowTooltip(string text, Vector2 nodePos)
    {
        tooltipText.text = text;
        tooltipObj.SetActive(true);
        
        RectTransform rect = tooltipObj.GetComponent<RectTransform>();
        rect.anchoredPosition = nodePos + new Vector2(0, 120);
    }

    void HideTooltip()
    {
        tooltipObj.SetActive(false);
    }

    void OnNodeClicked(Skill skill)
    {
        if (SkillTreeManager.Instance != null)
            SkillTreeManager.Instance.OnSkillClicked(skill);
    }

    void CreateCloseButton()
    {
        GameObject closeBtn = new GameObject("CloseButton");
        closeBtn.transform.SetParent(transform, false);
        
        RectTransform rect = closeBtn.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(1f, 1f);
        rect.anchorMax = new Vector2(1f, 1f);
        rect.anchoredPosition = new Vector2(-60, -60);
        rect.sizeDelta = new Vector2(80, 80);
        
        Image img = closeBtn.AddComponent<Image>();
        img.color = new Color(1f, 0.3f, 0.3f, 0.8f);
        
        Button btn = closeBtn.AddComponent<Button>();
        btn.targetGraphic = img;
        btn.onClick.AddListener(() => gameObject.SetActive(false));
        
        GameObject text = new GameObject("Text");
        text.transform.SetParent(closeBtn.transform, false);
        
        RectTransform textRect = text.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;
        
        TextMeshProUGUI closeText = text.AddComponent<TextMeshProUGUI>();
        closeText.text = "✕";
        closeText.fontSize = 50;
        closeText.alignment = TextAlignmentOptions.Center;
        closeText.color = Color.white;
    }
}
