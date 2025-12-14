using UnityEngine;

public class Viking : Person
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
        ObjectPooler.Instance.ReturnToPool(gameObject, "Viking");
        base.Die();
    }

    protected override void AttackSound()
    {
        AudioManager.Instance.PlaySFXAtPoint("axe_hit", gameObject.transform.position);
    }

    protected override void PlayWalkSFX()
    {
        //AudioManager.Instance.PlaySFXAtPoint()
    }
}