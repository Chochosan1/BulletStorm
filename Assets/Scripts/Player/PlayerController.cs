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

    private Animator anim;
    private Camera mainCamera;
    private Rigidbody rb;
    private Transform thisTransform;
    private Vector3 moveDir;
    private Vector2 moveAxis;
    private bool isGrounded = false;

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

            Debug.Log("SR" + shootRate);
        }
    }

    public float MaxHealth
    {
        get => maxHealth;

        set
        {
            maxHealth = value;
            healthBar.maxValue = maxHealth;
            Debug.Log("HP" + maxHealth);
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
            Debug.Log("Knockback" + knockbackPower);
        }
    }

    public float AttackDamage
    {
        get => attackDamage;

        set
        {
            attackDamage = value;
            Debug.Log("AD" + attackDamage);
        }
    }


    private float CurrentHealth
    {
        get => currentHealth;
        set
        {
            currentHealth = value >= maxHealth ? maxHealth : value;
            healthBar.value = currentHealth;
            Debug.Log(currentHealth);
        }
    }

    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
        }
    }

    void Start()
    {
        this.tag = "Player";
        uc = GetComponent<UpgradeController>();

        rb = GetComponent<Rigidbody>();
        thisColl = GetComponent<Collider>();
        thisTransform = transform;
        mainCamera = Camera.main;
        anim = GetComponent<Animator>();

        ShootRate = shootRate; //calculate initial stuff in the setter
        CurrentHealth = maxHealth;
        MaxHealth = maxHealth;
        CurrentHealth = currentHealth;
    }

    void Update()
    {
        if (individualUnitCanvas != null)
            individualUnitCanvas.LookAt(mainCamera.transform.position);
        
        CheckInput();
        RotateWithMouse();

        if (moveAxis.x != 0 || moveAxis.y != 0)
        {
            moveDir = Vector3.forward * moveAxis.y + Vector3.right * moveAxis.x;
       //     thisTransform.Translate(moveDir.normalized * movementSpeed * Time.deltaTime, Space.World);
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


        if(Input.GetKeyDown(KeyCode.Q))
        {
            Dash();
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            Jump();
        }

        if(Input.GetKey(KeyCode.Mouse0))
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
        if(Time.time >= shootTimestamp)
        {
            shootTimestamp = Time.time + shootCooldown;
            GameObject projectileCopy = Instantiate(projectile, projectileSpawnPosition.position, projectile.transform.rotation);
            projectileCopy.transform.forward = projectileSpawnPosition.transform.forward;

            if(uc.IsUpgradeUnlocked(UpgradeController.UpgradeType.TripleProjectile))
            {
                projectileCopy = Instantiate(projectile, uc.tripleProjectileSpawnLeft.position, uc.tripleProjectileSpawnLeft.rotation);
                projectileCopy.transform.forward = uc.tripleProjectileSpawnLeft.forward;

                projectileCopy = Instantiate(projectile, uc.tripleProjectileSpawnRight.position, uc.tripleProjectileSpawnRight.rotation);
                projectileCopy.transform.forward = uc.tripleProjectileSpawnRight.forward;
            }

            if(uc.IsUpgradeUnlocked(UpgradeController.UpgradeType.ProjectileBackwards))
            {
                projectileCopy = Instantiate(projectile, uc.projectileBackwardsSpawn.position, uc.projectileBackwardsSpawn.rotation);
                projectileCopy.transform.forward = uc.projectileBackwardsSpawn.forward;
            }

            if(uc.IsUpgradeUnlocked(UpgradeController.UpgradeType.ProjectilesSideways))
            {
                projectileCopy = Instantiate(projectile, uc.sidewaysProjectileLeft.position, uc.sidewaysProjectileLeft.rotation);
                projectileCopy.transform.forward = uc.sidewaysProjectileLeft.forward;

                projectileCopy = Instantiate(projectile, uc.sidewaysProjectileRight.position, uc.sidewaysProjectileRight.rotation);
                projectileCopy.transform.forward = uc.sidewaysProjectileRight.forward;
            }
        }     
    }

    private bool IsObjectInDashDistance()
    {
        RaycastHit hit;
        return Physics.Raycast(thisTransform.position, transform.TransformDirection(Vector3.forward), out hit, dashDistance);
    }

    private void Dash()
    {
        if (Time.time < dashTimestamp)
            return;

        dashEffect.SetActive(true);
        StartCoroutine(DisableObjectAfter(dashEffect, 0.75f));
        dashTimestamp = Time.time + dashCooldown;
        if(IsObjectInDashDistance())
        {
            RaycastHit hit;
            Physics.Raycast(thisTransform.position, transform.TransformDirection(Vector3.forward), out hit, dashDistance);

            thisTransform.position += thisTransform.forward * (hit.distance - thisColl.bounds.extents.magnitude);
         //   Debug.Log(hit.distance - gameObject.GetComponent<Collider>().bounds.extents.magnitude);
        }
        else
        {
            thisTransform.position += thisTransform.forward * dashDistance;
        }
    }

    private void CalculateShootAnimSpeed()
    {
        //testing shows that the attack anim speed should be half of the attack speed of the player
        attackAnimSpeed = shootRate * 0.5f;

        //max attack animation speed should be capped at 2.5 as it seems weird to more than that
        if (attackAnimSpeed >= 3f)
            attackAnimSpeed = 3f;
        else if (attackAnimSpeed <= 0.5f)
            attackAnimSpeed = 0.5f;

        anim.SetFloat("attackSpeed", attackAnimSpeed);
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

        if(!healEffect.activeSelf)
        {
            healEffect.SetActive(true);
            StartCoroutine(DisableObjectAfter(healEffect, 1f));
        }       
    }

    private IEnumerator DisableObjectAfter(GameObject objectToDisable, float disableAfter)
    {
        yield return new WaitForSeconds(disableAfter);
        objectToDisable.SetActive(false);
    }
}
