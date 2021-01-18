using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundProjectile : MonoBehaviour
{
    [Header("General")]
    [SerializeField] private LayerMask affectableLayers;
    [SerializeField] private ProjectileStats stats;
    [SerializeField] private ProjectileSounds soundsAsset;
    [SerializeField] private GameObject mainParticleDefault;
    [SerializeField] private GameObject hitParticleDefault;
    [SerializeField] private float hitParticleDuration = 0.65f;
    [SerializeField] private GameObject muzzleParticleDefault;
    [SerializeField] private GameObject markedGroundEffect;
    [SerializeField] private GameObject spawnOnHitObject;

    [Header("Properties")]
    [SerializeField] private bool isFreezingProjectile = false;

    [Header("Debug")]
    [SerializeField] private bool rangeGizmo;

    private float explodeRadius;

    private float flySpeed;
    private Transform thisTransform;
    private Vector3 dir;
    private Vector3 targetPos;
    private bool isTargetHit;
    private AudioSource audioSource;

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
        thisTransform = transform;
        flySpeed = stats.travelSpeed;

        mainParticleDefault.SetActive(true);

        if (muzzleParticleDefault != null)
        {
            StartCoroutine(ActivateAndStopMuzzleParticle());
        }

        explodeRadius = markedGroundEffect.transform.localScale.x;
    }

    void Update()
    {
        if (isTargetHit)
            return;

        dir = (targetPos - thisTransform.position) + new Vector3(0f, 1f, 0f);
        if (!isTargetHit)
            thisTransform.rotation = Quaternion.Slerp(thisTransform.rotation, Quaternion.LookRotation(dir), 10f * Time.deltaTime);

        thisTransform.Translate(dir.normalized * flySpeed * Time.deltaTime, Space.World);

        if(dir.magnitude <= 0.05f)
        {
            Explode();
        }
    }

    private void Explode()
    {
        isTargetHit = true;
        Vector3 explosionPos = thisTransform.position;
        Collider[] colliders = Physics.OverlapSphere(explosionPos, explodeRadius, affectableLayers);
        foreach (Collider hit in colliders)
        {
            hit.GetComponent<IDamageable>()?.TakeDamage(stats.damage, null);

            if (isFreezingProjectile)
                hit.GetComponent<IDamageable>()?.Freeze(2.5f, 1f);
        }

        TriggerOnHitFeedback();
    }

    public void SetTarget(Vector3 targetPos)
    {
        this.targetPos = targetPos;
        markedGroundEffect.SetActive(true);
        markedGroundEffect.transform.position = targetPos;
        markedGroundEffect.transform.SetParent(null);
    }

    private void TriggerOnHitFeedback()
    {
        isTargetHit = true;
        if (hitParticleDefault == null || muzzleParticleDefault == null)
            return;
        hitParticleDefault.SetActive(true);
        mainParticleDefault.SetActive(false);

        audioSource.PlayOneShot(soundsAsset.hitProjectileSound);
        Destroy(markedGroundEffect);
        //   rb.velocity = Vector3.zero;
        StartCoroutine(DeactivateObjectAfter(hitParticleDuration));
    }

    private IEnumerator DeactivateObjectAfter(float duration)
    {
        yield return new WaitForSeconds(duration);
        Instantiate(spawnOnHitObject, targetPos, spawnOnHitObject.transform.rotation);
        Destroy(this.gameObject);
    }

    private IEnumerator ActivateAndStopMuzzleParticle()
    {
        muzzleParticleDefault.SetActive(true);
        yield return new WaitForSeconds(0.25f);
        muzzleParticleDefault.SetActive(false);
    }

    private void OnDrawGizmos()
    {
        if (!rangeGizmo)
            return;  

        explodeRadius = markedGroundEffect.transform.localScale.x;
        Gizmos.DrawSphere(transform.position, explodeRadius);
    }
}
