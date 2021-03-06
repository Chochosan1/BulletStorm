﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(Rigidbody))]
public class BaseEnemy : MonoBehaviour, IDamageable
{
    [Header("Base AI")]
    [SerializeField] protected string unitName;
    [SerializeField] protected float FOLLOW_PLAYER_STOPPING_DISTANCE = 6.5f;
    [Tooltip("The layer which can be detected by the AI unit. All other layers will be ignored.")]
    [SerializeField] protected LayerMask enemyLayer;
    [Tooltip("How far away can the AI unit detect its target without taking into consideration walls and such. Will be used before looking for target within the greater range. ")]
    [SerializeField] protected float firstPureEnemySenseRange = 5f;
    [Tooltip("How far away can the AI unit detect its target without taking into consideration walls and such. Will be used after first looking for targets within the smaller range. ")]
    [SerializeField] protected float secondPureEnemySenseRange = 30f;
    [SerializeField] protected StatsEntity stats;
    [Tooltip("Set to true if the object should suffer knockback.")]
    [SerializeField] protected bool canBeKnockedBack = true;
    [Tooltip("Should the death particle get destroyed after some time or just remain there after being spawned?")]
    [SerializeField] protected bool isDestroyDeathParticleAfterTime = true;
    [SerializeField] protected GameObject deathParticle;
    [SerializeField] protected GameObject frozenParticle;
    [SerializeField] protected GameObject explodedDeathParticle;
    [SerializeField] protected GameObject firstSpawnedParticle;
    protected GameObject usedDeathParticle;
    [SerializeField] protected LootContainer lootTable;
    [HideInInspector]
    public GameObject currentTarget;
    protected Rigidbody rb;
    protected Transform thisTransform;
    private bool lootDropped = false;
    protected bool isBoss = false;

    protected bool isBurning = false;
    protected int totalTicksBurned;
    private float currentHealth;
    public float CurrentHealth
    {
        get => currentHealth;
        set
        {
            currentHealth = value >= stats.maxHealth ? stats.maxHealth : value;
        }
    }

    protected void Start()
    {
        rb = GetComponent<Rigidbody>();
        thisTransform = transform;
        CurrentHealth = stats.maxHealth;

        if (firstSpawnedParticle != null)
        {
            firstSpawnedParticle.SetActive(true);
            StartCoroutine(DisableObjectAfter(firstSpawnedParticle, 2f));
        }
    }

    public virtual void TakeDamage(float damage, IDamageable owner)
    {
        CurrentHealth -= damage;

        if (CurrentHealth <= 0)
        {
            //  this.gameObject.SetActive(false);

            if (deathParticle != null)
            {
                GameObject deathParticleCopy = Instantiate(deathParticle, thisTransform.position, deathParticle.transform.rotation);

                if (isDestroyDeathParticleAfterTime)
                    Destroy(deathParticleCopy, 2f);
            }


            DetermineLoot();
            Destroy(this.gameObject);
        }
    }

    public virtual void TakeKnockback(float knockbackPower, Vector3 knockbackDirection)
    {
        rb.AddForce(knockbackDirection * knockbackPower, ForceMode.Impulse);
    }

    public virtual void HealSelf(float healValue)
    {
        CurrentHealth += healValue;
    }

    //Look towards the target
    protected virtual void LookAtTarget()
    {
        if (currentTarget != null)
        {
            Quaternion targetRotation = Quaternion.LookRotation(currentTarget.transform.position - transform.position);
            targetRotation.x = transform.rotation.x;
            targetRotation.z = transform.rotation.z;
            transform.rotation = targetRotation;
        }
    }

    //choose a random target out of all detected targets in a layer OR if the parameter is false choose the first target always
    protected virtual void ChooseNewTarget(bool chooseRandom)
    {
        if (currentTarget == null)
        {
            Collider[] firstHitColliders = Physics.OverlapSphere(transform.position, firstPureEnemySenseRange, enemyLayer);
            if (firstHitColliders.Length > 0)
            {
                if (chooseRandom)
                {
                    int randomIndex = Random.Range(0, firstHitColliders.Length);
                    currentTarget = firstHitColliders[randomIndex].gameObject;
                }
                else
                {
                    currentTarget = firstHitColliders[0].gameObject;
                }
                //  Chochosan.ChochosanHelper.ChochosanDebug("TARGET ACQUIRED", "red");
            }
            else
            {
                Collider[] secondHitColliders = Physics.OverlapSphere(transform.position, secondPureEnemySenseRange, enemyLayer);
                if (secondHitColliders.Length > 0)
                {
                    if (chooseRandom)
                    {
                        int randomIndex = Random.Range(0, secondHitColliders.Length);
                        currentTarget = secondHitColliders[randomIndex].gameObject;
                    }
                    else
                    {
                        currentTarget = secondHitColliders[0].gameObject;
                    }
                    //  Chochosan.ChochosanHelper.ChochosanDebug("TARGET ACQUIRED", "red");
                }
            }
        }
    }

    protected virtual void SetAgentDestination(NavMeshAgent agent, Vector3 targetPosition)
    {
        agent.destination = targetPosition;
    }

    protected virtual Collider[] GetAllInRange(Vector3 offset)
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position + offset, firstPureEnemySenseRange, enemyLayer);

        return hitColliders;
    }

    public virtual void Freeze(float duration, float chance)
    {
        //    throw new System.NotImplementedException();
    }

    public virtual void GetSlowed(float duration, float slowPower, float chance)
    {
        //     throw new System.NotImplementedException();
    }

    public virtual void GetBurned(int totalTicks, float tickCooldown, float damagePerTick)
    {
        if (isBurning)
            return;

        isBurning = true;
        totalTicksBurned = 0;
        StartCoroutine(BurnOverTime(totalTicks, tickCooldown, damagePerTick));
    }

    /// <summary>Determines what loot should drop.</summary>
    protected virtual void DetermineLoot()
    {
        if (lootDropped)
            return;

        lootDropped = true;
        GameObject lootDrop = lootTable?.DetermineLoot();

        if (lootDrop != null)
        {
            Instantiate(lootDrop, transform.position + new Vector3(0f, 1f, 0f), lootDrop.transform.rotation);
        }
    }

    public void SetEnemyAsBoss()
    {
        isBoss = true;
    }

    public Transform GetCurrentTargetTransform()
    {
        if (currentTarget == null)
            return thisTransform;

        return currentTarget.transform;
    }

    protected IEnumerator DisableObjectAfter(GameObject objectToDisable, float duration)
    {
        yield return new WaitForSeconds(duration);
        objectToDisable.SetActive(false);
    }

    protected IEnumerator BurnOverTime(float totalTicks, float tickCooldown, float damagePerTick)
    {
        //enable burn effect
        while(totalTicksBurned < totalTicks)
        {
            TakeDamage(damagePerTick, null);
            totalTicksBurned++;
            yield return new WaitForSeconds(tickCooldown);

            if (totalTicksBurned >= totalTicks)
                break;
        }
        isBurning = false;
        //disable burn effect
    }
}
