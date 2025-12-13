using UnityEngine;

public class Scout_lvl2 : Person
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
        ObjectPooler.Instance.ReturnToPool(gameObject, "Scout_lvl2");
        base.Die();
    }
}