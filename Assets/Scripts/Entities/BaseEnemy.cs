using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseEnemy : MonoBehaviour, IDamageable
{
    private Rigidbody rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    public void TakeDamage(float damage, IDamageable owner)
    {
        
    }

    public void TakeKnockback(float knockbackPower, Vector3 knockbackDirection)
    {
        rb.AddForce(knockbackDirection * knockbackPower, ForceMode.Impulse);
    }
}
