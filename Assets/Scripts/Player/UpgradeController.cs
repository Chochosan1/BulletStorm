using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PlayerController))]
public class UpgradeController : MonoBehaviour
{
    [Header("Upgrade sequence")]
    public GameObject upgradeParticle;
    public GameObject upgradeParticle2;

    [Header("Bonuses")]
    public GameObject rotatingAroundPlayerProjectile;

    [Header("Triple projectile")]
    public Transform tripleProjectileSpawnLeft;
    public Transform tripleProjectileSpawnRight;

    [Header("Backwards projectile")]
    public Transform projectileBackwardsSpawn;

    [Header("Sideways projectiles")]
    public Transform sidewaysProjectileLeft;
    public Transform sidewaysProjectileRight;

    public static UpgradeController Instance;

    public enum UpgradeType { TripleProjectile, ProjectileBackwards, ProjectilesSideways, RotatingProjectile, ProjectileHomingOnCloseEnemies, ProjectileAOE, SlowingProjectile, FreezingProjectile }
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

      //  UnlockUpgrade(UpgradeType.TripleProjectile);
      //  UnlockUpgrade(UpgradeType.ProjectilesSideways);
      ////  UnlockUpgrade(UpgradeType.ProjectileBackwards);
      //  UnlockUpgrade(UpgradeType.ProjectileHomingOnCloseEnemies);
      //  UnlockUpgrade(UpgradeType.ProjectileAOE);
      //  UnlockUpgrade(UpgradeType.FreezingProjectile);
      //  UnlockUpgrade(UpgradeType.SlowingProjectile);
    }


    public bool IsUpgradeUnlocked(UpgradeType upgradeType)
    {
        return upgradeStatusMap.ContainsKey(upgradeType);
    }

    public void UnlockUpgrade(UpgradeType upgradeType)
    {
        if (IsUpgradeUnlocked(upgradeType))
            return;

        upgradeStatusMap.Add(upgradeType, true);
        Chochosan.CustomEventManager.OnUpgradeLearned?.Invoke(upgradeType);

        Chochosan.ChochosanHelper.ChochosanDebug("Just unlocked " + upgradeType.ToString(), "red");
    }

    public void ButtonUpgradeChoice(string upgradeString)
    {
        UnlockUpgrade((UpgradeType)Enum.Parse(typeof(UpgradeType), upgradeString));
    }
}
