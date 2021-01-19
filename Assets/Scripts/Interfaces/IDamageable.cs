using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IDamageable 
{
    void TakeDamage(float damage, IDamageable ownerofDamage);
    void TakeKnockback(float knockbackPower, Vector3 knockbackDirection);
    void HealSelf(float healValue);

    void Freeze(float duration, float chance);
    void GetSlowed(float duration, float slowPower, float chance);
    void GetBurned(int totalTicks, float tickCooldown, float damagePerTick);
}
