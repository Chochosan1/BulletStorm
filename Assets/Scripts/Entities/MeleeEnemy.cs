using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
[RequireComponent(typeof(NavMeshAgent))]
public sealed class MeleeEnemy : BaseEnemy
{
    enum MeleeSpecialType { None, Kamikadze }

    enum StatesAI { Idle, MovingToTarget, Attack };
    private StatesAI currentStateAI;

    private NavMeshAgent agent;
    private IDamageable currentTargetDamageable;

    [Header("Special")]
    [SerializeField] private MeleeSpecialType specialTypeUnit = MeleeSpecialType.None;

    [Header("References")]
    [SerializeField] private UnityEngine.UI.Slider healthBar;
    [SerializeField] private Transform individualUnitCanvas;
    private Camera mainCamera;

    [Header("Properties")]
    [SerializeField] private float stoppingDistance = 2f;
    [SerializeField] private float attackRate = 2f;
    private float attackTimestamp;
    private float attackCooldown;

    [Header("Explode")]
    [SerializeField] private float radius = 5f;
    private float power;
    private Animator anim;
    private bool canExitAttackState = true;

    private float AttackRate
    {
        get => attackRate;
        set
        {
            attackRate = value;
            attackCooldown = 1 / attackRate;
        }
    }

    void Start()
    {
        base.Start();
        AttackRate = attackRate;
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();
        mainCamera = Camera.main;
        agent.stoppingDistance = stoppingDistance;

        healthBar.maxValue = stats.maxHealth;
        healthBar.value = CurrentHealth;
    }

    void Update()
    {
        if (individualUnitCanvas != null)
            individualUnitCanvas.LookAt(mainCamera.transform.position);

        if (currentStateAI == StatesAI.MovingToTarget)
        {
            SetAgentDestination(agent, currentTarget.transform.position);
        }
        else if(currentStateAI == StatesAI.Attack)
        {
            canExitAttackState = false;
            LookAtTarget();
            if (specialTypeUnit == MeleeSpecialType.None)
            {
                if (Time.time >= attackTimestamp)
                    Attack();
            }
            else if (specialTypeUnit == MeleeSpecialType.Kamikadze)
            {
                if (Time.time >= attackTimestamp)
                {
                    AttackAndExplode();
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
        if (currentStateAI == StatesAI.Idle)
            return;

        anim.SetBool("isRun", false);
        anim.SetBool("isAttack", false);
        anim.SetBool("isIdle", true);
        currentStateAI = StatesAI.Idle;
        currentTarget = null;
        currentTargetDamageable = null;
    }

    private void GoToAttackState()
    {
        if (currentStateAI == StatesAI.Attack)
            return;

        attackTimestamp = Time.time + attackCooldown;
        currentTargetDamageable = currentTarget.GetComponent<IDamageable>();
        currentStateAI = StatesAI.Attack;
        anim.SetBool("isRun", false);
        anim.SetBool("isAttack", true);
    }

    private void GoToMovingToTarget()
    {
        if (currentStateAI == StatesAI.MovingToTarget)
            return;

        currentStateAI = StatesAI.MovingToTarget;
        anim.SetBool("isRun", true);
        anim.SetBool("isAttack", false);
    }

    private void Attack()
    {
        attackTimestamp = Time.time + attackCooldown;
        currentTargetDamageable?.TakeDamage(stats.damage, this);
        canExitAttackState = true;
    }

    private void AttackAndExplode()
    {
        currentTargetDamageable.TakeDamage(stats.damage, this);
        Explode();
        this.gameObject.SetActive(false);
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
