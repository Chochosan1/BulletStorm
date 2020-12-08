using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ProjectileStats", menuName = "Chochosan/Projectiles/StatsAsset", order = 1)]
public class ProjectileStats : ScriptableObject
{
    public float travelSpeed;
    public float damage;
    public float knockbackPower;
}
