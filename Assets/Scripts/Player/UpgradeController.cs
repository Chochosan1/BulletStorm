﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// Controls what upgrades are available to the player.
/// </summary>
[RequireComponent(typeof(PlayerController))]
public class UpgradeController : MonoBehaviour
{
    [Header("Upgrade sequence")]
    public GameObject upgradeParticle;
    public GameObject upgradeParticle2;

    [Header("Bonuses")]
    public GameObject rotatingAroundPlayerProjectile;
    public GameObject tornadoPrefab;
    public GameObject freezeZonePrefab;
    public float oneShotChance = 0.03f;
    public float tornadoChanceToSpawnOnDeath = 0.1f;
    public float freezeZoneChanceToSpawnOnDeath = 0.1f;
    public float explodeOnDeathChance = 0.35f;

    [Header("Triple projectile")]
    public Transform tripleProjectileSpawnLeft;
    public Transform tripleProjectileSpawnRight;

    [Header("Backwards projectile")]
    public Transform projectileBackwardsSpawn;

    [Header("Sideways projectiles")]
    public Transform sidewaysProjectileLeft;
    public Transform sidewaysProjectileRight;

    public static UpgradeController Instance;

    public enum UpgradeType { TripleProjectile, ProjectileBackwards, ProjectilesSideways, RotatingProjectile, ProjectileHomingOnCloseEnemies, ProjectileAOE, SlowingProjectile, FreezingProjectile, DashDistanceIncreased, DashCooldownReduced, MaxHealthIncrease, AttackSpeedIncrease, AttackDamageIncrease, OneShotChance, TornadoChanceOnDeath, FreezeZoneOnDeath, ProjectileSpeed, ExplodeOnDeath }
    private UpgradeType currentUpgrade;

    private Dictionary<UpgradeType, bool> upgradeStatusMap;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }

    private void Start()
    {
        upgradeStatusMap = new Dictionary<UpgradeType, bool>();
    }

    ///<summary>Checks if the said enum exists in the dictionary with upgrades.</summary>
    public bool IsUpgradeUnlocked(UpgradeType upgradeType)
    {
        return upgradeStatusMap.ContainsKey(upgradeType);
    }

    ///<summary>Checks if the said string (after it gets parsed to an enum) exists in the dictionary with upgrades.</summary>
    public bool IsUpgradeUnlocked(string upgradeString)
    {
        return IsUpgradeUnlocked((UpgradeType)Enum.Parse(typeof(UpgradeType), upgradeString));
    }

    ///<summary>Adds the said enum to a dictionary. One upgrade can be added only once. Fires the OnUpgradeLearned event.</summary>
    public void UnlockUpgrade(UpgradeType upgradeType)
    {
        if (IsUpgradeUnlocked(upgradeType))
            return;

        upgradeStatusMap.Add(upgradeType, true);
        Chochosan.CustomEventManager.OnUpgradeLearned?.Invoke(upgradeType);

        Chochosan.ChochosanHelper.ChochosanDebug("Just unlocked " + upgradeType.ToString(), "red");
    }

    ///<summary>Used by the upgrade buttons. Parses the string to an enum and then sends it to the UnlockUpgrade() method.</summary>
    public void ButtonUpgradeChoice(string upgradeString)
    {
        UnlockUpgrade((UpgradeType)Enum.Parse(typeof(UpgradeType), upgradeString));
    }
}
