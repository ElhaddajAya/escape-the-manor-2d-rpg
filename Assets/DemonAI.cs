using UnityEngine;

public class DemonAI : EnemyAIBase
{
    protected override void Start()
    {
        base.Start();
        speed = 4.5f;
        chaseRange = 6f;
        attackRange = 3f;
    }

    protected override void Attack()
    {
        base.Attack();

        PlayerObj playerObj = player.GetComponent<PlayerObj>();
        if (playerObj != null)
        {
            playerObj.TakeDamage();
        }

        Debug.Log("Démon utilise une attaque plus puissante !");
    }
}
