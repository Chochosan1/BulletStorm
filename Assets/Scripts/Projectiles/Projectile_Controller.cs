using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile_Controller : MonoBehaviour
{
   [SerializeField] private LayerMask affectableLayers;
    [SerializeField] private ProjectileStats stats;
    [SerializeField] private GameObject mainParticle;
    [SerializeField] private GameObject hitParticle;
    [SerializeField] private Rigidbody rb;
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
        rb.AddForce(transform.forward * stats.travelSpeed, ForceMode.Impulse);
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
            if (tempInterface != null)
            {
                tempInterface.TakeDamage(stats.damage, ownerOfProjectile);
                tempInterface.TakeKnockback(stats.knockbackPower, thisTransform.forward);
            }
            TriggerOnHitFeedback();
        }
    }

    private void Update()
    { 
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
        rb.velocity = Vector3.zero;
        StartCoroutine(DeactivateObjectAfter(hitParticleDuration));
    }

    private IEnumerator DeactivateObjectAfter(float duration)
    {
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
