using UnityEngine;

public class DevilAI : EnemyAIBase
{
    public float retreatRange = 2f;

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

    void Retreat()
    {
        Vector2 retreatDirection = ((Vector2)transform.position - (Vector2)player.position).normalized;
        rb.velocity = retreatDirection * (speed * 1.5f);
        Debug.Log("Devil se retire stratégiquement !");
    }
}
