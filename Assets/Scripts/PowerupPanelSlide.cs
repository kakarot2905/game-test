using UnityEngine;

public class PowerupPanelSlide : MonoBehaviour
{
    public RectTransform panel;

    [Header("Widths")]
    public float collapsedWidth = 48f;
    public float expandedWidth = 160f;

    [Header("Speed")]
    public float resizeSpeed = 600f;

    [Header("Contents")]
    public GameObject[] contents; // HealSlot, ShieldSlot

    int hoverCount = 0;

    void Start()
    {
        SetWidth(collapsedWidth);
        SetContents(false);
    }

    void Update()
    {
        float targetWidth = hoverCount > 0 ? expandedWidth : collapsedWidth;

        float newWidth = Mathf.MoveTowards(
            panel.sizeDelta.x,
            targetWidth,
            resizeSpeed * Time.deltaTime
        );

        panel.sizeDelta = new Vector2(newWidth, panel.sizeDelta.y);
    }

    public void HoverEnter()
    {
        hoverCount++;
        SetContents(true); // 👈 IMMEDIATE ENABLE
    }

    public void HoverExit()
    {
        hoverCount = Mathf.Max(0, hoverCount - 1);

        if (hoverCount == 0)
            SetContents(false); // 👈 IMMEDIATE DISABLE
    }

    void SetWidth(float w)
    {
        panel.sizeDelta = new Vector2(w, panel.sizeDelta.y);
    }

    void SetContents(bool state)
    {
        foreach (var obj in contents)
            obj.SetActive(state);
    }
}
