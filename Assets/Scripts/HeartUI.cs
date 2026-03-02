using UnityEngine;
using UnityEngine.UI;

public class HeartUI : MonoBehaviour
{
    public Image image;
    
    [Header("Heart Sprites (3-State System)")]
    public Sprite full;    // Value 2
    public Sprite half;    // Value 1
    public Sprite empty;   // Value 0

    public void SetHeart(int value)
    {
        // 3-state system: 0 = empty, 1 = half, 2 = full
        if (value == 2)
            image.sprite = full;
        else if (value == 1)
            image.sprite = half;
        else
            image.sprite = empty;
    }
}