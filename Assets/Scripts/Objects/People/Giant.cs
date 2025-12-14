using UnityEngine;

public class Giant : Person
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
        ObjectPooler.Instance.ReturnToPool(gameObject, "Giant");
        base.Die();
    }

    protected override void AttackSound()
    {
        AudioManager.Instance.PlaySFXAtPoint("axe_hit", gameObject.transform.position);
    }

    protected override void PlayWalkSFX()
    {
        AudioManager.Instance.PlaySFXAtPoint("giant_step", gameObject.transform.position);  
    }
}