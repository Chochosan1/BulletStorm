using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealZone : MonoBehaviour
{
    [Header("Properties")]
    [SerializeField] private LayerMask acceptEntitiesFromLayers;
    [SerializeField] private float healCooldown = 1f;
    [SerializeField] private float healPerTick = 5f;
    private float healTimestamp;

    private void OnTriggerStay(Collider other)
    {
        if (Time.time < healTimestamp || acceptEntitiesFromLayers != (acceptEntitiesFromLayers | (1 << other.gameObject.layer)))
            return;

        healTimestamp = Time.time + healCooldown;
        other.gameObject.GetComponent<IDamageable>()?.HealSelf(healPerTick);
    }
}
