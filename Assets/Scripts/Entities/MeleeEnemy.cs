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
            currentTargetDamageable.TakeKnockback(stats.knockbackPower, thisTransform.forward);
            this.gameObject.SetActive(false);
        }

        ChooseNewTarget(true);

        if (currentTarget != null)
        {
            if ((currentTarget.transform.position - thisTransform.position).magnitude <= agent.stoppingDistance + 0.01f)
            {
                currentTargetDamageable = currentTarget.GetComponent<IDamageable>();
                currentStateAI = StatesAI.Attack;
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
}
