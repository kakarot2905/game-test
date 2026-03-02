using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PlayerFormUI : MonoBehaviour
{
    [Header("Player Forms")]
    public GameObject movementCat;
    public GameObject attackCat;

    [Header("Icons")]
    public Image movementIcon;
    public Image attackIcon;

    [Header("Colors")]
    public Color activeColor = Color.white;
    public Color inactiveColor = new Color(1, 1, 1, 0.35f);

    [Header("Effects")]
    public float popScale = 1.4f;
    public float shakeAmount = 6f;
    public float animSpeed = 10f;

    bool lastAttackMode = false;

    void Update()
    {
        // ✅ SAFETY: player (and cats) may be destroyed
        if (movementCat == null || attackCat == null)
        {
            // Hide UI safely when player is dead
            movementIcon.enabled = false;
            attackIcon.enabled = false;
            return;
        }

        // Ensure icons are visible again when player exists
        movementIcon.enabled = true;
        attackIcon.enabled = true;

        bool isAttackMode = attackCat.activeInHierarchy;

        if (isAttackMode != lastAttackMode)
        {
            StartCoroutine(PlaySwitchEffect(isAttackMode));
            lastAttackMode = isAttackMode;
        }

        UpdateIcons(isAttackMode);
    }

    void UpdateIcons(bool isAttack)
    {
        if (isAttack)
        {
            attackIcon.color = activeColor;
            movementIcon.color = inactiveColor;
        }
        else
        {
            movementIcon.color = activeColor;
            attackIcon.color = inactiveColor;
        }
    }

    IEnumerator PlaySwitchEffect(bool isAttack)
    {
        Image main = isAttack ? attackIcon : movementIcon;
        Image other = isAttack ? movementIcon : attackIcon;

        Vector3 mainStart = main.transform.localScale;
        Vector3 otherStart = other.transform.localScale;

        float t = 0;

        while (t < 1)
        {
            t += Time.deltaTime * animSpeed;

            float pop = Mathf.Sin(t * Mathf.PI);

            main.transform.localScale = Vector3.Lerp(mainStart, Vector3.one * popScale, pop);
            other.transform.localScale = Vector3.Lerp(otherStart, Vector3.one * 0.9f, pop);

            Vector3 shake = Random.insideUnitCircle * shakeAmount;
            main.transform.localPosition = shake;
            other.transform.localPosition = -shake;

            yield return null;
        }

        main.transform.localScale = Vector3.one * 1.2f;
        other.transform.localScale = Vector3.one;

        main.transform.localPosition = Vector3.zero;
        other.transform.localPosition = Vector3.zero;
    }
}
