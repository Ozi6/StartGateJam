public class Giant_lvl2 : Person
{
    protected override void Start()
    {
        // Initialization if needed
    }

    protected override void OnDestroy()
    {
        // Cleanup if needed
    }
    protected override void Die()
    {
        ObjectPooler.Instance.ReturnToPool(gameObject, "Giant_lvl2");
        base.Die();
    }
}