/// <summary>
/// Interface for any object that can receive damage.
/// Provides consistent damage handling across player, enemies, and destructibles.
/// </summary>
public interface IDamageable
{
    /// <summary>
    /// Apply damage to this object.
    /// </summary>
    /// <param name="damage">Amount of damage to apply.</param>
    void TakeDamage(int damage);
    
    /// <summary>
    /// Check if this object is still alive.
    /// </summary>
    bool IsAlive { get; }
}
