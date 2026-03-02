using UnityEngine;

public class HeartShake : MonoBehaviour
{
    Vector3 originalPos;
    float shakeTimer;
    float intensity;

    void Start()
    {
        originalPos = transform.localPosition;
    }

    public void Shake(float duration = 0.15f, float power = 10f)
    {
        shakeTimer = duration;
        intensity = power;
    }

    void Update()
    {
        if (shakeTimer > 0)
        {
            shakeTimer -= Time.deltaTime;

            transform.localPosition = originalPos +
                (Vector3)Random.insideUnitCircle * intensity;

            if (shakeTimer <= 0)
                transform.localPosition = originalPos;
        }
    }
}
