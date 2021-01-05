using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ProjectileStats", menuName = "Chochosan/Projectiles/StatsAsset", order = 1)]
public class ProjectileStats : ScriptableObject
{
    [Header("General stats")]
    public float travelSpeed;
    public float damage;
    public float knockbackPower;

    [Header("Freeze")]
    [Range(0, 1)]
    public float chanceToFreeze;
    [Tooltip("Should match with the freeze particle duration for better immersion.")]
    public float freezeDuration = 2.5f;

    [Header("Slow")]
    [Range(0,1)]
    public float chanceToSlow;
    public float slowDuration;
    [Tooltip("The entity's movement speed will be multiplied by that. Lower number = harder slow.")]
    public float slowMultiplier;
}
