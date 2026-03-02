using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameOverUI : MonoBehaviour
{
    void OnEnable()
    {
        StyleGameOverScreen();
    }

    void Start()
    {
        StyleGameOverScreen();
    }

    void StyleGameOverScreen()
    {
        RectTransform panelRect = GetComponent<RectTransform>();
        Image panelImage = GetComponent<Image>();

        // Set cosmic gradient background
        panelImage.color = new Color(0.05f, 0.05f, 0.15f, 0.95f); // Deep space color

        // Find and style the Game Over text
        Transform gameOverText = transform.Find("GameOver");
        if (gameOverText != null)
        {
            TextMeshProUGUI text = gameOverText.GetComponent<TextMeshProUGUI>();
            if (text != null)
            {
                text.text = "⚰️ SINGULARITY COLLAPSED ⚰️";
                text.fontSize = 80;
                text.color = new Color(1f, 0.2f, 0.2f, 1f); // Red cosmic
                text.alignment = TextAlignmentOptions.Center;
            }

            RectTransform textRect = gameOverText.GetComponent<RectTransform>();
            if (textRect != null)
            {
                textRect.anchorMin = new Vector2(0.5f, 0.5f);
                textRect.anchorMax = new Vector2(0.5f, 0.5f);
                textRect.anchoredPosition = new Vector2(0, 150);
                textRect.sizeDelta = new Vector2(1200, 200);
            }
        }

        // Find and style the Restart button
        Transform restartButton = transform.Find("Restart");
        if (restartButton != null)
        {
            RectTransform buttonRect = restartButton.GetComponent<RectTransform>();
            if (buttonRect != null)
            {
                buttonRect.anchorMin = new Vector2(0.5f, 0.5f);
                buttonRect.anchorMax = new Vector2(0.5f, 0.5f);
                buttonRect.anchoredPosition = new Vector2(0, -100);
                buttonRect.sizeDelta = new Vector2(300, 100);
            }

            Image buttonImage = restartButton.GetComponent<Image>();
            if (buttonImage != null)
            {
                buttonImage.color = new Color(0.2f, 1f, 1f, 0.9f); // Cyan
            }

            Button button = restartButton.GetComponent<Button>();
            if (button != null)
            {
                ColorBlock colors = button.colors;
                colors.normalColor = new Color(0.2f, 1f, 1f, 0.9f);
                colors.highlightedColor = new Color(0.4f, 1f, 1f, 1f);
                colors.pressedColor = new Color(0.1f, 0.8f, 0.8f, 1f);
                button.colors = colors;
            }

            // Style button text
            TextMeshProUGUI buttonText = restartButton.GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null)
            {
                buttonText.text = "↻ RESTART SIMULATION";
                buttonText.fontSize = 32;
                buttonText.color = Color.white;
            }
        }

        // Remove old subtitle if it exists
        Transform oldSubtitle = transform.Find("Subtitle");
        if (oldSubtitle != null)
        {
            Destroy(oldSubtitle.gameObject);
        }

        // Add subtitle text
        CreateSubtitle();
    }

    void CreateSubtitle()
    {
        GameObject subtitleObj = new GameObject("Subtitle");
        subtitleObj.transform.SetParent(transform, false);

        RectTransform subtitleRect = subtitleObj.AddComponent<RectTransform>();
        subtitleRect.anchorMin = new Vector2(0.5f, 0.5f);
        subtitleRect.anchorMax = new Vector2(0.5f, 0.5f);
        subtitleRect.anchoredPosition = new Vector2(0, 50);
        subtitleRect.sizeDelta = new Vector2(800, 100);

        TextMeshProUGUI subtitleText = subtitleObj.AddComponent<TextMeshProUGUI>();
        subtitleText.text = "The Twin Flame has been extinguished...";
        subtitleText.fontSize = 28;
        subtitleText.color = new Color(0.8f, 0.8f, 1f, 0.8f); // Light blue
        subtitleText.alignment = TextAlignmentOptions.Center;
    }
}
