using UnityEngine;
using System.Collections;

public class GhostTrail : MonoBehaviour
{
    public float spawnInterval = 0.1f;
    public float fadeDuration = 0.5f;
    public Color ghostColor = new Color(0.5f, 0.5f, 1f, 0.5f); // Bluish transparent
    public Material ghostMaterial; // Optional: Assign a specific material (like a simple sprite material)

    private SpriteRenderer sr;
    private float timer;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        if (sr == null) sr = GetComponent<SpriteRenderer>();
        if (sr == null) return; // Need a sprite renderer to copy

        timer += Time.deltaTime;
        if (timer >= spawnInterval)
        {
            SpawnGhost();
            timer = 0;
        }
    }

    void SpawnGhost()
    {
        GameObject ghostObj = new GameObject("Ghost");
        ghostObj.transform.position = transform.position;
        ghostObj.transform.rotation = transform.rotation;
        ghostObj.transform.localScale = transform.localScale;

        SpriteRenderer ghostSr = ghostObj.AddComponent<SpriteRenderer>();
        ghostSr.sprite = sr.sprite;
        ghostSr.color = ghostColor;
        ghostSr.flipX = sr.flipX;
        ghostSr.flipY = sr.flipY;
        ghostSr.sortingLayerID = sr.sortingLayerID;
        ghostSr.sortingOrder = sr.sortingOrder - 1; // Behind player

        if (ghostMaterial != null)
            ghostSr.material = ghostMaterial;

        GhostFader fader = ghostObj.AddComponent<GhostFader>();
        fader.fadeDuration = fadeDuration;
    }
}
