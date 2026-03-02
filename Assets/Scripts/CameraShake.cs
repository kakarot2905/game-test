using UnityEngine;
using System.Collections;

/// <summary>
/// Adds screen shake effect to the camera for impactful feedback.
/// Attach to the Main Camera.
/// </summary>
public class CameraShake : MonoBehaviour
{
    public static CameraShake Instance { get; private set; }

    [Header("Default Settings")]
    [SerializeField] private float defaultDuration = 0.15f;
    [SerializeField] private float defaultMagnitude = 0.2f;

    private Coroutine shakeCoroutine;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }
        Instance = this;
    }

    /// <summary>
    /// Trigger a camera shake with default values.
    /// </summary>
    public void Shake()
    {
        Shake(defaultDuration, defaultMagnitude);
    }

    /// <summary>
    /// Trigger a camera shake with custom duration and magnitude.
    /// </summary>
    public void Shake(float duration, float magnitude)
    {
        if (shakeCoroutine != null)
            StopCoroutine(shakeCoroutine);
        
        shakeCoroutine = StartCoroutine(DoShake(duration, magnitude));
    }

    /// <summary>
    /// Trigger a small shake for minor hits.
    /// </summary>
    public void ShakeSmall()
    {
        Shake(0.1f, 0.1f);
    }

    /// <summary>
    /// Trigger a large shake for heavy hits.
    /// </summary>
    public void ShakeLarge()
    {
        Shake(0.25f, 0.4f);
    }

    /// <summary>
    /// Stops any ongoing shake immediately.
    /// </summary>
    public void StopShake()
    {
        if (shakeCoroutine != null)
        {
            StopCoroutine(shakeCoroutine);
            shakeCoroutine = null;
        }
    }

    private IEnumerator DoShake(float duration, float magnitude)
    {
        // Capture the position at the START of the shake (the target position that CameraFollow is moving towards)
        Vector3 targetPosition = transform.position;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;

            transform.position = new Vector3(
                targetPosition.x + x,
                targetPosition.y + y,
                targetPosition.z
            );

            elapsed += Time.deltaTime;
            yield return null;
        }

        // Return to the target position (not originalPosition)
        transform.position = targetPosition;
        shakeCoroutine = null;
    }
}
