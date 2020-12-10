using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ProjectileStats", menuName = "Chochosan/StatsUpgrade/StatsUpgradeAsset", order = 1)]
public class StatsUpgrade : ScriptableObject
{
    public float attackDamageFlatBonus, attackDamagePercentBonus;
    public float healthFlatBonus, healthPercentBonus;
    public float attackSpeedFlatBonus, attackSpeedPercentBonus;
}
