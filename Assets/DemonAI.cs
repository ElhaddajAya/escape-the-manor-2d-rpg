using UnityEngine;

public class DemonAI : EnemyAIBase
{
    protected override void Start()
    {
        base.Start();
        speed = 4.5f;
        chaseRange = 6f;
        attackRange = 1.5f;
    }

    protected override void Attack()
    {
        base.Attack();
        Debug.Log("Démon utilise une attaque plus puissante !");
    }
}
