using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealZone : MonoBehaviour
{
    public enum HealType { Once, Continuous };

    [Header("Properties")]
    [Tooltip("Once -> go through, heal, disappear.  Continuos -> stay in, get healed over time.")]
    [SerializeField] private HealType healType;
    [SerializeField] private LayerMask acceptEntitiesFromLayers;
    [SerializeField] private float healCooldown = 1f;
    [SerializeField] private float healPerTick = 5f;
    private float healTimestamp;

    private void OnTriggerStay(Collider other)
    {
        if(healType == HealType.Continuous)
        {
            if (Time.time < healTimestamp || acceptEntitiesFromLayers != (acceptEntitiesFromLayers | (1 << other.gameObject.layer)))
                return;

            healTimestamp = Time.time + healCooldown;
            other.gameObject.GetComponent<IDamageable>()?.HealSelf(healPerTick);
        }     
    }

    private void OnTriggerEnter(Collider other)
    {
        if(healType == HealType.Once)
        {
            other.gameObject.GetComponent<IDamageable>()?.HealSelf(healPerTick);
            Destroy(this.gameObject);
        }
    }
}
