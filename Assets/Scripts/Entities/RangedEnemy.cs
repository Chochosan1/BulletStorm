using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
[RequireComponent(typeof(NavMeshAgent))]
public sealed class RangedEnemy : BaseEnemy
{
    enum StatesAI { Idle, MovingToTarget, Attack };
    private StatesAI currentStateAI;

    private NavMeshAgent agent;
    private IDamageable currentTargetDamageable;

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
        ShootRate = shootRate;
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();
        agent.stoppingDistance = stoppingDistance;
        base.Start();
        Debug.Log(CurrentHealth);
    }

    void Update()
    {
        if (currentStateAI == StatesAI.MovingToTarget)
        {
            SetAgentDestination(agent, currentTarget.transform.position);
        }
        else if(currentStateAI == StatesAI.Attack)
        {
            Shoot();
            LookAtTarget();
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
                GoToMovingToTarget();
            }
        }
        else
        {
            currentStateAI = StatesAI.Idle;
        }
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

        if (CurrentHealth <= 0)
        {
            Explode();
            this.gameObject.SetActive(false);
        }
    }

    private void Shoot()
    {
        if (Time.time >= shootTimestamp)
        {
            shootTimestamp = Time.time + shootCooldown;
            GameObject projectileCopy = Instantiate(projectile, projectileSpawnPosition.position, projectile.transform.rotation);
            projectileCopy.transform.forward = projectileSpawnPosition.transform.forward;
        }
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
