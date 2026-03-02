using UnityEngine;
using System.Collections.Generic;

public class InfiniteForeground : MonoBehaviour
{
    public Transform cam;

    private List<Transform> tiles = new List<Transform>();
    private float tileWidth;

    void Start()
    {
        foreach (Transform t in transform)
            tiles.Add(t);

        tileWidth = tiles[0].GetComponent<SpriteRenderer>().bounds.size.x;
    }

    void Update()
    {
        float camLeft = cam.position.x - Camera.main.orthographicSize * Camera.main.aspect;

        foreach (Transform tile in tiles)
        {
            if (tile.position.x + tileWidth < camLeft)
            {
                MoveTileToEnd(tile);
            }
        }
    }

    void MoveTileToEnd(Transform tile)
    {
        float rightMost = GetRightMostX();
        tile.position = new Vector3(rightMost + tileWidth, tile.position.y, tile.position.z);
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
    