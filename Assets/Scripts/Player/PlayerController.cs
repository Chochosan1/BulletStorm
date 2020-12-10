using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour, IDamageable
{
    [Header("References")]
    [Space]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private Transform projectileSpawnPosition;
    [SerializeField] private GameObject projectile;
    [SerializeField] private StatsEntity playerStats;
    [SerializeField] private UnityEngine.UI.Slider healthBar;
    [SerializeField] private Transform individualUnitCanvas;
    [SerializeField] private GameObject healEffect;
    [SerializeField] private GameObject dashEffect;
    private Collider thisColl;

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
    private float shootTimestamp;
    private float shootCooldown;
    private float currentHealth;

    private Animator anim;
    private Camera mainCamera;
    private Rigidbody rb;
    private Transform thisTransform;
    private Vector3 moveDir;
    private Vector2 moveAxis;
    private bool isGrounded = false;

    private float ShootRate
    {
        get => shootRate;
        set
        {
            shootRate = value;
            shootCooldown = 1 / shootRate;
        }
    }

    private float CurrentHealth
    {
        get => currentHealth;
        set
        {
            currentHealth = value >= playerStats.maxHealth ? playerStats.maxHealth : value;
            Debug.Log(currentHealth);
        }
    }

    void Start()
    {
        ShootRate = shootRate;
        rb = GetComponent<Rigidbody>();
        thisColl = GetComponent<Collider>();
        thisTransform = transform;
        mainCamera = Camera.main;
        anim = GetComponent<Animator>();

        CurrentHealth = playerStats.maxHealth;
        healthBar.maxValue = playerStats.maxHealth;
        healthBar.value = CurrentHealth;
    }

    void Update()
    {
        if (individualUnitCanvas != null)
            individualUnitCanvas.LookAt(mainCamera.transform.position);
        
        CheckInput();
        RotateWithMouse();

        if(moveAxis.x != 0 || moveAxis.y != 0)
        {
            moveDir = Vector3.forward * moveAxis.y + Vector3.right * moveAxis.x;
            rb.AddForce(moveDir.normalized * movementSpeed * Time.deltaTime, ForceMode.VelocityChange);
        }

        //if (canDash)
        //{
        //    rb.velocity = thisTransform.forward * dashForce;
        //}
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

    private void OnDrawGizmos()
    {
        Gizmos.DrawSphere(groundCheck.position, distanceToGround);
    }

    public void TakeDamage(float damage, IDamageable owner)
    {
        CurrentHealth -= damage;
        healthBar.value = CurrentHealth;
    }

    public void TakeKnockback(float knockbackPower, Vector3 knockbackDirection)
    {
        rb.AddForce(knockbackDirection * knockbackPower, ForceMode.Impulse);
    }

    public void HealSelf(float healValue)
    {
        CurrentHealth += healValue;
        healthBar.value = CurrentHealth;

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
