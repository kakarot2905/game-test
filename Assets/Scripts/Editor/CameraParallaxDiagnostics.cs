using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;

public class CameraParallaxDiagnostics
{
    [MenuItem("Cosmic Meows/Diagnostics/Check Parallax Setup")]
    public static void CheckParallaxSetup()
    {
        Debug.Log("=== PARALLAX SETUP DIAGNOSTICS ===\n");

        // Check Camera
        Camera mainCam = Camera.main;
        if (mainCam != null)
        {
            Debug.Log($"✓ Main Camera found at {mainCam.transform.position}");
            Debug.Log($"  - Camera Mode: {(mainCam.orthographic ? "Orthographic" : "Perspective")}");
            Debug.Log($"  - Ortho Size: {mainCam.orthographicSize}");
            
            CameraShake shake = mainCam.GetComponent<CameraShake>();
            if (shake != null)
            {
                Debug.Log($"✓ CameraShake component found");
            }
            else
            {
                Debug.LogWarning("✗ CameraShake component NOT found on Main Camera");
            }

            CameraFollow follow = mainCam.GetComponent<CameraFollow>();
            if (follow != null)
            {
                Debug.Log($"✓ CameraFollow component found");
            }
            else
            {
                Debug.LogWarning("✗ CameraFollow component NOT found on Main Camera");
            }
        }
        else
        {
            Debug.LogError("✗ Main Camera NOT FOUND!");
        }

        Debug.Log("\n--- Background Objects ---");

        // Check Background
        Transform bg = GameObject.Find("Background")?.transform;
        if (bg != null)
        {
            Debug.Log($"✓ Background found");
            SkyFollowCamera skyFollow = bg.GetComponent<SkyFollowCamera>();
            if (skyFollow != null)
            {
                Debug.Log($"  ✓ SkyFollowCamera attached");
                Debug.Log($"    - ParallaxX: {(skyFollow.GetType().GetField("parallaxX", System.Reflection.BindingFlags.NonPublic)?.GetValue(skyFollow) ?? "N/A")}");
                Debug.Log($"    - ParallaxY: {(skyFollow.GetType().GetField("parallaxY", System.Reflection.BindingFlags.NonPublic)?.GetValue(skyFollow) ?? "N/A")}");
            }
            else
            {
                Debug.LogWarning("  ✗ SkyFollowCamera NOT attached");
            }
        }
        else
        {
            Debug.LogError("✗ Background NOT found!");
        }

        // Check Background B
        Transform bgB = GameObject.Find("Background B")?.transform;
        if (bgB != null)
        {
            Debug.Log($"✓ Background B found");
            SkyFollowCamera skyFollow = bgB.GetComponent<SkyFollowCamera>();
            if (skyFollow != null)
            {
                Debug.Log($"  ✓ SkyFollowCamera attached");
            }
            else
            {
                Debug.LogWarning("  ✗ SkyFollowCamera NOT attached");
            }
        }

        Debug.Log("\n--- Foreground Objects ---");

        // Check Foreground
        Transform fg = GameObject.Find("Foreground")?.transform;
        if (fg != null)
        {
            Debug.Log($"✓ Foreground found");
            InfiniteForeground infFg = fg.GetComponent<InfiniteForeground>();
            if (infFg != null)
            {
                Debug.Log($"  ✓ InfiniteForeground attached");
            }
        }

        // Check Foreground B
        Transform fgB = GameObject.Find("Foreground B")?.transform;
        if (fgB != null)
        {
            Debug.Log($"✓ Foreground B found");
            InfiniteForeground infFgB = fgB.GetComponent<InfiniteForeground>();
            if (infFgB != null)
            {
                Debug.Log($"  ✓ InfiniteForeground attached");
            }
        }

        Debug.Log("\n=== END DIAGNOSTICS ===");
    }
}
#endif
