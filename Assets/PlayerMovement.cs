using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// This script handles player movement and animation in a 2D game.
public class PlayerMovement : MonoBehaviour
{
    // Speed of the player's movement
    public float moveSpeed = 4f;

    // Reference to the Rigidbody2D component for physics-based movement
    private Rigidbody2D rb;

    // Stores the player's movement direction
    private Vector2 movement;

    // Reference to the Animator component to handle animations
    private Animator animator;

    // Indicates if the player is facing right
    private bool facingRight = true;

    // Called once at the start of the game
    void Start()
    {
        // Get the Rigidbody2D component attached to the player
        rb = GetComponent<Rigidbody2D>();

        // Get the Animator component attached to the player
        animator = GetComponentInChildren<Animator>();

        // Check if the Animator component is missing and log an error if it is
        if (animator == null)
        {
            Debug.LogError("Animator component is missing on the Player object.");
        }

        // Prevent the player from rotating when colliding with objects
        rb.freezeRotation = true;

        // Ensure the character starts in Idle state
        animator.SetBool("isMoving", false);
    }

    // Called every frame to handle input
    void Update()
    {
        // Get movement input from arrow keys or WASD
        movement.x = Input.GetAxisRaw("Horizontal"); // Left (-1) / Right (+1)
        movement.y = Input.GetAxisRaw("Vertical");   // Down (-1) / Up (+1)

        // Check if the player is moving
        bool isMoving = movement.sqrMagnitude > 0.01f;
        animator.SetBool("isMoving", isMoving);

        // Update animation parameters for movement direction
        if (isMoving)
        {
            animator.SetFloat("MoveX", movement.x);
            animator.SetFloat("MoveY", movement.y);

            // Flip character sprite based on movement direction
            if (movement.x > 0 && facingRight)
            {
                Flip();
            }
            else if (movement.x < 0 && !facingRight)
            {
                Flip();
            }
        }
    }

    // Called at a fixed interval for physics-based movement
    void FixedUpdate()
    {
        // If the player is moving, apply velocity
        if (animator.GetBool("isMoving"))
        {
            rb.velocity = movement * moveSpeed;
        }
        else
        {
            rb.velocity = Vector2.zero; // Stop movement if idle
        }
    }

    // Function to flip the character's direction
    void Flip()
    {
        facingRight = !facingRight;
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }
}
