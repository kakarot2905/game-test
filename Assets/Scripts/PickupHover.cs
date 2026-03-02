using UnityEngine;

public class PickupHover : MonoBehaviour
{
    [Header("Hover Settings")]
    public float hoverHeight = 0.15f;
    public float hoverSpeed = 2f;

    [Header("Optional Rotation")]
    public bool rotate = false;
    public float rotateSpeed = 30f;

    Vector3 startPos;

    void Start()
    {
        startPos = transform.position;
    }

    void Update()
    {
        float yOffset = Mathf.Sin(Time.time * hoverSpeed) * hoverHeight;
        transform.position = startPos + Vector3.up * yOffset;

        if (rotate)
            transform.Rotate(Vector3.forward, rotateSpeed * Time.deltaTime);
    }
}
