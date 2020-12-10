using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StatsBoostPickUp : MonoBehaviour
{   
    [Header("Properties")]
    [SerializeField] private LayerMask acceptEntitiesFromLayers;
    [SerializeField] private StatsUpgrade statsUpgradeAsset;

    private void OnTriggerEnter(Collider other)
    {
        if (acceptEntitiesFromLayers != (acceptEntitiesFromLayers | (1 << other.gameObject.layer)))
            return;

        if (other.CompareTag("Player"))
            GrantBonusToThePlayer(other.GetComponent<PlayerController>());

        Destroy(gameObject);
    }

    private void GrantBonusToThePlayer(PlayerController pc)
    {
        pc.ShootRate += statsUpgradeAsset.attackSpeedFlatBonus;
        pc.ShootRate += pc.ShootRate * statsUpgradeAsset.attackSpeedPercentBonus;
    }
}
