using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile_Controller : MonoBehaviour
{
    [Tooltip("Set to true if the damage and knockback should come from the player instead of from the projectile stats asset. Usually true for the player projectiles only.")]
    [SerializeField] private bool isScaleWithPlayerStats = false;

    [SerializeField] private LayerMask affectableLayers;
    [SerializeField] private ProjectileStats stats;
    [SerializeField] private GameObject mainParticle;
    [SerializeField] private GameObject hitParticle;
    [SerializeField] private GameObject arrowVisual;
    [SerializeField] private float hitParticleDuration = 0.65f;
    [SerializeField] private float projectileLifetime = 2f;
    [SerializeField] private GameObject muzzleParticle;
    [SerializeField] private bool is_Homing;
    [SerializeField] private bool is_AoE_Projectile;
    private GameObject target;
    private IDamageable ownerOfProjectile;
    private Transform thisTransform, targetTransform;
    private Vector3 dir;

    //upon hitting a target set to true and disable the rotation of the projectile else it looks strangely
    //when called again from the pool set to false
    private bool isTargetHit = false;

    private void Start()
    {
        thisTransform = GetComponent<Transform>();
      //  rb.AddForce(transform.forward * stats.travelSpeed, ForceMode.Impulse);
        StartCoroutine(DeactivateObjectAfter(projectileLifetime));

        if (muzzleParticle != null)
        {
            StartCoroutine(ActivateAndStopMuzzleParticle());
        }
        mainParticle.SetActive(true);

    }

    private void OnTriggerEnter(Collider other)
    {
        if (affectableLayers == (affectableLayers | (1 << other.gameObject.layer)))
        {
            IDamageable tempInterface = other.gameObject.GetComponent<IDamageable>();
            if (tempInterface != null && !isTargetHit)
            {
                if(!isScaleWithPlayerStats)
                {
                    tempInterface.TakeDamage(stats.damage, ownerOfProjectile);
                    tempInterface.TakeKnockback(stats.knockbackPower, thisTransform.forward);
                }
                else
                {
                    tempInterface.TakeDamage(PlayerController.Instance.AttackDamage, ownerOfProjectile);
                    tempInterface.TakeKnockback(PlayerController.Instance.KnockbackPower, thisTransform.forward);
                }
            }
            TriggerOnHitFeedback();
        }
    }

    private void Update()
    {
        if (isTargetHit)
            return;
        thisTransform.Translate(thisTransform.forward * stats.travelSpeed * Time.deltaTime, Space.World);
        //add ChooseTarget() -> raycast sphere and home in on a close enemy
        if (is_Homing && target != null)
        {
            //  thisTransform.position = Vector3.Lerp(thisTransform.position, target.transform.position + new Vector3(0f, 1f, 0f), stats.travelSpeed * Time.deltaTime);
            dir = (targetTransform.position - thisTransform.position) + new Vector3(0f, 1f, 0f);
            if (!isTargetHit)
                thisTransform.rotation = Quaternion.LookRotation(dir);
            thisTransform.Translate(dir.normalized * stats.travelSpeed * Time.deltaTime, Space.World);
        }
    }

    private void OnBecameInvisible()
    {
        this.gameObject.SetActive(false);
    }

    public void SetTarget(GameObject target, IDamageable owner)
    {
        ownerOfProjectile = owner;
        isTargetHit = false;
        if (muzzleParticle != null)
        {
            StartCoroutine(ActivateAndStopMuzzleParticle());
        }
        mainParticle.SetActive(true);
        this.target = target;
        targetTransform = target.transform;
    }

    private void TriggerOnHitFeedback()
    {
        isTargetHit = true;
        if (hitParticle == null || mainParticle == null)
            return;
        hitParticle.SetActive(true);
        mainParticle.SetActive(false);
     //   rb.velocity = Vector3.zero;
        StartCoroutine(DeactivateObjectAfter(hitParticleDuration));
    }

    private IEnumerator DeactivateObjectAfter(float duration)
    {
        arrowVisual.SetActive(false);
        yield return new WaitForSeconds(duration);
        hitParticle.SetActive(false);
        gameObject.SetActive(false);
    }

    private IEnumerator ActivateAndStopMuzzleParticle()
    {
        muzzleParticle.SetActive(true);
        yield return new WaitForSeconds(0.25f);
        muzzleParticle.SetActive(false);
    }
}
