using System.Collections;
using UnityEngine;

public class DevilAI : EnemyAIBase
{
    public GameObject firePrefab; // Assigné dans l'inspecteur
    public float retreatRange = 2f;
    public float fireSpeed = 7f;

    protected override void Start()
    {
        base.Start();
        speed = 5.5f;
        chaseRange = 7f;
        attackRange = 1.8f;
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
        if (isAttacking) return;

        isAttacking = true;
        rb.velocity = Vector2.zero;
        animator.SetTrigger("2_Attack");
        animator.SetBool("1_Move", false);
        lastAttackTime = Time.time;

        // Lancer le feu après un court délai pour correspondre à l'animation
        StartCoroutine(FireAttack());

        StartCoroutine(ResumeAfterAttack());
    }

    IEnumerator FireAttack()
    {
        yield return new WaitForSeconds(0.2f); // Synchroniser avec l'animation

        if (firePrefab != null)
        {
            GameObject fireball = Instantiate(firePrefab, transform.position, Quaternion.identity);
            FireProjectile fireScript = fireball.GetComponent<FireProjectile>();

            if (fireScript != null)
            {
                Vector2 direction = ((Vector2)player.position - (Vector2)transform.position).normalized;
                fireScript.Launch(direction, fireSpeed);
            }
        }

    }
    
    void Retreat()
    {
        Vector2 retreatDirection = ((Vector2)transform.position - (Vector2)player.position).normalized;
        rb.velocity = retreatDirection * (speed * 1.5f);
        Debug.Log("Devil se retire stratégiquement !");
    }
}
