using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// This script handles player movement and animation in a 2D game.
public class PlayerMovement : MonoBehaviour
{
    // Speed of the player's movement
    public float moveSpeed = 3f;

    // Reference to the Rigidbody2D component for physics-based movement
    private Rigidbody2D rb;

    // Stores the player's movement direction
    private Vector2 movement;

    // Reference to the Animator component to handle animations
    private Animator animator;
    private bool facingRight = true; // Indique si le personnage fait face à la droite

    // Called once at the start of the game
    void Start()
    {
        // Get the Rigidbody2D component attached to the player
        rb = GetComponent<Rigidbody2D>();

        // Get the Animator component attached to the player
        animator = GetComponent<Animator>();

        // Check if the Animator component is missing and log an error if it is
        if (animator == null)
        {
            Debug.LogError("Animator component is missing on the Player object.");
        }

        // Prevent the player from rotating when colliding with objects
        rb.freezeRotation = true;

        // Assurer que le personnage démarre en Idle
        animator.SetBool("isMoving", false);
    }

    // Called every frame to handle input
    void Update()
    {
        // Get movement input from arrow keys or WASD
        movement.x = Input.GetAxis("Horizontal"); // Left (-1) / Right (+1)
        movement.y = Input.GetAxis("Vertical");   // Down (-1) / Up (+1)

        // If the Animator component exists, update animations
        if (animator != null)
        {
            // Check if the player is moving
            bool isMoving = movement.sqrMagnitude > 0.01f;
            animator.SetBool("isMoving", isMoving);

        // Si le joueur bouge, on met à jour les valeurs MoveX et MoveY
        if (isMoving)
        {
            animator.SetFloat("MoveX", movement.x);
            animator.SetFloat("MoveY", movement.y);

            // Gérer l'inversion du sprite (flip)
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
        if (animator.GetBool("isMoving"))
        {
            rb.velocity = movement * moveSpeed;
        }
        else
        {
            rb.velocity = Vector2.zero; // S'assurer que le personnage ne bouge pas en Idle
        }
    }

    // Fonction pour inverser le personnage horizontalement
    void Flip()
    {
        facingRight = !facingRight;
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }
}
