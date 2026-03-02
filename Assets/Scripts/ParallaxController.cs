using UnityEngine;

[ExecuteAlways]
public class ParallaxController : MonoBehaviour
{
    [System.Serializable]
    public class ParallaxLayer
    {
        public Transform transform;

        [Tooltip("0 = static background, 1 = moves with camera, <1 = background, >1 = foreground")]
        public float parallaxFactor = 0.5f;

        [Tooltip("Enable if this layer should loop infinitely")]
        public bool infiniteX = false;

        [HideInInspector] public float spriteWidth;
        [HideInInspector] public Vector3 startPos;
    }

    [Header("References")]
    public Camera cam;

    [Header("Layers")]
    public ParallaxLayer[] layers;

    Vector3 lastCamPos;

    void Start()
    {
        if (cam == null)
            cam = Camera.main;

        lastCamPos = cam.transform.position;

        // Cache sprite widths and start positions
        foreach (var layer in layers)
        {
            if (layer.transform == null) continue;

            layer.startPos = layer.transform.position;

            var sr = layer.transform.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                layer.spriteWidth = sr.bounds.size.x;
            }
        }
    }

    void LateUpdate()
    {
        if (cam == null) return;

        Vector3 camPos = cam.transform.position;
        Vector3 delta = camPos - lastCamPos;

        foreach (var layer in layers)
        {
            if (layer.transform == null) continue;

            // ---- PARALLAX MOVEMENT ----
            Vector3 pos = layer.transform.position;
            pos.x += delta.x * layer.parallaxFactor;
            layer.transform.position = pos;

            // ---- INFINITE LOOPING ----
            if (layer.infiniteX && layer.spriteWidth > 0f)
            {
                float camDistFromLayer = camPos.x - layer.transform.position.x;

                if (Mathf.Abs(camDistFromLayer) >= layer.spriteWidth)
                {
                    float offset = camDistFromLayer > 0
                        ? layer.spriteWidth
                        : -layer.spriteWidth;

                    layer.transform.position += Vector3.right * offset;
                }
            }
        }

        lastCamPos = camPos;
    }

#if UNITY_EDITOR
    void OnValidate()
    {
        if (cam == null)
            cam = Camera.main;
    }
#endif
}
