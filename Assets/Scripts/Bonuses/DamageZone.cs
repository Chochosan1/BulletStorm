using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageZone : MonoBehaviour
{
    enum DamageType { OnTriggerEnter, OnTriggerStay };


    [Header("Properties")]
    [SerializeField] private LayerMask acceptEntitiesFromLayers;
    [SerializeField] private DamageType damageType = DamageType.OnTriggerEnter;
    [SerializeField] private float damageCooldown = 0.5f;
    [SerializeField] private float damagePerTick = 5f;
    private float damageTimestamp;

    private void OnTriggerEnter(Collider other)
    {
        if (damageType == DamageType.OnTriggerEnter)
        {
            if (acceptEntitiesFromLayers != (acceptEntitiesFromLayers | (1 << other.gameObject.layer)))
                return;

            other.gameObject.GetComponent<IDamageable>()?.TakeDamage(damagePerTick, null);
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (damageType == DamageType.OnTriggerStay)
        {
            if (Time.time < damageTimestamp || acceptEntitiesFromLayers != (acceptEntitiesFromLayers | (1 << other.gameObject.layer)))
                return;

            damageTimestamp = Time.time + damageCooldown;
            other.gameObject.GetComponent<IDamageable>()?.TakeDamage(damagePerTick, null);
        }
    }
}
