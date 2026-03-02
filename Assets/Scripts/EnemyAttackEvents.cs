using UnityEngine;

public class EnemyAttackEvents : MonoBehaviour
{
    EnemyController enemy;

    void Start()
    {
        enemy = GetComponentInParent<EnemyController>();
    }

    public void DoDamage()
    {
        enemy.DealDamage();
    }
}
