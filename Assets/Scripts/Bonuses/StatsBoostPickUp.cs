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
        int upgradeRolled = Random.Range(0, 6);
        switch(upgradeRolled)
        {
            case 0:
                pc.ShootRate += statsUpgradeAsset.attackSpeedFlatBonus;
                break;
            case 1:
                pc.ShootRate += pc.ShootRate * statsUpgradeAsset.attackSpeedPercentBonus;
                break;
            case 2:
                pc.MaxHealth += statsUpgradeAsset.healthFlatBonus;
                break;
            case 3:
                pc.MaxHealth += pc.MaxHealth * statsUpgradeAsset.healthPercentBonus;
                break;
            case 4:
                pc.AttackDamage += statsUpgradeAsset.attackDamageFlatBonus;
                break;
            case 5:
                pc.AttackDamage += pc.AttackDamage * statsUpgradeAsset.attackDamagePercentBonus;
                break;
            case 6:
                pc.KnockbackPower += statsUpgradeAsset.knockbackPowerFlat;
                break;
        }
    }
}
