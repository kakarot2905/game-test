using UnityEngine;

public class BiomeTrigger : MonoBehaviour
{
    public float biomeLength = 120f;
    float lastBiomeX;

    void Update()
    {
        float x = transform.position.x;

        if (x > lastBiomeX + biomeLength)
        {
            lastBiomeX = x;
            FindObjectOfType<BiomeFader>().SwitchBiome();
        }
    }
}
