public class SkeletonAI : EnemyAIBase
{
    protected override void Start()
    {
        base.Start();
        speed = 3f;
        chaseRange = 4f;
        attackRange = 1.2f;
        maxHealth = 5; // Skeletons die in 5 hits
    }

    protected override void Die()
    {
        base.Die();
        // Add any skeleton-specific death behavior here
        
    }
}