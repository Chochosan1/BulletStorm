using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
[RequireComponent(typeof(NavMeshAgent))]
public sealed class MeleeEnemy : BaseEnemy
{
    enum StatesAI { Idle, MovingToTarget, Attack };
    private StatesAI currentStateAI;

    private NavMeshAgent agent;
    private IDamageable currentTargetDamageable;

    [Header("Explode")]
    [SerializeField] private float radius = 5f;
    private float power;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        base.Start();
    }

    void Update()
    {
        if (currentStateAI == StatesAI.MovingToTarget)
        {
            SetAgentDestination(agent, currentTarget.transform.position);
        }
        else if(currentStateAI == StatesAI.Attack)
        {
            currentTargetDamageable.TakeDamage(stats.damage, this);
            Explode();
            this.gameObject.SetActive(false);
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
                currentStateAI = StatesAI.MovingToTarget;
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
