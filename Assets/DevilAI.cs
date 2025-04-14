using System.Collections;
using UnityEngine;

public class DevilAI : EnemyAIBase
{
    public GameObject firePrefab; // Assigné dans l'inspecteur
    public float retreatRange = 2f;
    public float fireSpeed = 7f;
    public Transform firePoint; // Référence au point où le feu doit sortir

    protected override void Start()
    {
        base.Start();
        speed = 5.5f;
        chaseRange = 7f;
        attackRange = 1f;
        attackCooldown = 4f;
        maxHealth = 10; 
        // damageForce = 10; // No need to set it here because the Fire Projectile will handle it
    }

    protected override void Update()
    {
        base.Update();

        float playerDistance = Vector2.Distance(transform.position, player.position);

        if (playerDistance < retreatRange)
        {
            Retreat();
        }
    }

    protected override void Attack()
    {
        base.Attack();

        // Lancer le feu immédiatement après une courte pause pour que l'animation d'attaque se déclenche
        StartCoroutine(FireAttack());

        StartCoroutine(ResumeAfterAttack());
    }

    IEnumerator FireAttack()
    {
        while (isAttacking)
        {
            // Attendre que l'animation d'attaque commence (ajuster ce délai selon ton animation)
            yield return new WaitForSeconds(0.1f); // Attendre un peu pour que l'animation se lance

            // Lancer le feu à la position des bras (firePoint)
            if (firePrefab != null && firePoint != null)
            {
                GameObject fireball = Instantiate(firePrefab, firePoint.position, Quaternion.identity); // Utiliser firePoint
                FireProjectile fireScript = fireball.GetComponent<FireProjectile>();

                if (fireScript != null)
                {
                    // Calculer la direction du feu par rapport à l'ennemi
                    Vector2 direction = (player.position - firePoint.position).normalized; // Utiliser firePoint
                    fireScript.Launch(direction, fireSpeed);
                }
            }

            yield return new WaitForSeconds(0.3f); // Attendre un peu avant de lancer à nouveau
        }
    }
    
    void Retreat()
    {
        Vector2 retreatDirection = ((Vector2)transform.position - (Vector2)player.position).normalized;
        rb.velocity = retreatDirection * (speed * 1.5f);
        Debug.Log("Devil se retire stratégiquement !");
    }
}
