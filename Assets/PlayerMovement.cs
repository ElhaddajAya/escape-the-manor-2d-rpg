using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 3f;
    private Rigidbody2D rb;
    private Vector2 movement;
    private Animator animator;
    private bool facingRight = true; // Indique si le personnage fait face à la droite

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();

        if (animator == null)
        {
            Debug.LogError("Animator component is missing on the Player object.");
        }

        rb.freezeRotation = true;

        // Assurer que le personnage démarre en Idle
        animator.SetBool("isMoving", false);
    }

    void Update()
    {
        movement.x = Input.GetAxisRaw("Horizontal"); 
        movement.y = Input.GetAxisRaw("Vertical");

        // Vérifier si le joueur est en mouvement
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
