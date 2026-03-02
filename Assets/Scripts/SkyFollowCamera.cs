using UnityEngine;

public class SkyFollowCamera : MonoBehaviour
{
    public Transform cam;
    public float parallax = 0.2f;   // lower = slower sky

    Vector3 lastCamPos;

    void Start()
    {
        lastCamPos = cam.position;
    }

    void LateUpdate()
    {
        Vector3 delta = cam.position - lastCamPos;
        transform.position += new Vector3(delta.x * parallax, delta.y * parallax, 0);
        lastCamPos = cam.position;
    }
}
