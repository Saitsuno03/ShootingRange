using System.Collections;
using UnityEngine;

public class Player : MonoBehaviour
{
    // Components
    private Rigidbody2D rb;
    private Animator anim;
    private SpriteRenderer spriteRenderer;

    // Movement Parameters
    public float walkSpeed = 2f;
    public float runSpeed = 5f;
    public float dashCooldown = 5f;
    private float dashCooldownTimer = 0f;

    // Animation States
    private bool isAwake = false;
    private bool isCharging = false;
    private bool isDashing = false;
    private bool isShooting = false;

    // Shooting Parameters
    public float chargeTime = 2f;
    private float chargeTimer = 0f;

    // Health Parameters
    public int maxHealth = 100;
    private int currentHealth;

    // Bullet Parameters
    public int maxBullets = 30;
    private int currentBullets;

    private bool isDead = false; // To track if the player is dead

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        // Initialize Health and Bullets
        currentHealth = maxHealth;
        currentBullets = maxBullets;

        // Player starts in Sleep state
        anim.Play("Sleep_player");
        StartCoroutine(StartWakePlayer());
    }

    void Update()
    {
        if (!isAwake) return;

        HandleMovement();
        if (Input.GetButtonDown("Fire1") && !isDashing)//checks shoot
        {
            Debug.Log("hello in f1");
            HandleShooting();
        }
        if (Input.GetButtonDown("Fire2") && dashCooldownTimer <= 0 && !isDashing)//checks dash
        {
            Debug.Log("hello in f2");
            HandleDash();
        }
        HandleDamage();

        // Reset dash cooldown
        if (dashCooldownTimer > 0) dashCooldownTimer -= Time.deltaTime;
    }

    // Coroutine to handle waking up after 1 second
    IEnumerator StartWakePlayer()
    {
        // add stop animation or change this into a trigger flow
        yield return new WaitForSeconds(1f);
        anim.SetTrigger("TriggerWake");
        yield return new WaitForSeconds(anim.GetCurrentAnimatorStateInfo(0).length); // Wait for wake animation to finish
        

        // Hold the last frame of the wake animation
        anim.Play("Wake_player");
        isAwake = true;
    }

    void HandleMovement()
    {
        float move = Input.GetAxis("Horizontal"); // Get horizontal input (A and D or Left and Right Arrow keys)
        float speed = 0f; // Default speed to 0

        // Check if the player is trying to move
        if (Mathf.Abs(move) > 0.1f)
        {
            // Check for running (hold Shift) or walking
            if (Input.GetKey(KeyCode.LeftShift))
            {
                speed = runSpeed; // Set speed to runSpeed for running
                rb.velocity = new Vector2(move * speed, rb.velocity.y); // Set velocity for running
                anim.Play("Run_player"); // Play run animation
            }
            else
            {
                speed = walkSpeed; // Set speed to walkSpeed for walking
                rb.velocity = new Vector2(move * speed, rb.velocity.y); // Set velocity for walking
                anim.Play("Walk_player"); // Play walk animation
            }

            // Flip the sprite depending on movement direction
            spriteRenderer.flipX = move < 0; // Flip left or right based on movement direction
        }
        else
        {
            rb.velocity = Vector2.zero; // Stop the player when there's no input
            anim.Play("idle_player"); // Play wake animation if stationary
        }

        // Set the speed parameter in the animator
        //anim.SetFloat("speed", speed);
    }

    void HandleShooting()
    {
        if (isDashing) return;

        // Normal Shoot
        if (Input.GetButtonDown("Fire1") && !isShooting )
        {
            Debug.Log("hello in shoot");
            isShooting = true;
            anim.SetTrigger("TriggerShoot");
            anim.Play("Shoot_player");
            currentBullets -= 1; // Consume 1 bullet
            StartCoroutine(ShootCycle());
        }
        // Charge and Strong Shoot
        else if (Input.GetButton("Fire1") && !isCharging && !isShooting )
        {
            Debug.Log("hello in charge");
            anim.SetTrigger("TriggerCharge");
            anim.Play("Charge_player");
            chargeTimer += Time.deltaTime;
            if (chargeTimer >= chargeTime)
            {
                isCharging = true;
            }
        }
        else if (Input.GetButtonUp("Fire1") && isCharging)
        {
            if (true)
            {
                Debug.Log("hello in ss");
                anim.SetTrigger("TriggerStrongShoot");
                anim.Play("Strong_Shoot_player");
                currentBullets -= 5; // Consume 5 bullets
                StartCoroutine(StrongShootCycle());
            }
            isCharging = false;
            chargeTimer = 0f;
        }
    }

    IEnumerator ShootCycle()
    {
        yield return new WaitForSeconds(anim.GetCurrentAnimatorStateInfo(0).length); // Wait for shoot animation to finish
        isShooting = false;

        // Hold the last frame of the shooting animation
        anim.Play("Shoot_player");
    }

    IEnumerator StrongShootCycle()
    {
        yield return new WaitForSeconds(anim.GetCurrentAnimatorStateInfo(0).length); // Wait for strong shoot animation to finish
        isShooting = false;

        // Hold the last frame of the strong shoot animation
        anim.Play("Strong_Shoot_player");
    }

    void HandleDash()
    {
        anim.Play("Dash_player");
        isDashing = true;
            anim.SetTrigger("TriggerDash");
            StartCoroutine(DashCooldown());
    }

    IEnumerator DashCooldown()
    {
        yield return new WaitForSeconds(anim.GetCurrentAnimatorStateInfo(0).length); // Wait for dash animation to finish
        isDashing = false;
        dashCooldownTimer = dashCooldown;

        // Hold the last frame of the dash animation
        anim.Play("Dash_player");
    }

    void HandleDamage()
    {
        // Placeholder for damage logic
        // Call TakeDamage() when damage is taken
    }

    public void TakeDamage(int damageAmount) // Call this method when taking damage
    {
        if (isDead) return;

        currentHealth -= damageAmount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        anim.SetTrigger("TriggerDamage");

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public void Die() // Call this method when the player dies
    {
        if (isDead) return;

        isDead = true;
        anim.SetTrigger("TriggerDeath");
        // Disable movement and other controls after death
        this.enabled = false;
    }

    
}
