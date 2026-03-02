using UnityEngine;

public class InfiniteParallax : MonoBehaviour
{
    public Transform cam;
    public bool followCamera = false;   // TRUE for background, FALSE for foreground
    public float parallaxSpeed = 0.5f;  // Only used when followCamera = true

    private Transform[] tiles;
    private float tileWidth;

    void Start()
    {
        tiles = new Transform[transform.childCount];
        for (int i = 0; i < transform.childCount; i++)
            tiles[i] = transform.GetChild(i);

        tileWidth = tiles[0].GetComponent<SpriteRenderer>().bounds.size.x;
    }

    void Update()
    {
        // Background parallax
        if (followCamera)
        {
            transform.position = new Vector3(
                cam.position.x * parallaxSpeed,
                transform.position.y,
                transform.position.z
            );
        }

        // Tile looping
        foreach (Transform t in tiles)
        {
            if (t.position.x + tileWidth < cam.position.x - Camera.main.orthographicSize * Camera.main.aspect)
            {
                float rightMost = GetRightMostX();
                t.position = new Vector3(rightMost + tileWidth, t.position.y, t.position.z);
            }
        }
    }

    float GetRightMostX()
    {
        float max = tiles[0].position.x;
        foreach (Transform t in tiles)
            if (t.position.x > max)
                max = t.position.x;
        return max;
    }
}
