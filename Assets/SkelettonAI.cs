public class SkeletonAI : EnemyAIBase
{
    protected override void Start()
    {
        base.Start();
        speed = 3f;
        chaseRange = 4f;
        attackRange = 1.2f;
    }

    protected override void Attack()
    {
        base.Attack();

        PlayerObj playerObj = player.GetComponent<PlayerObj>();
        if (playerObj != null)
        {
            playerObj.TakeDamage();
        }
    }
}
