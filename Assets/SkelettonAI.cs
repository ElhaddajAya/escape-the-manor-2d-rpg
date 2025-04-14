public class SkeletonAI : EnemyAIBase
{
    protected override void Start()
    {
        base.Start();
        speed = 5f;
        chaseRange = 4f;
        attackRange = 1f;
        attackCooldown = 2f;
        maxHealth = 5; // Skeletons die in 5 hits
        damageForce = 5; 
    }

    protected override void Attack()
    {
        base.Attack();
    }

}