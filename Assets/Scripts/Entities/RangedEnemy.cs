using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
[RequireComponent(typeof(NavMeshAgent))]
public sealed class RangedEnemy : BaseEnemy
{
    enum RangedSpecialType { None, MultipleCastAlways, MultipleCastSometimes }

    enum StatesAI { Idle, MovingToTarget, Attack, Dead, Stunned };
    private StatesAI currentStateAI;

    private NavMeshAgent agent;
    private IDamageable currentTargetDamageable;

    [Header("Special")]
    [SerializeField] private RangedSpecialType specialTypeUnit = RangedSpecialType.None;
    [SerializeField] private Transform[] projectileSpawnPositions;
    [Tooltip("If unit type is MulticastSometimes, this value will be used to multicast every X casts.")]
    [SerializeField] private int multipleCastEveryXshots = 2;

    [Header("References")]
    [SerializeField] private UnityEngine.UI.Slider healthBar;
    [SerializeField] private Transform individualUnitCanvas;
    private Transform mainCameraTransform;

    [Header("Properties")]
    [Space]
    [SerializeField] private Transform projectileSpawnPosition;
    [SerializeField] private GameObject projectile;
    [SerializeField] private GameObject projectileMulticast;
    [SerializeField] private float shootRate = 2f;
    [SerializeField] private float stoppingDistance = 12f;
    private bool isCurrentlySlowed = false;

    [Header("Camera Shake")]
    [SerializeField] private bool useCamShakeOnDeath = true;
    [SerializeField] private float camShakeDuration = 0.1f;
    [SerializeField] private float camShakeMagnitude = 0.2f;
    private float shootTimestamp;
    private float shootCooldown;
    private MeshRenderer meshRend;

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
        meshRend = GetComponentInChildren<MeshRenderer>();
        ShootRate = shootRate;
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();
        agent.stoppingDistance = stoppingDistance;
        mainCameraTransform = Camera.main.transform;
        individualUnitCanvas.gameObject.SetActive(false);
        healthBar.maxValue = stats.maxHealth;
        healthBar.value = CurrentHealth;
    }

    void Update()
    {
        if ((!meshRend.isVisible && currentTarget == null) || currentStateAI == StatesAI.Dead)
            return;

        if (individualUnitCanvas != null)
            individualUnitCanvas.LookAt(mainCameraTransform.transform.position);

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

            if (specialTypeUnit == RangedSpecialType.None)
            {
                if (Time.time >= shootTimestamp)
                    Shoot();
            }
            else if (specialTypeUnit == RangedSpecialType.MultipleCastAlways)
            {
                if (Time.time >= shootTimestamp)
                    ShootMultipleFireball();
            }
            else if (specialTypeUnit == RangedSpecialType.MultipleCastSometimes)
            {
                if (Time.time >= shootTimestamp)
                {
                    if (shotsFired >= multipleCastEveryXshots - 1)
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

        shootTimestamp = Time.time + shootCooldown;
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

    private void GoToStunnedState()
    {
        if (currentStateAI == StatesAI.Stunned)
            return;

        currentStateAI = StatesAI.Stunned;
        anim.SetBool("isFrozen", true);
        anim.SetBool("isRun", false);
        anim.SetBool("isAttack", false);
        anim.SetBool("isIdle", false);

        if (agent != null)
            agent.destination = thisTransform.position;
    }

    public override void TakeDamage(float damage, IDamageable owner)
    {
        if (currentTarget == null)
        {
            ChooseNewTarget(false);
        }

        individualUnitCanvas.gameObject.SetActive(true);
        CurrentHealth -= damage;
        healthBar.value = CurrentHealth;

        if (CurrentHealth <= 0)
        {
            Explode();
            //    this.gameObject.SetActive(false);
            deathParticle.SetActive(true);
            deathParticle.gameObject.transform.SetParent(null);
            CameraFollowTarget.Instance.ShakeCamera(camShakeDuration, camShakeMagnitude, false);
            Destroy(deathParticle.gameObject, 2f);
            Destroy(this.gameObject, 0f);
        }
    }

    private void Shoot()
    {
        shootTimestamp = Time.time + shootCooldown;
        GameObject projectileCopy = Instantiate(projectile, projectileSpawnPosition.position, projectile.transform.rotation);
        projectileCopy.transform.forward = projectileSpawnPosition.transform.forward;
        canExitAttackState = true;
    }

    private void ShootMultipleFireball()
    {
        shootTimestamp = Time.time + shootCooldown;
        foreach (Transform psp in projectileSpawnPositions)
        {
            GameObject projectileCopy = Instantiate(projectileMulticast, psp.position, projectileMulticast.transform.rotation);
            projectileCopy.transform.forward = psp.transform.forward;
        }

        canExitAttackState = true;
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
