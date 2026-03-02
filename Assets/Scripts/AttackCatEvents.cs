using UnityEngine;

public class AttackCatEvents : MonoBehaviour
{
    PlayerController player;

    void Start()
    {
        player = GetComponentInParent<PlayerController>();
    }

    public void ShootProjectile()
    {
        player.ShootProjectile();
    }

    // Called by Melee_Attack animation event at hit frame
    public void DealMeleeDamage()
    {
        player.DealMeleeDamage();
    }

    // Called by Melee_Attack2 animation event at hit frame
    public void DealMeleeCombo()
    {
        player.DealMeleeComboDamage();
    }
}
