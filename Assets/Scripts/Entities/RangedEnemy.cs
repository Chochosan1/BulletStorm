using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
[RequireComponent(typeof(NavMeshAgent))]
public sealed class RangedEnemy : BaseEnemy
{
    enum RangedSpecialType { None, MultipleFireballAlways, MultipleFireballSometimes }

    enum StatesAI { Idle, MovingToTarget, Attack };
    private StatesAI currentStateAI;

    private NavMeshAgent agent;
    private IDamageable currentTargetDamageable;

    [Header("Special")]
    [SerializeField] private RangedSpecialType specialTypeUnit = RangedSpecialType.None;
    [SerializeField] private Transform[] projectileSpawnPositions;

    [Header("References")]
    [SerializeField] private UnityEngine.UI.Slider healthBar;

    [Header("Shoot")]
    [Space]
    [SerializeField] private Transform projectileSpawnPosition;
    [SerializeField] private GameObject projectile;
    [SerializeField] private float shootRate = 2f;
    [SerializeField] private float stoppingDistance = 12f;
    private float shootTimestamp;
    private float shootCooldown;

    [Header("Explode")]
    [SerializeField] private float radius = 5f;
    private float power;

    int shotsFired;

    //once in the attack state finish the attack then be allowed to leave the state (to avoid endless kiting)
    private bool canExitAttackState = true;
    private Animator anim;

    private float ShootRate
    {
        get => shootRate;
        set
        {
            shootRate = value;
            shootCooldown = 1 / shootRate;
        }
    }

    void Start()
    {
        base.Start();
        ShootRate = shootRate;
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();
        agent.stoppingDistance = stoppingDistance;
      
        healthBar.maxValue = stats.maxHealth;
        healthBar.value = CurrentHealth;
    }

    void Update()
    {
        if (currentStateAI == StatesAI.MovingToTarget)
        {
            SetAgentDestination(agent, currentTarget.transform.position);
        }
        else if(currentStateAI == StatesAI.Attack)
        {
            canExitAttackState = false;
            LookAtTarget();

            if(specialTypeUnit == RangedSpecialType.None)
            {
                Shoot();         
            }
            else if(specialTypeUnit == RangedSpecialType.MultipleFireballAlways)
            {
                ShootMultipleFireball();
            }    
            else if(specialTypeUnit == RangedSpecialType.MultipleFireballSometimes)
            {
                if (Time.time >= shootTimestamp)
                {
                    if (shotsFired >= 2)
                    {
                        ShootMultipleFireball();
                        shotsFired = 0;
                    }
                    else
                    {
                        Shoot();
                        shotsFired++;
                    }
                }
            }
        }

        ChooseNewTarget(true);

        if (currentTarget != null)
        {
            if ((currentTarget.transform.position - thisTransform.position).magnitude <= agent.stoppingDistance + 0.01f)
            {
                GoToAttackState();
            }
            else
            {
                if (!canExitAttackState)
                    return;
                GoToMovingToTarget();
            }
        }
        else
        {
            if (!canExitAttackState)
                return;
            GoToIdleState();
        }
    }

    private void GoToIdleState()
    {
        anim.SetBool("isRun", false);
        anim.SetBool("isAttack", false);
        anim.SetBool("isIdle", true);
        currentStateAI = StatesAI.Idle;
    }

    private void GoToAttackState()
    {
        currentTargetDamageable = currentTarget.GetComponent<IDamageable>();
        currentStateAI = StatesAI.Attack;
        anim.SetBool("isRun", false);
        anim.SetBool("isAttack", true);
    }

    private void GoToMovingToTarget()
    {
        currentStateAI = StatesAI.MovingToTarget;
        anim.SetBool("isRun", true);
        anim.SetBool("isAttack", false);
    }

    public override void TakeDamage(float damage, IDamageable owner)
    {
        CurrentHealth -= damage;
        healthBar.value = CurrentHealth;

        if (CurrentHealth <= 0)
        {
            Explode();
            this.gameObject.SetActive(false);
        }
    }

    private void Shoot()
    {
       // if (Time.time >= shootTimestamp)
     //   {
            shootTimestamp = Time.time + shootCooldown;
            GameObject projectileCopy = Instantiate(projectile, projectileSpawnPosition.position, projectile.transform.rotation);
            projectileCopy.transform.forward = projectileSpawnPosition.transform.forward;
            canExitAttackState = true;
      //  }
    }

    private void ShootMultipleFireball()
    {
      //  if (Time.time >= shootTimestamp)
     //   {
            shootTimestamp = Time.time + shootCooldown;
            foreach(Transform psp in projectileSpawnPositions)
            {
                GameObject projectileCopy = Instantiate(projectile, psp.position, projectile.transform.rotation);
                projectileCopy.transform.forward = psp.transform.forward;
            }
            
           
            canExitAttackState = true;
         //   GoToIdleState();
     //   }
    }

    public void Explode()
    {
        Vector3 explosionPos = thisTransform.position;
        Collider[] colliders = Physics.OverlapSphere(explosionPos, radius);
        foreach (Collider hit in colliders)
        {
            Rigidbody rb = hit.GetComponent<Rigidbody>();

            if (rb != null)
            {
                power = Random.Range(stats.knockbackPower, stats.knockbackPower * 3f);
                rb.AddExplosionForce(power, explosionPos, radius, 1.0F);
            }
        }
    }
}
