using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ProjectileStats", menuName = "Chochosan/Entities/StatsAsset", order = 1)]
public class StatsEntity : ScriptableObject
{
    public float maxHealth;
    public float damage;
    public float knockbackPower;
}
