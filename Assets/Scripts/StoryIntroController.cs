using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;

public class StoryIntroController : MonoBehaviour
{
    [Header("References")]
    public TextMeshProUGUI storyText;
    public CanvasGroup canvas;

    [Header("Timing")]
    public float typeSpeed = 0.03f;
    public float scrollSpeed = 25f;
    public float fadeSpeed = 1f;

    [Header("Text")]
    [TextArea(5, 10)]
    public string story;

    private bool skipped = false;
    private bool scrollingStarted = false;

    private RectTransform textRect;
    private RectTransform canvasRect;

    // 🔒 prevents scroll from outrunning typed text
    private float maxAllowedScrollY = 0f;

    void Start()
    {
        textRect = storyText.rectTransform;
        canvasRect = canvas.GetComponent<RectTransform>();

        storyText.text = "";
        canvas.alpha = 1f;

        StartCoroutine(TypeText());
    }

    void Update()
    {
        if (Input.anyKeyDown && !skipped)
        {
            skipped = true;
            LoadGame();
        }
    }

    IEnumerator TypeText()
    {
        foreach (char c in story)
        {
            if (skipped) yield break;

            storyText.text += c;
            storyText.ForceMeshUpdate(); // REQUIRED

            // Detect overflow → start scrolling
            if (!scrollingStarted && IsTextOverflowing())
            {
                scrollingStarted = true;
                StartCoroutine(ScrollText());
            }

            // 🔒 update max scroll allowed based on content
            UpdateMaxScrollLimit();

            yield return new WaitForSeconds(typeSpeed);
        }

        // If text never overflowed, fade out normally
        if (!scrollingStarted)
            StartCoroutine(FadeAndExit());
    }

    IEnumerator ScrollText()
    {
        while (!skipped)
        {
            Vector2 pos = textRect.anchoredPosition;
            pos.y += scrollSpeed * Time.deltaTime;

            // 🔒 CLAMP so scroll never beats typing
            pos.y = Mathf.Min(pos.y, maxAllowedScrollY);

            textRect.anchoredPosition = pos;

            if (IsTextFullyOffScreen())
                break;

            yield return null;
        }

        StartCoroutine(FadeAndExit());
    }

    IEnumerator FadeAndExit()
    {
        float t = 1f;
        while (t > 0 && !skipped)
        {
            t -= Time.deltaTime * fadeSpeed;
            canvas.alpha = t;
            yield return null;
        }

        LoadGame();
    }

    // -------------------------
    // SCROLL LIMIT CALCULATION
    // -------------------------
    void UpdateMaxScrollLimit()
    {
        float textBottom = storyText.textBounds.min.y;
        float screenBottom = -canvasRect.rect.height / 2f;

        if (textBottom < screenBottom)
        {
            maxAllowedScrollY = screenBottom - textBottom;
        }
    }

    // -------------------------
    // BOUNDS CHECKS
    // -------------------------
    bool IsTextOverflowing()
    {
        float textBottom = storyText.textBounds.min.y + textRect.anchoredPosition.y;
        float screenBottom = -canvasRect.rect.height / 2f;

        return textBottom < screenBottom;
    }

    bool IsTextFullyOffScreen()
    {
        float textTop = storyText.textBounds.max.y + textRect.anchoredPosition.y;
        float screenTop = canvasRect.rect.height / 2f;

        return textTop > screenTop;
    }

    void LoadGame()
    {
        SceneManager.LoadScene("SampleScene");
    }
}
