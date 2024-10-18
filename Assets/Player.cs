using System.Collections;
using UnityEngine;

public class PlayerController : MonoBehaviour
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

    // UI (Optional)
    // public Text healthText;
    // public Text bulletsText;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();  // Get SpriteRenderer component

        // Initialize Health and Bullets
        currentHealth = maxHealth;
        currentBullets = maxBullets;

        // Update UI (Optional)
        // UpdateHealthUI();
        // UpdateBulletsUI();

        // Player starts in Sleep state
        anim.Play("Sleep_player");
        StartCoroutine(StartWakePlayer());
    }

    void Update()
    {
        if (!isAwake) return; // No operations until wake animation completes

        HandleMovement();
        HandleShooting();
        HandleDash();
        HandleDamage();

        // Reset dash cooldown
        if (dashCooldownTimer > 0) dashCooldownTimer -= Time.deltaTime;
    }

    // Coroutine to handle waking up after 1 second
    IEnumerator StartWakePlayer()
    {
        yield return new WaitForSeconds(1f);
        anim.SetTrigger("Wake_player");
        yield return new WaitForSeconds(anim.GetCurrentAnimatorStateInfo(0).length); // Wait for wake animation to finish
        isAwake = true; // Player can now move and shoot
    }

    void HandleMovement()
    {
        float move = Input.GetAxis("Horizontal");
        float speed = Mathf.Abs(move) > 0.1f ? (Input.GetKey(KeyCode.LeftShift) ? runSpeed : walkSpeed) : 0f;

        // Set movement speed in animator
        anim.SetFloat("speed", speed);

        if (speed > 0)
        {
            rb.velocity = new Vector2(move * speed, rb.velocity.y);

            // Flip the sprite depending on movement direction
            if (move < 0)
                spriteRenderer.flipX = true;  // Flip to face left
            else if (move > 0)
                spriteRenderer.flipX = false; // Face right

            // Handle running/walking animations
            if (speed == runSpeed)
                anim.Play("Run_player");
            else
                anim.Play("Walk_player");
        }
        else
        {
            rb.velocity = Vector2.zero;
            anim.Play("Sleep_player");
        }
    }

    void HandleShooting()
    {
        if (isDashing) return; // Dash overrides all animations

        // Normal Shoot
        if (Input.GetButtonDown("Fire1") && !isShooting && currentBullets >= 1)
        {
            isShooting = true;
            anim.SetTrigger("Shoot_player");
            currentBullets -= 1; // Consume 1 bullet
            // UpdateBulletsUI(); // Update UI if implemented
            StartCoroutine(ShootCycle());
        }
        // Charge and Strong Shoot
        else if (Input.GetButton("Fire1") && !isCharging && !isShooting && currentBullets >= 5)
        {
            chargeTimer += Time.deltaTime;
            if (chargeTimer >= chargeTime)
            {
                isCharging = true;
                anim.SetTrigger("Charge_player");
            }
        }
        else if (Input.GetButtonUp("Fire1") && isCharging)
        {
            if (currentBullets >= 5)
            {
                anim.SetTrigger("Strong_Shoot_player");
                currentBullets -= 5; // Consume 5 bullets
                // UpdateBulletsUI(); // Update UI if implemented
                StartCoroutine(StrongShootCycle());
            }
            isCharging = false;
            chargeTimer = 0f;
        }
    }

    // Shoot animation cycle to ensure completion
    IEnumerator ShootCycle()
    {
        yield return new WaitForSeconds(anim.GetCurrentAnimatorStateInfo(0).length); // Wait for shoot animation to finish
        isShooting = false;
        // Here you can implement the logic to connect to the enemy after the animation completes
        // e.g., Instantiate a bullet prefab, raycast, etc.
    }

    // Strong Shoot animation cycle to ensure completion
    IEnumerator StrongShootCycle()
    {
        yield return new WaitForSeconds(anim.GetCurrentAnimatorStateInfo(0).length); // Wait for strong shoot animation to finish
        // Implement the logic to connect to the enemy after the strong shoot animation completes
        isShooting = false;
    }

    void HandleDash()
    {
        if (Input.GetButtonDown("Fire2") && dashCooldownTimer <= 0 && !isDashing)
        {
            isDashing = true;
            anim.SetTrigger("Dash_player");
            StartCoroutine(DashCooldown());
        }
    }

    // Dash cycle and cooldown
    IEnumerator DashCooldown()
    {
        yield return new WaitForSeconds(anim.GetCurrentAnimatorStateInfo(0).length); // Wait for dash animation to finish
        isDashing = false;
        dashCooldownTimer = dashCooldown;
    }

    void HandleDamage()
    {
        // Placeholder for damage logic
        // If player takes damage, trigger damage animation
        // Example: Detect collision with enemy/projectile and call TakeDamage()
    }

    public void TakeDamage(int damageAmount) // Call this method when taking damage
    {
        if (isDead) return;

        currentHealth -= damageAmount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        // UpdateHealthUI(); // Update UI if implemented

        anim.SetTrigger("damage_player");

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private bool isDead = false;

    public void Die() // Call this method when the player dies
    {
        if (isDead) return;

        isDead = true;
        anim.SetTrigger("Death_player");
        // Disable movement and other controls after death
        this.enabled = false;
    }

    // Optional: UI Update Methods
    /*
    void UpdateHealthUI()
    {
        if (healthText != null)
        {
            healthText.text = "Health: " + currentHealth;
        }
    }

    void UpdateBulletsUI()
    {
        if (bulletsText != null)
        {
            bulletsText.text = "Bullets: " + currentBullets;
        }
    }
    */

    // Optional: Reloading Bullets (Not requested but useful)
    /*
    void ReloadBullets()
    {
        currentBullets = maxBullets;
        // UpdateBulletsUI(); // Update UI if implemented
    }
    */
}
