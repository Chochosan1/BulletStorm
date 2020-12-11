using System.Collections;
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
    private bool is_AoE_Projectile;
    private bool is_HomingOnCloseEnemy;
    private float chooseNewTargetEveryXSeconds = 0.1f;
    private float chooseNewTargetTimestamp;

    private GameObject mainParticleToUse, hitParticleToUse, muzzleParticleToUse;

    private GameObject target;
    private IDamageable ownerOfProjectile;
    private Transform thisTransform, targetTransform;
    private Vector3 dir;

    //upon hitting a target set to true and disable the rotation of the projectile else it looks strangely
    //when called again from the pool set to false
    private bool isTargetHit = false;

    private void Start()
    {
        thisTransform = transform;
        //  rb.AddForce(transform.forward * stats.travelSpeed, ForceMode.Impulse);
        StartCoroutine(DeactivateObjectAfter(projectileLifetime));

        mainParticleToUse = mainParticleDefault;
        hitParticleToUse = hitParticleDefault;
        muzzleParticleToUse = muzzleParticleDefault;

        if (isScaleWithPlayerStats)
        {
            if (UpgradeController.Instance.IsUpgradeUnlocked(UpgradeController.UpgradeType.ProjectileHomingOnCloseEnemies))
            {
                is_HomingOnCloseEnemy = true;
                //mainParticleToUse = mainParticleHoming;
                //hitParticleToUse = hitParticleHoming;
                //muzzleParticleToUse = muzzleParticleHoming;
            }


            if (UpgradeController.Instance.IsUpgradeUnlocked(UpgradeController.UpgradeType.ProjectileAOE))
            {
                is_AoE_Projectile = true;
                //mainParticleToUse = mainParticleAoE;
                //hitParticleToUse = hitParticleAoE;
                //muzzleParticleToUse = muzzleParticleAoE;
            }
        }

        if (muzzleParticleDefault != null)
        {
            StartCoroutine(ActivateAndStopMuzzleParticle());
        }
        mainParticleToUse.SetActive(true);
    }

    public void ResetProjectileFromPool(Transform posToResetAt)
    {
        if (thisTransform == null)
            thisTransform = transform;

        thisTransform.position = posToResetAt.position;
        thisTransform.forward = posToResetAt.forward;

        StartCoroutine(DeactivateObjectAfter(projectileLifetime));

        mainParticleToUse = mainParticleDefault;
        hitParticleToUse = hitParticleDefault;
        muzzleParticleToUse = muzzleParticleDefault;

        if (isScaleWithPlayerStats)
        {
            if (UpgradeController.Instance.IsUpgradeUnlocked(UpgradeController.UpgradeType.ProjectileHomingOnCloseEnemies))
            {
                is_HomingOnCloseEnemy = true;
                //mainParticleToUse = mainParticleHoming;
                //hitParticleToUse = hitParticleHoming;
                //muzzleParticleToUse = muzzleParticleHoming;
            }


            if (UpgradeController.Instance.IsUpgradeUnlocked(UpgradeController.UpgradeType.ProjectileAOE))
            {
                is_AoE_Projectile = true;
                //mainParticleToUse = mainParticleAoE;
                //hitParticleToUse = hitParticleAoE;
                //muzzleParticleToUse = muzzleParticleAoE;
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
                if (!isScaleWithPlayerStats)
                {
                    tempInterface.TakeDamage(stats.damage, ownerOfProjectile);
                    tempInterface.TakeKnockback(stats.knockbackPower, thisTransform.forward);
                }
                else
                {
                    if (!is_AoE_Projectile)
                    {
                        tempInterface.TakeDamage(PlayerController.Instance.AttackDamage, ownerOfProjectile);
                        tempInterface.TakeKnockback(PlayerController.Instance.KnockbackPower, thisTransform.forward);
                    }
                    else
                    {
                        Collider[] firstHitColliders = Physics.OverlapSphere(transform.position, homingRangeDetection, affectableLayers);
                        foreach (Collider coll in firstHitColliders)
                        {
                            coll.gameObject.GetComponent<IDamageable>().TakeDamage(PlayerController.Instance.AttackDamage, ownerOfProjectile);
                        }
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

            thisTransform.Translate(dir.normalized * stats.travelSpeed * Time.deltaTime, Space.World);
        }
        else
        {
            thisTransform.Translate(thisTransform.forward * stats.travelSpeed * Time.deltaTime, Space.World);

            if (is_HomingOnCloseEnemy && Time.time >= chooseNewTargetTimestamp)
            {
                ChooseNewTarget();
                chooseNewTargetTimestamp = Time.time + chooseNewTargetEveryXSeconds;
            }
               
        }
    }

    private void OnBecameInvisible()
    {
        this.gameObject.SetActive(false);
        // Destroy(this.gameObject);
    }

    //choose a random target out of all detected targets in a layer OR if the parameter is false choose the first target always
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

    public void SetTarget(GameObject target, IDamageable owner)
    {
        ownerOfProjectile = owner;
        isTargetHit = false;
        if (muzzleParticleDefault != null)
        {
            StartCoroutine(ActivateAndStopMuzzleParticle());
        }
        mainParticleToUse.SetActive(true);
        this.target = target;
        targetTransform = target.transform;
    }

    private void TriggerOnHitFeedback()
    {
        isTargetHit = true;
        if (hitParticleToUse == null || muzzleParticleToUse == null)
            return;
        hitParticleToUse.SetActive(true);
        mainParticleToUse.SetActive(false);
        //   rb.velocity = Vector3.zero;
        StartCoroutine(DeactivateObjectAfter(hitParticleDuration));
    }

    private IEnumerator DeactivateObjectAfter(float duration)
    {
        arrowVisual.SetActive(false);
        yield return new WaitForSeconds(duration);
        hitParticleToUse.SetActive(false);
        gameObject.SetActive(false);
        //    Destroy(this.gameObject);
    }

    private IEnumerator ActivateAndStopMuzzleParticle()
    {
        muzzleParticleToUse.SetActive(true);
        yield return new WaitForSeconds(0.25f);
        muzzleParticleToUse.SetActive(false);
    }
}
