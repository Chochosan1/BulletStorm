using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
[RequireComponent(typeof(NavMeshAgent))]
public sealed class MeleeEnemy : BaseEnemy
{
    enum MeleeSpecialType { None, Kamikadze }

    enum StatesAI { Idle, MovingToTarget, Attack, Dead, Stunned };
    private StatesAI currentStateAI;

    private NavMeshAgent agent;
    private IDamageable currentTargetDamageable;

    [Header("Special")]
    [SerializeField] private MeleeSpecialType specialTypeUnit = MeleeSpecialType.None;

    [Header("References")]
    [SerializeField] private UnityEngine.UI.Slider healthBar;
    [SerializeField] private Transform individualUnitCanvas;
    private Transform mainCameraTransform;

    [Header("Properties")]
    [SerializeField] private float stoppingDistance = 2f;
    [SerializeField] private float attackRate = 2f;
    private bool isCurrentlySlowed = false;
    private bool isFriendlyUnit;

    [Header("Camera Shake")]
    [SerializeField] private bool useCamShakeOnDeath = true;
    [SerializeField] private float camShakeDuration = 0.1f;
    [SerializeField] private float camShakeMagnitude = 0.2f;
    private float attackTimestamp;
    private float attackCooldown;

    [Header("Explode")]
    [SerializeField] private float radius = 5f;
    [SerializeField] private LayerMask explodeLayerMask;
    private float power;
    private Animator anim;
    private bool canExitAttackState = true;

    private MeshRenderer meshRend;
    private Collider thisColl;

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
        meshRend = GetComponentInChildren<MeshRenderer>();
        thisColl = GetComponent<Collider>();
        AttackRate = attackRate;
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();
        mainCameraTransform = Camera.main.transform;
        agent.stoppingDistance = stoppingDistance;
        individualUnitCanvas.gameObject.SetActive(false);
        healthBar.maxValue = stats.maxHealth;
        healthBar.value = CurrentHealth;

        if (this.gameObject.layer == LayerMask.NameToLayer("Allied"))
            isFriendlyUnit = true;
        else
            isFriendlyUnit = false;
    }

    void Update()
    {
        if ((!meshRend.isVisible && currentTarget == null) || currentStateAI == StatesAI.Dead)
            return;

        if (individualUnitCanvas != null)
            individualUnitCanvas.LookAt(mainCameraTransform.position);

        if (currentStateAI == StatesAI.MovingToTarget)
        {
            if (currentTarget == null)
            {
                GoToIdleState(false);
                return;
            }

            SetAgentDestination(agent, currentTarget.transform.position);
        }
        else if (currentStateAI == StatesAI.Attack)
        {
            if (currentTarget == null)
            {
                GoToIdleState(false);
                return;
            }

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
                GoToAttackState(false);
            }
            else
            {
                if (!canExitAttackState)
                    return;
                GoToMovingToTarget(false);
            }
        }
        else
        {
            if (!canExitAttackState)
                return;

            GoToIdleState(false);
        }
    }

    private void GoToIdleState(bool forceThisState)
    {
        if (!forceThisState && (currentStateAI == StatesAI.Idle || currentStateAI == StatesAI.Stunned))
            return;

        anim.SetBool("isRun", false);
        anim.SetBool("isAttack", false);
        anim.SetBool("isIdle", true);
        anim.SetBool("isFrozen", false);
        currentStateAI = StatesAI.Idle;
        currentTarget = null;
        currentTargetDamageable = null;
        canExitAttackState = true;
    }

    private void GoToAttackState(bool forceThisState)
    {
        if (!forceThisState && (currentStateAI == StatesAI.Attack || currentStateAI == StatesAI.Stunned))
            return;

        //kamikadze type should detonate instantly on contact
        if (specialTypeUnit != MeleeSpecialType.Kamikadze)
        {
            attackTimestamp = Time.time + attackCooldown;
        }

        currentTargetDamageable = currentTarget.GetComponent<IDamageable>();
        currentStateAI = StatesAI.Attack;
        anim.SetBool("isRun", false);
        anim.SetBool("isAttack", true);
        anim.SetBool("isIdle", false);
        anim.SetBool("isFrozen", false);
    }

    private void GoToMovingToTarget(bool forceThisState)
    {
        if (!forceThisState && (currentStateAI == StatesAI.MovingToTarget || currentStateAI == StatesAI.Stunned))
            return;

        currentStateAI = StatesAI.MovingToTarget;
        anim.SetBool("isRun", true);
        anim.SetBool("isAttack", false);
        anim.SetBool("isIdle", false);
        anim.SetBool("isFrozen", false);
    }

    private void GoToDeadState()
    {
        currentStateAI = StatesAI.Dead;
    }

    private void Attack()
    {
        attackTimestamp = Time.time + attackCooldown;
        if (currentTargetDamageable != null && currentTarget != null)
            currentTargetDamageable?.TakeDamage(stats.damage, this);
        canExitAttackState = true;
    }

    private void AttackAndExplode()
    {
        if (currentTargetDamageable != null && currentTarget != null)
            currentTargetDamageable?.TakeDamage(stats.damage, this);
        Explode();
        // this.gameObject.SetActive(false);
        Destroy(this.gameObject);
    }

    private void GoToStunnedState()
    {
        if (currentStateAI == StatesAI.Stunned)
            return;

        currentStateAI = StatesAI.Stunned;
        anim.SetBool("isFrozen", true);
        anim.SetBool("isRun", false);
        anim.SetBool("isAttack", false);
        anim.SetBool("isIdle", false);
        if (agent.enabled)
            agent.destination = thisTransform.position;
    }

    public override void TakeDamage(float damage, IDamageable owner)
    {
        if (currentTarget == null)
        {
            ChooseNewTarget(false);
        }

        if (individualUnitCanvas != null)
            individualUnitCanvas.gameObject.SetActive(true);

        CurrentHealth -= damage;
        RollOnDamagedBonuses();
        healthBar.value = CurrentHealth;

        if (CurrentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        if (currentStateAI == StatesAI.Dead)
            return;

        if (!isFriendlyUnit)
            Chochosan.CustomEventManager.OnEnemyKilled?.Invoke();

        if (isBoss)
            Chochosan.CustomEventManager.OnBossKilled?.Invoke();

        RollOnDeathBonuses();
        DetermineLoot();

        usedDeathParticle = deathParticle;

        if (UpgradeController.Instance.IsUpgradeUnlocked(UpgradeController.UpgradeType.ExplodeOnDeath))
            Explode();
        //   this.gameObject.SetActive(false);
        GoToDeadState();
        usedDeathParticle.SetActive(true);
        usedDeathParticle.gameObject.transform.SetParent(null);
        CameraFollowTarget.Instance.ShakeCamera(camShakeDuration, camShakeMagnitude, false);
        Destroy(usedDeathParticle.gameObject, 2f);
        try
        {
            Destroy(this.gameObject);
        }
        catch(System.Exception e)
        {
            Debug.Log("Caught: " + e);
        }

    }

    ///<summary>Rolls the chances for all bonuses when the entity is damaged.</summary>
    private void RollOnDamagedBonuses()
    {
        if (UpgradeController.Instance.IsUpgradeUnlocked(UpgradeController.UpgradeType.OneShotChance))
        {
            if (Random.Range(0f, 1f) <= UpgradeController.Instance.oneShotChance)
            {
                if (!isBoss)
                    CurrentHealth -= stats.maxHealth;
                else
                    CurrentHealth -= stats.maxHealth * 0.05f;
            }
        }
    }

    ///<summary>Rolls the chances for all bonuses when the entity is killed.</summary>
    private void RollOnDeathBonuses()
    {
        if (UpgradeController.Instance.IsUpgradeUnlocked(UpgradeController.UpgradeType.TornadoChanceOnDeath))
        {
            if (Random.Range(0f, 1f) <= UpgradeController.Instance.tornadoChanceToSpawnOnDeath)
            {
                Instantiate(UpgradeController.Instance.tornadoPrefab, thisTransform.position, UpgradeController.Instance.tornadoPrefab.transform.rotation);
            }
        }

        if (UpgradeController.Instance.IsUpgradeUnlocked(UpgradeController.UpgradeType.FreezeZoneOnDeath))
        {
            if (Random.Range(0f, 1f) <= UpgradeController.Instance.freezeZoneChanceToSpawnOnDeath)
            {
                Instantiate(UpgradeController.Instance.freezeZonePrefab, thisTransform.position, UpgradeController.Instance.freezeZonePrefab.transform.rotation);
            }
        }
    }

    public void Explode()
    {
        float chanceRolled = Random.Range(0f, 1f);
        if (chanceRolled >= 0.55f)
            return;

        usedDeathParticle = explodedDeathParticle;
        Debug.Log("DEATHPARTICLE CHANCED : +  " + usedDeathParticle.name);
        Vector3 explosionPos = thisTransform.position;
        Collider[] colliders = Physics.OverlapSphere(explosionPos, radius, explodeLayerMask);
        foreach (Collider hit in colliders)
        {
            if (hit.gameObject != this.gameObject)
            {
                Rigidbody rb = hit.GetComponent<Rigidbody>();
                IDamageable tempDamageable = hit.GetComponent<IDamageable>();

                if (rb != null)
                {
                    power = Random.Range(stats.knockbackPower, stats.knockbackPower * 3f);
                    rb.AddExplosionForce(power, explosionPos, radius, 1.0F);
                }

                if (tempDamageable != null)
                {
                    tempDamageable.TakeDamage(stats.damage, null);
                }
            }
        }
    }

    public override void Freeze(float duration, float chance)
    {
        if (currentStateAI == StatesAI.Stunned)
            return;

        float freezeChanceRolled = Random.Range(0f, 1f);

        if (freezeChanceRolled <= chance)
            StartCoroutine(GetFrozen(duration));
    }

    private IEnumerator GetFrozen(float duration)
    {
        GoToStunnedState();
        frozenParticle.SetActive(true);
        yield return new WaitForSeconds(duration);
        frozenParticle.SetActive(false);
        GoToIdleState(true);
    }

    private IEnumerator Slow(float duration, float slowMultiplier)
    {
        isCurrentlySlowed = true;
        float originalSpeed = agent.speed;
        agent.speed *= slowMultiplier;
        yield return new WaitForSeconds(duration);
        agent.speed = originalSpeed;
        isCurrentlySlowed = false;
    }

    public override void GetSlowed(float duration, float slowMultiplier, float chance)
    {
        if (isCurrentlySlowed)
            return;

        float slowChanceRolled = Random.Range(0f, 1f);

        if (slowChanceRolled <= chance)
            StartCoroutine(Slow(duration, slowMultiplier));
    }
}
