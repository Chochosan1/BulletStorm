﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile_Controller : MonoBehaviour
{
    [Header("Projectile scaling")]
    [Tooltip("Set to true if the damage, knockback and upgrades should come from the player instead of from the projectile stats asset. Usually true for the player projectiles only.")]
    [SerializeField] private bool isScaleWithPlayerStats = false;

    [Header("General")]
    [SerializeField] private LayerMask affectableLayers;
    [SerializeField] private ProjectileStats stats;
    [SerializeField] private ProjectileSounds soundsAsset;
    [SerializeField] private GameObject mainParticleDefault;
    [SerializeField] private GameObject hitParticleDefault;
    [SerializeField] private GameObject arrowVisual;
    [SerializeField] private float hitParticleDuration = 0.65f;
    [SerializeField] private float projectileLifetime = 2f;
    [SerializeField] private GameObject muzzleParticleDefault;


    [Header("Upgrades")]
    [SerializeField] private float homingRangeDetection = 2f;
    [Tooltip("The layers which will be used by the projectile to home onto. Different from the affectable layers as the projectile should not always home onto things it will deal damage to (e.g breakable objects)")]
    [SerializeField] private LayerMask homingDetectionMask;
    [SerializeField] private float radius_AoE = 2f;
    [SerializeField] private GameObject mainParticleHoming;
    [SerializeField] private GameObject hitParticleHoming;
    [SerializeField] private GameObject muzzleParticleHoming;
    [SerializeField] private GameObject mainParticleAoE;
    [SerializeField] private GameObject hitParticleAoE;
    [SerializeField] private GameObject muzzleParticleAoE;

    [Header("Non-scale with player properties")]
    [Tooltip("Property to make the projectile have a chance to freeze on hit. Use only if the projectile does not scale with player stats.")]
    [SerializeField] private bool is_FreezingProjectile;
    [Tooltip("Property to make the projectile have a chance to slow on hit. Use only if the projectile does not scale with player stats.")]
    [SerializeField] private bool is_SlowingProjectile;
    private bool is_AoE_Projectile;
    private bool is_HomingOnCloseEnemy;


    private float chooseNewTargetEveryXSeconds = 0.1f;
    private float chooseNewTargetTimestamp;
    private float flySpeed;

    private GameObject mainParticleToUse, hitParticleToUse, muzzleParticleToUse;

    private GameObject target;
    private IDamageable ownerOfProjectile;
    private Transform thisTransform, targetTransform;
    private Vector3 dir;
    private AudioSource audioSource;

    //upon hitting a target set to true and disable the rotation of the projectile else it looks strangely
    //when called again from the pool set to false
    private bool isTargetHit = false;
    private bool isLockedOntoTarget = false;

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
        thisTransform = transform;
        //  rb.AddForce(transform.forward * stats.travelSpeed, ForceMode.Impulse);
        StartCoroutine(DeactivateObjectAfter(projectileLifetime));

        mainParticleToUse?.SetActive(false);
        hitParticleToUse?.SetActive(false);
        muzzleParticleToUse?.SetActive(false);
        //mainParticleDefault.SetActive(false);
        //hitParticleDefault.SetActive(false);
        //muzzleParticleDefault.SetActive(false);

        //mainParticleAoE.SetActive(false);
        //hitParticleAoE.SetActive(false);
        //muzzleParticleAoE.SetActive(false);

        //mainParticleHoming.SetActive(false);
        //hitParticleHoming.SetActive(false);
        //muzzleParticleHoming.SetActive(false);

        mainParticleToUse = mainParticleDefault;
        hitParticleToUse = hitParticleDefault;
        muzzleParticleToUse = muzzleParticleDefault;

        flySpeed = stats.travelSpeed;

        if (isScaleWithPlayerStats)
        {
            if (UpgradeController.Instance.IsUpgradeUnlocked(UpgradeController.UpgradeType.ProjectileHomingOnCloseEnemies))
            {
                is_HomingOnCloseEnemy = true;
                mainParticleToUse = mainParticleHoming;
                hitParticleToUse = hitParticleHoming;
                muzzleParticleToUse = muzzleParticleHoming;
            }


            if (UpgradeController.Instance.IsUpgradeUnlocked(UpgradeController.UpgradeType.ProjectileAOE))
            {
                is_AoE_Projectile = true;
                //mainParticleToUse = mainParticleAoE;
                hitParticleToUse = hitParticleAoE;
                //muzzleParticleToUse = muzzleParticleAoE;
            }

            if (UpgradeController.Instance.IsUpgradeUnlocked(UpgradeController.UpgradeType.FreezingProjectile))
            {
                is_FreezingProjectile = true;
            }

            if (UpgradeController.Instance.IsUpgradeUnlocked(UpgradeController.UpgradeType.SlowingProjectile))
            {
                is_SlowingProjectile = true;
            }

            if (UpgradeController.Instance.IsUpgradeUnlocked(UpgradeController.UpgradeType.ProjectileSpeed))
            {
                flySpeed *= 1.5f;
            }
        }

        if (muzzleParticleDefault != null)
        {
            StartCoroutine(ActivateAndStopMuzzleParticle());
        }
        mainParticleToUse.SetActive(true);

        audioSource.PlayOneShot(soundsAsset.shootProjectileSound);
    }

    public void ResetProjectileFromPool(Transform posToResetAt)
    {
        if (thisTransform == null)
            thisTransform = transform;

        target = null;

        mainParticleToUse?.SetActive(false);
        hitParticleToUse?.SetActive(false);
        muzzleParticleToUse?.SetActive(false);

        thisTransform.position = posToResetAt.position;
        thisTransform.forward = posToResetAt.forward;

        StartCoroutine(DeactivateObjectAfter(projectileLifetime));

        mainParticleToUse = mainParticleDefault;
        hitParticleToUse = hitParticleDefault;
        muzzleParticleToUse = muzzleParticleDefault;

        flySpeed = stats.travelSpeed;

        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();

        audioSource.PlayOneShot(soundsAsset.shootProjectileSound);

        if (isScaleWithPlayerStats)
        {
            if (UpgradeController.Instance.IsUpgradeUnlocked(UpgradeController.UpgradeType.ProjectileHomingOnCloseEnemies))
            {
                is_HomingOnCloseEnemy = true;
                mainParticleToUse = mainParticleHoming;
                hitParticleToUse = hitParticleHoming;
                muzzleParticleToUse = muzzleParticleHoming;
            }


            if (UpgradeController.Instance.IsUpgradeUnlocked(UpgradeController.UpgradeType.ProjectileAOE))
            {
                is_AoE_Projectile = true;
                //mainParticleToUse = mainParticleAoE;
                hitParticleToUse = hitParticleAoE;
                // muzzleParticleToUse = muzzleParticleAoE;
            }

            if (UpgradeController.Instance.IsUpgradeUnlocked(UpgradeController.UpgradeType.FreezingProjectile))
            {
                is_FreezingProjectile = true;
            }

            if (UpgradeController.Instance.IsUpgradeUnlocked(UpgradeController.UpgradeType.SlowingProjectile))
            {
                is_SlowingProjectile = true;
            }

            if (UpgradeController.Instance.IsUpgradeUnlocked(UpgradeController.UpgradeType.ProjectileSpeed))
            {
                flySpeed *= 1.5f;
            }
        }

        if (muzzleParticleDefault != null)
        {
            StartCoroutine(ActivateAndStopMuzzleParticle());
        }
        mainParticleToUse.SetActive(true);
        isTargetHit = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (affectableLayers == (affectableLayers | (1 << other.gameObject.layer)))
        {
            IDamageable tempInterface = other.gameObject.GetComponent<IDamageable>();
            if (tempInterface != null && !isTargetHit)
            {
                if (is_FreezingProjectile)
                {
                    tempInterface.Freeze(stats.freezeDuration, stats.chanceToFreeze);
                }

                if (is_SlowingProjectile)
                {
                    tempInterface.GetSlowed(stats.slowDuration, stats.slowMultiplier, stats.chanceToSlow);
                }

                if (!isScaleWithPlayerStats)
                {
                    tempInterface.TakeDamage(stats.damage, ownerOfProjectile);
                    tempInterface.TakeKnockback(stats.knockbackPower, thisTransform.forward);
                }
                else
                {

                    if (is_AoE_Projectile)
                    {
                        Collider[] firstHitColliders = Physics.OverlapSphere(transform.position, homingRangeDetection, affectableLayers);
                        foreach (Collider coll in firstHitColliders)
                        {
                            coll.gameObject.GetComponent<IDamageable>()?.TakeDamage(PlayerController.Instance.AttackDamage, ownerOfProjectile);
                        }
                    }
                    else
                    {
                        tempInterface.TakeDamage(PlayerController.Instance.AttackDamage, ownerOfProjectile);
                        tempInterface.TakeKnockback(PlayerController.Instance.KnockbackPower, thisTransform.forward);
                    }
                }
            }
            TriggerOnHitFeedback();
        }
    }

    private void Update()
    {
        if (isTargetHit)
            return;

        if (is_HomingOnCloseEnemy && target != null)
        {
            //  thisTransform.position = Vector3.Lerp(thisTransform.position, target.transform.position + new Vector3(0f, 1f, 0f), stats.travelSpeed * Time.deltaTime);
            dir = (targetTransform.position - thisTransform.position) + new Vector3(0f, 1f, 0f);
            if (!isTargetHit)
                thisTransform.rotation = Quaternion.Slerp(thisTransform.rotation, Quaternion.LookRotation(dir), 10f * Time.deltaTime);
            // thisTransform.rotation = Quaternion.LookRotation(dir);

            thisTransform.Translate(dir.normalized * flySpeed * Time.deltaTime, Space.World);
        }
        else
        {
            thisTransform.Translate(thisTransform.forward * flySpeed * Time.deltaTime, Space.World);

            if (is_HomingOnCloseEnemy && Time.time >= chooseNewTargetTimestamp)
            {
                ChooseNewTarget();
                chooseNewTargetTimestamp = Time.time + chooseNewTargetEveryXSeconds;
            }
        }
    }

    private void OnBecameInvisible()
    {
        if (isScaleWithPlayerStats)
            this.gameObject.SetActive(false);
        else
            Destroy(this.gameObject);
    }

    protected virtual void ChooseNewTarget()
    {
        if (target == null)
        {
            Collider[] firstHitColliders = Physics.OverlapSphere(transform.position, homingRangeDetection, homingDetectionMask);
            if (firstHitColliders.Length > 0)
            {
                target = firstHitColliders[0].gameObject;
                targetTransform = target.transform;
                //  Chochosan.ChochosanHelper.ChochosanDebug("TARGET ACQUIRED", "red");
            }
        }
    }

    public void SetTarget(Transform target, IDamageable owner)
    {
        ownerOfProjectile = owner;
        isTargetHit = false;
        //if (muzzleParticleDefault != null)
        //{
        //    StartCoroutine(ActivateAndStopMuzzleParticle());
        //}
        //mainParticleToUse.SetActive(true);
        this.target = target.gameObject;
        targetTransform = target;
    }

    private void TriggerOnHitFeedback()
    {
        if (isTargetHit)
            return;

        isTargetHit = true;
        if (hitParticleToUse == null || muzzleParticleToUse == null)
            return;
        hitParticleToUse.SetActive(true);
        mainParticleToUse.SetActive(false);

        audioSource.PlayOneShot(soundsAsset.hitProjectileSound);
        //   rb.velocity = Vector3.zero;
        StartCoroutine(DeactivateObjectAfter(hitParticleDuration));
    }

    private IEnumerator DeactivateObjectAfter(float duration)
    {
        arrowVisual.SetActive(false);
        yield return new WaitForSeconds(duration);
        hitParticleToUse.SetActive(false);

        if (isScaleWithPlayerStats)
            gameObject.SetActive(false);
        else
            Destroy(this.gameObject);
    }

    private IEnumerator ActivateAndStopMuzzleParticle()
    {
        muzzleParticleToUse.SetActive(true);
        yield return new WaitForSeconds(0.25f);
        muzzleParticleToUse.SetActive(false);
    }
}
