using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IDamageable 
{
    void TakeDamage(float damage, IDamageable owner);
    void TakeKnockback(float knockbackPower, Vector3 knockbackDirection);
    void HealSelf(float healValue);
}
