using UnityEngine;

[ExecuteAlways]
public class ParallaxControllerForTutorial : MonoBehaviour
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

    [Header("Settings")]
    [Tooltip("How many units BEFORE the edge becomes visible to snap it to the new position. Prevents pop-in.")]
    public float respawnAheadBuffer = 5.0f;

    Vector3 lastCamPos;

    void Start()
    {
        if (cam == null)
            cam = Camera.main;

        if (cam != null)
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

            // ---- INFINITE LOOPING WITH BUFFER ----
            if (layer.infiniteX && layer.spriteWidth > 0f)
            {
                float camDistFromLayer = camPos.x - layer.transform.position.x;
                
                // Use buffer to spawn SOONER (when distance is slightly less than full width)
                // Ensure threshold is at least half width to prevent flip-flopping
                float threshold = Mathf.Max(layer.spriteWidth - respawnAheadBuffer, layer.spriteWidth * 0.55f);

                if (Mathf.Abs(camDistFromLayer) >= threshold)
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
