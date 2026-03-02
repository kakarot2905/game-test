using UnityEngine;
using UnityEngine.EventSystems;

public class PowerupPanelHoverForward : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public PowerupPanelSlide controller;

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (controller != null) controller.HoverEnter();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (controller != null) controller.HoverExit();
    }
}
