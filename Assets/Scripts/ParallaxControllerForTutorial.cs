using UnityEngine;
using System.Collections.Generic;

public class ParallaxControllerForTutorial : MonoBehaviour
{
    [System.Serializable]
    public class ParallaxLayer
    {
        public Transform transform;
        [Tooltip("0 = static background, 1 = moves with camera")]
        [Range(0f, 1f)] public float parallaxFactor = 0.5f;
    }

    [Header("References")]
    public Camera cam;

    [Header("Layers")]
    public ParallaxLayer[] layers;

    // Internal class to manage the 3-panel system for a single layer
    private class InfiniteLayer
    {
        public Transform[] panels; // [0]=Left, [1]=Center, [2]=Right
        public float width;
        public float parallaxFactor;
    }

    private List<InfiniteLayer> infiniteLayers = new List<InfiniteLayer>();
    private Vector3 lastCamPos;

    void Start()
    {
        if (cam == null) cam = Camera.main;
        lastCamPos = cam.transform.position;

        foreach (var layerSettings in layers)
        {
            if (layerSettings.transform == null) continue;

            // 1. Get Width
            SpriteRenderer sr = layerSettings.transform.GetComponent<SpriteRenderer>();
            if (sr == null)
            {
                Debug.LogWarning($"[Parallax] Layer '{layerSettings.transform.name}' has no SpriteRenderer! Skipping.");
                continue;
            }
            float width = sr.bounds.size.x;

            // 2. Setup Panels (Left, Center, Right)
            Transform center = layerSettings.transform;
            
            // Create Clones
            Transform left = Instantiate(center, center.parent);
            left.name = center.name + "_Left";
            
            Transform right = Instantiate(center, center.parent);
            right.name = center.name + "_Right";

            // Position them
            // Ensure we use localPosition relative to parent or world depending on context
            // Assuming flat hierarchy for parallax, world position is safest for now
            Vector3 centerPos = center.position;
            left.position = centerPos + Vector3.left * width;
            right.position = centerPos + Vector3.right * width;

            // 3. Store in runtime list
            InfiniteLayer il = new InfiniteLayer
            {
                panels = new Transform[] { left, center, right },
                width = width,
                parallaxFactor = layerSettings.parallaxFactor
            };
            infiniteLayers.Add(il);
        }
    }

    void LateUpdate()
    {
        if (cam == null) return;

        float camX = cam.transform.position.x;
        float deltaX = camX - lastCamPos.x;

        foreach (var il in infiniteLayers)
        {
            // 1. Move all panels based on parallax factor (relative movement)
            // If factor is 1, it moves exactly with camera (static relative to camera).
            // If factor is 0, it stays static in world (moves -delta relative to camera).
            float moveAmount = deltaX * il.parallaxFactor;

            for (int i = 0; i < il.panels.Length; i++)
            {
                Vector3 p = il.panels[i].position;
                p.x += moveAmount;
                il.panels[i].position = p;
            }

            // 2. Check for Looping (Shift panels to keep center near camera)
            // The "Center" panel is always at index [1]
            Transform centerPanel = il.panels[1];
            float distFromCenter = camX - centerPanel.position.x;

            // If camera moved too far RIGHT (past half width + buffer)
            if (distFromCenter > il.width / 2f)
            {
                ShiftRight(il);
            }
            // If camera moved too far LEFT
            else if (distFromCenter < -il.width / 2f)
            {
                ShiftLeft(il);
            }
        }

        lastCamPos = cam.transform.position;
    }

    // Move left panel to the far right
    void ShiftRight(InfiniteLayer il)
    {
        Transform left = il.panels[0];
        Transform center = il.panels[1];
        Transform right = il.panels[2];

        // Move Left to be the new Right neighbor of the current Right
        Vector3 newPos = right.position; 
        newPos.x += il.width;
        left.position = newPos;

        // Update Array Indices: [0]->recycling, [1]->0, [2]->1
        // New Array: [OldCenter, OldRight, OldLeft]
        il.panels[0] = center;
        il.panels[1] = right;
        il.panels[2] = left;
    }

    // Move right panel to the far left
    void ShiftLeft(InfiniteLayer il)
    {
        Transform left = il.panels[0];
        Transform center = il.panels[1];
        Transform right = il.panels[2];

        // Move Right to be the new Left neighbor of the current Left
        Vector3 newPos = left.position;
        newPos.x -= il.width;
        right.position = newPos;

        // Update Array Indices
        // New Array: [OldRight, OldLeft, OldCenter]
        il.panels[0] = right;
        il.panels[1] = left;
        il.panels[2] = center;
    }
}
