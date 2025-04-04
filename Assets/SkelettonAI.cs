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

    protected override void Attack()
    {
        base.Attack();

        // Lorseque le Player est touchée par l'attaque du démon, activer le damage
        PlayerObj playerObj = player.GetComponent<PlayerObj>();
        if (playerObj != null)
        {
            playerObj.TakeDamage();
        }
    }

}