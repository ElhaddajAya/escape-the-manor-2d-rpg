public class SkeletonAI : EnemyAIBase
{
    protected override void Start()
    {
        base.Start();
        speed = 3f;
        chaseRange = 4f;
        attackRange = 1.2f;
    }
}
