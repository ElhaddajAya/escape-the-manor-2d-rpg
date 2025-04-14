using UnityEngine;

public class DemonAI : EnemyAIBase
{
    protected override void Start()
    {
        base.Start();
        speed = 5f;
        chaseRange = 5f;
        attackRange = 1f;
        attackCooldown = 2f;
        maxHealth = 7; // Le démon meurt en 7 coups
    }

    protected override void Attack()
    {
        base.Attack();

        Debug.Log("Démon utilise une attaque plus puissante !");
    }
}
