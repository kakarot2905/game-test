using UnityEngine;
using System.Collections;

public class BiomeFader : MonoBehaviour
{
    [Header("Backgrounds")]
    public SpriteRenderer bgA;
    public SpriteRenderer bgB;

    [Header("Foreground Layers")]
    public SpriteRenderer[] fgA;
    public SpriteRenderer[] fgB;

    [Header("Fade Settings")]
    public float fadeDuration = 3f;

    bool isBiomeB = false;
    bool isFading = false;

    void Start()
    {
        SetAlpha(bgA, 1);
        SetAlpha(bgB, 0);

        SetAlpha(fgA, 1);
        SetAlpha(fgB, 0);
    }

    public void SwitchBiome()
    {
        if (isFading) return;
        StartCoroutine(FadeBiome());
    }

    IEnumerator FadeBiome()
    {
        isFading = true;

        float t = 0f;

        float fromA = isBiomeB ? 0 : 1;
        float toA = isBiomeB ? 1 : 0;

        float fromB = isBiomeB ? 1 : 0;
        float toB = isBiomeB ? 0 : 1;

        while (t < 1)
        {
            t += Time.deltaTime / fadeDuration;

            float a = Mathf.Lerp(fromA, toA, t);
            float b = Mathf.Lerp(fromB, toB, t);

            SetAlpha(bgA, a);
            SetAlpha(bgB, b);

            SetAlpha(fgA, a);
            SetAlpha(fgB, b);

            yield return null;
        }

        isBiomeB = !isBiomeB;
        isFading = false;
    }

    // =====================
    // Helpers
    // =====================
    void SetAlpha(SpriteRenderer sr, float a)
    {
        if (sr == null) return;
        Color c = sr.color;
        c.a = a;
        sr.color = c;
    }

    void SetAlpha(SpriteRenderer[] list, float a)
    {
        foreach (var sr in list)
            SetAlpha(sr, a);
    }
}
