using UnityEngine;

public class PlayerHealthUI : MonoBehaviour
{
    public HeartUI[] hearts;
    PlayerController player;

    void Start()
    {
        player = FindObjectOfType<PlayerController>();
    }

    void Update()
    {
        int hp = player.GetHP();

        for (int i = 0; i < hearts.Length; i++)
        {
            // Each heart represents 20 HP (100 HP total for 5 hearts)
            int heartValue = Mathf.Clamp(hp - (i * 20), 0, 20);
            
            // Convert to 3-state system: Full(2), Half(1), Empty(0)
            int displayValue;
            if (heartValue >= 15)
                displayValue = 2;      // 15-20 HP -> Full heart
            else if (heartValue >= 1)
                displayValue = 1;      // 1-14 HP -> Half heart
            else
                displayValue = 0;      // 0 HP -> Empty heart
            
            hearts[i].SetHeart(displayValue);
        }
    }

    public void PlayDamageEffect()
    {
        foreach (var heart in hearts)
            heart.GetComponent<HeartShake>()?.Shake();
    }

    public void RevealExtraHeart()
    {
        // Show the 6th heart (index 5) if it exists
        if (hearts.Length > 5 && hearts[5] != null)
        {
            hearts[5].gameObject.SetActive(true);
            Debug.Log("[HealthUI] 6th heart revealed!");
        }
    }

}
