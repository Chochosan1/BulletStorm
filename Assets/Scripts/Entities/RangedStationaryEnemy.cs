using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RangedStationaryEnemy : BaseEnemy
{
    enum RangedSpecialType { None, MultipleCastAlways, MultipleCastSometimes }

    enum StatesAI { Idle, Attack };
    private StatesAI currentStateAI;

    private IDamageable currentTargetDamageable;

    [Header("Special")]
    [SerializeField] private RangedSpecialType specialTypeUnit = RangedSpecialType.None;
    [SerializeField] private Transform[] projectileSpawnPositions;
    [Tooltip("If unit type is MulticastSometimes, this value will be used to multicast every X casts.")]
    [SerializeField] private int multipleCastEveryXshots = 2;

    [Header("References")]
    [SerializeField] private UnityEngine.UI.Slider healthBar;

    [Header("Shoot")]
    [Space]
    [SerializeField] private Transform projectileSpawnPosition;
    [SerializeField] private GameObject projectile;
    [SerializeField] private GameObject projectileMulticast;
    [SerializeField] private float shootRate = 2f;
    [SerializeField] private float shootRange = 12f;
    [SerializeField] private bool isLookAtTargetWhileShooting = true;
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
        anim = GetComponent<Animator>();

        healthBar.maxValue = stats.maxHealth;
        healthBar.value = CurrentHealth;
    }

    void Update()
    {
        if (currentStateAI == StatesAI.Attack)
        {
            canExitAttackState = false;

            if (currentTarget == null)
                currentStateAI = StatesAI.Idle;

            if(isLookAtTargetWhileShooting)
            {
                LookAtTarget();
            }      

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
            if ((currentTarget.transform.position - thisTransform.position).magnitude <= shootRange + 0.01f)
            {
                GoToAttackState();
            }
            else
            {
                if (!canExitAttackState)
                    return;
                GoToIdleState();
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
        currentStateAI = StatesAI.Idle;
        currentTarget = null;
        currentTargetDamageable = null;
    }

    private void GoToAttackState()
    {
        currentTargetDamageable = currentTarget.GetComponent<IDamageable>();
        currentStateAI = StatesAI.Attack;
    }

    public override void TakeDamage(float damage, IDamageable owner)
    {
        CurrentHealth -= damage;
        healthBar.value = CurrentHealth;

        if (CurrentHealth <= 0)
        {
            Explode();
            //    this.gameObject.SetActive(false);
            Destroy(this.gameObject);
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
}
