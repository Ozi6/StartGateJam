using UnityEngine;

public class Scout : Person
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
        ObjectPooler.Instance.ReturnToPool(gameObject, "Scout");
        base.Die();
    }

    protected override void AttackSound()
    {
        AudioManager.Instance.PlaySFXAtPoint("sword", gameObject.transform.position);
    }

    protected override void PlayWalkSFX()
    {
        AudioManager.Instance.PlaySFXAtPoint("horse", gameObject.transform.position);
    }
}