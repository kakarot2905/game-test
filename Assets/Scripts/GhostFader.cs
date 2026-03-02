using UnityEngine;
using System.Collections;

public class GhostFader : MonoBehaviour
{
    public float fadeDuration = 0.5f;
    private SpriteRenderer sr;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        StartCoroutine(FadeAndDestroy());
    }

    IEnumerator FadeAndDestroy()
    {
        float elapsed = 0f;
        Color startColor = sr.color;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(startColor.a, 0f, elapsed / fadeDuration);
            sr.color = new Color(startColor.r, startColor.g, startColor.b, alpha);
            yield return null;
        }

        Destroy(gameObject);
    }
}
