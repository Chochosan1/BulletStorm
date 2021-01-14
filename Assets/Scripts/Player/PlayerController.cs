using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour, IDamageable
{
    public static PlayerController Instance;

    [Header("References")]
    [Space]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private Transform projectileSpawnPosition;
    [SerializeField] private GameObject projectile;
    [SerializeField] private UnityEngine.UI.Slider healthBar;
    [SerializeField] private Transform individualUnitCanvas;
    [SerializeField] private GameObject healEffect;
    [SerializeField] private GameObject dashEffect;
    [SerializeField] private GameObject frozenParticle;
    [SerializeField] private GameObject slowedMovementParticle;
    [SerializeField] private GameObject normalMovementParticle;
    [SerializeField] private GameSounds gameSoundsAsset;
    private Collider thisColl;

    [Header("General stats")]
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float attackDamage = 10f;
    [SerializeField] private float knockbackPower = 0f;
    [SerializeField] private float maxKnockbackPower = 8f;

    [Header("Movement")]
    [Space]
    [SerializeField] private float movementSpeed = 15f;
    [SerializeField] private float jumpForce = 5f;
    [SerializeField] private float dashDistance = 5f;
    [SerializeField] private float dashCooldown = 3f;
    private float dashTimestamp;
    [SerializeField] private LayerMask groundMask;
    [SerializeField] private float distanceToGround = 0.1f;
    private bool isCurrentlySlowed = false;

    [Header("Shoot")]
    [Space]
    [SerializeField] private float shootRate = 2f;
    [SerializeField] private float maxShootRate = 10f;
    [SerializeField] private float minShootRate = 1f;
    private float attackAnimSpeed;
    private float shootTimestamp;
    private float shootCooldown;
    private float currentHealth;

    private UpgradeController uc;
    private List<Projectile_Controller> projectilePool;
    private int currentPoolItem = 0;

    private Animator anim;
    private Camera mainCamera;
    private Rigidbody rb;
    private Transform thisTransform;
    private Vector3 moveDir;
    private Vector2 moveAxis;
    private bool isGrounded = false;
    private bool isPlayerDisabled = false;

    private int enemiesKilled = 0;
    public int EnemiesKilled
    {
        get => enemiesKilled;
        set
        {
            enemiesKilled = value;

            Chochosan.CustomEventManager.OnPlayerStatsChanged("enemiesKilled");
        }
    }

    public float ShootRate
    {
        get => shootRate;
        set
        {
            shootRate = value;

            if (shootRate >= maxShootRate)
                shootRate = maxShootRate;
            else if (shootRate <= minShootRate)
                shootRate = minShootRate;

            shootCooldown = 1 / shootRate;

            CalculateShootAnimSpeed();

            Chochosan.CustomEventManager.OnPlayerStatsChanged?.Invoke("shootRate");
            //    Debug.Log("SR" + shootRate);
        }
    }

    public float MaxHealth
    {
        get => maxHealth;

        set
        {
            float amountHealthWillIncreaseBy = value - maxHealth; //get the amount health will increase by
            maxHealth = value;
            healthBar.maxValue = maxHealth;
            CurrentHealth += amountHealthWillIncreaseBy; //heal the player for the amount of bonus max health
                                    //     Debug.Log("HP" + maxHealth);

            Chochosan.CustomEventManager.OnPlayerStatsChanged?.Invoke("maxHealth");
        }
    }

    public float KnockbackPower
    {
        get => knockbackPower;

        set
        {
            knockbackPower = value;
            if (knockbackPower >= maxKnockbackPower)
                knockbackPower = maxKnockbackPower;
            //    Debug.Log("Knockback" + knockbackPower);
        }
    }

    public float AttackDamage
    {
        get => attackDamage;

        set
        {
            attackDamage = value;
            //    Debug.Log("AD" + attackDamage);
            Chochosan.CustomEventManager.OnPlayerStatsChanged?.Invoke("attackDamage");
        }
    }


    public float CurrentHealth
    {
        get => currentHealth;
        set
        {
            currentHealth = value >= maxHealth ? maxHealth : value;
            if (currentHealth < 0)
                currentHealth = 0;

            healthBar.value = currentHealth;

            Chochosan.CustomEventManager.OnPlayerStatsChanged?.Invoke("currentHealth");
            //     Debug.Log(currentHealth);
        }
    }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(this.gameObject);
            return;
        }

        mainCamera = Camera.main;
        mainCamera.GetComponent<CameraFollowTarget>().SetCameraTarget(transform);
    }

    void Start()
    {
        this.tag = "Player";

        ///Event subscription///

        Chochosan.CustomEventManager.OnUpgradeLearned += OnNewUpgradeLearned;
        Chochosan.CustomEventManager.OnEnemyKilled += OnEnemyKilled;

        ////////////////////////


        uc = GetComponent<UpgradeController>();
        

        rb = GetComponent<Rigidbody>();
        thisColl = GetComponent<Collider>();
        thisTransform = transform;
        
        anim = GetComponent<Animator>();

        //trigger the setters to calculate initial stuff
        ShootRate = shootRate;
        CurrentHealth = maxHealth;
        MaxHealth = maxHealth;
        //  CurrentHealth = currentHealth;


        projectilePool = new List<Projectile_Controller>();
        //pool all player projectiles
        for (int i = 0; i < 150; i++)
        {
            GameObject projectileCopy = Instantiate(projectile, projectileSpawnPosition.position, projectile.transform.rotation);
            projectileCopy.SetActive(false);
            projectilePool.Add(projectileCopy.GetComponent<Projectile_Controller>());
            DontDestroyOnLoad(projectileCopy);
        }
    }

    private void OnDisable()
    {
        ///Event unsubscription///

        Chochosan.CustomEventManager.OnUpgradeLearned -= OnNewUpgradeLearned;
        Chochosan.CustomEventManager.OnEnemyKilled += OnEnemyKilled;

        ////////////////////////
    }

    void Update()
    {
        if (individualUnitCanvas != null)
            individualUnitCanvas.LookAt(mainCamera.transform.position);

        if (isPlayerDisabled)
            return;

        CheckInput();
        RotateWithMouse();

        if (moveAxis.x != 0 || moveAxis.y != 0)
        {
            moveDir = Vector3.forward * moveAxis.y + Vector3.right * moveAxis.x;
            //          thisTransform.Translate(moveDir.normalized * movementSpeed * Time.deltaTime, Space.World);
            rb.AddForce(moveDir.normalized * movementSpeed * Time.deltaTime, ForceMode.VelocityChange);
        }
    }


    private void RotateWithMouse()
    {
        Ray cameraRay = mainCamera.ScreenPointToRay(Input.mousePosition);
        Plane groundPlane = new Plane(Vector3.up, Vector3.zero);
        float rayLength;

        if (groundPlane.Raycast(cameraRay, out rayLength))
        {
            Vector3 pointToLook = cameraRay.GetPoint(rayLength);

            thisTransform.LookAt(new Vector3(pointToLook.x, thisTransform.position.y, pointToLook.z));
        }
    }

    private void CheckInput()
    {
        anim.SetBool("isIdle", true);
        anim.SetBool("isRun", false);
        anim.SetBool("isAttack", false);
        anim.SetBool("isUpgrading", false);
        moveAxis.x = 0;
        moveAxis.y = 0;

        if (Input.GetKey(KeyCode.W))
        {
            anim.SetBool("isRun", true);
            moveAxis.y = 1;
        }
        else if (Input.GetKey(KeyCode.S))
        {
            anim.SetBool("isRun", true);
            moveAxis.y = -1;
        }


        if (Input.GetKey(KeyCode.A))
        {
            anim.SetBool("isRun", true);
            moveAxis.x = -1;
        }
        else if (Input.GetKey(KeyCode.D))
        {
            anim.SetBool("isRun", true);
            moveAxis.x = 1;
        }


        if (Input.GetKeyDown(KeyCode.Q))
        {
            Dash(false, thisTransform.forward);
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            Jump();
        }

        if (Input.GetKey(KeyCode.Mouse0))
        {
            anim.SetBool("isAttack", true);
            Shoot();
        }
    }

    private void Jump()
    {
        isGrounded = Physics.CheckSphere(groundCheck.position, distanceToGround, groundMask);
        if (!isGrounded)
            return;

        rb.AddForce(thisTransform.up * jumpForce, ForceMode.Impulse);
    }

    private void Shoot()
    {
        if (Time.time >= shootTimestamp)
        {
            if (currentPoolItem >= projectilePool.Count)
                currentPoolItem = 0;

            shootTimestamp = Time.time + shootCooldown;

            projectilePool[currentPoolItem].gameObject.SetActive(true);
            projectilePool[currentPoolItem].ResetProjectileFromPool(projectileSpawnPosition);

            if (uc.IsUpgradeUnlocked(UpgradeController.UpgradeType.TripleProjectile))
            {
                currentPoolItem++;
                if (currentPoolItem >= projectilePool.Count)
                    currentPoolItem = 0;

                projectilePool[currentPoolItem].gameObject.SetActive(true);
                projectilePool[currentPoolItem].ResetProjectileFromPool(uc.tripleProjectileSpawnLeft);

                currentPoolItem++;
                if (currentPoolItem >= projectilePool.Count)
                    currentPoolItem = 0;

                projectilePool[currentPoolItem].gameObject.SetActive(true);
                projectilePool[currentPoolItem].ResetProjectileFromPool(uc.tripleProjectileSpawnRight);
            }

            if (uc.IsUpgradeUnlocked(UpgradeController.UpgradeType.ProjectileBackwards))
            {
                currentPoolItem++;
                if (currentPoolItem >= projectilePool.Count)
                    currentPoolItem = 0;

                projectilePool[currentPoolItem].gameObject.SetActive(true);
                projectilePool[currentPoolItem].ResetProjectileFromPool(uc.projectileBackwardsSpawn);
            }

            if (uc.IsUpgradeUnlocked(UpgradeController.UpgradeType.ProjectilesSideways))
            {
                currentPoolItem++;
                if (currentPoolItem >= projectilePool.Count)
                    currentPoolItem = 0;

                projectilePool[currentPoolItem].gameObject.SetActive(true);
                projectilePool[currentPoolItem].ResetProjectileFromPool(uc.sidewaysProjectileLeft);

                currentPoolItem++;
                if (currentPoolItem >= projectilePool.Count)
                    currentPoolItem = 0;

                projectilePool[currentPoolItem].gameObject.SetActive(true);
                projectilePool[currentPoolItem].ResetProjectileFromPool(uc.sidewaysProjectileRight);
            }

            currentPoolItem++;
        }
    }

    private float CheckDistanceToObjectInFront()
    {
        RaycastHit hit;
        if (Physics.Raycast(thisTransform.position, transform.TransformDirection(Vector3.forward), out hit, dashDistance))
        {
            return hit.distance;
        }
        else
        {
            return 0;
        }

    }

    public void Dash(bool avoidCooldown, Vector3 dashDir)
    {
        if (!avoidCooldown && Time.time < dashTimestamp)
            return;

        if (!avoidCooldown)
            dashTimestamp = Time.time + dashCooldown;

        dashEffect.SetActive(true);
        StartCoroutine(DisableObjectAfter(dashEffect, 0.75f));
        float hitObjectDistance = CheckDistanceToObjectInFront();

        if (hitObjectDistance > 0.1f)
        {
            thisTransform.position += dashDir * (hitObjectDistance - thisColl.bounds.extents.magnitude);
        }
        else
        {
            thisTransform.position += dashDir * dashDistance;
        }
    }

    private void OnNewUpgradeLearned(UpgradeController.UpgradeType upgradeType)
    {
        StartCoroutine(StartUpgradeParticleSequence());
        switch (upgradeType)
        {
            case UpgradeController.UpgradeType.RotatingProjectile:
                uc.rotatingAroundPlayerProjectile.SetActive(true);
                uc.rotatingAroundPlayerProjectile.transform.SetParent(null);
                break;
            case UpgradeController.UpgradeType.DashDistanceIncreased:
                dashDistance *= 2f;
                break;
            case UpgradeController.UpgradeType.DashCooldownReduced:
                dashCooldown /= 2f;
                break;
            case UpgradeController.UpgradeType.MaxHealthIncrease:
                MaxHealth *= 2f;
                break;
            case UpgradeController.UpgradeType.AttackDamageIncrease:
                AttackDamage *= 1.5f;
                break;
            case UpgradeController.UpgradeType.AttackSpeedIncrease:
                if(ShootRate >= maxShootRate)
                {
                    AttackDamage *= 1.15f;
                }
                else
                {
                    ShootRate *= 2f;
                }
                break;
        }
    }

    private void CalculateShootAnimSpeed()
    {
        //testing shows that the attack anim speed should be half of the attack speed of the player
        attackAnimSpeed = shootRate * 0.5f;

        //max attack animation speed should be capped at 2.5 as it seems weird to be more than that
        if (attackAnimSpeed >= 3f)
            attackAnimSpeed = 3f;
        else if (attackAnimSpeed <= 0.5f)
            attackAnimSpeed = 0.5f;

        anim.SetFloat("attackSpeed", attackAnimSpeed);
    }

    private void OnEnemyKilled()
    {
        EnemiesKilled++;
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawSphere(groundCheck.position, distanceToGround);
    }

    public void TakeDamage(float damage, IDamageable owner)
    {
        CurrentHealth -= damage;
    }

    public void TakeKnockback(float knockbackPower, Vector3 knockbackDirection)
    {
        rb.AddForce(knockbackDirection * knockbackPower, ForceMode.Impulse);
    }

    public void HealSelf(float healValue)
    {
        CurrentHealth += healValue;

        if (!healEffect.activeSelf)
        {
            healEffect.SetActive(true);
            StartCoroutine(DisableObjectAfter(healEffect, 1f));
        }
    }

    public void Freeze(float duration, float chance)
    {
        if (isPlayerDisabled)
            return;

        float freezeChanceRolled = Random.Range(0f, 1f);

        if (freezeChanceRolled <= chance)
            StartCoroutine(GetFrozen(duration));
    }

    private IEnumerator GetFrozen(float duration)
    {
        isPlayerDisabled = true;
        frozenParticle.SetActive(true);
        yield return new WaitForSeconds(duration);
        frozenParticle.SetActive(false);
        isPlayerDisabled = false;
    }

    private IEnumerator Slow(float duration, float slowMultiplier)
    {
        isCurrentlySlowed = true;
        slowedMovementParticle.SetActive(true);
        normalMovementParticle.SetActive(false);
        float originalSpeed = movementSpeed;
        movementSpeed *= slowMultiplier;
        yield return new WaitForSeconds(duration);
        movementSpeed = originalSpeed;
        slowedMovementParticle.SetActive(false);
        normalMovementParticle.SetActive(true);
        isCurrentlySlowed = false;
    }

    public void GetSlowed(float duration, float slowMultiplier, float chance)
    {
        if (isCurrentlySlowed)
            return;

        float slowChanceRolled = Random.Range(0f, 1f);

        if (slowChanceRolled <= chance)
            StartCoroutine(Slow(duration, slowMultiplier));
    }

    private IEnumerator DisableObjectAfter(GameObject objectToDisable, float disableAfter)
    {
        yield return new WaitForSeconds(disableAfter);
        objectToDisable.SetActive(false);
    }

    private IEnumerator StartUpgradeParticleSequence()
    {
        anim.SetBool("isUpgrading", true);
        isPlayerDisabled = true;
        uc.upgradeParticle.SetActive(true);
        CameraFollowTarget.Instance.ShakeCamera(1.1f, 0.1f, true);
        yield return new WaitForSeconds(1.2f);
        uc.upgradeParticle.SetActive(false);
        uc.upgradeParticle2.SetActive(true);
        CameraFollowTarget.Instance.ShakeCameraWithX(0.05f, 0.3f, true);
        StartCoroutine(DisableObjectAfter(uc.upgradeParticle2, 1f));
        isPlayerDisabled = false;
    }
}
