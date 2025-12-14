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

    protected override void AttackSound()
    {
        int roll = Random.Range(0, 1); // 0, 1

        switch (roll)
        {
            case 0:
                AudioManager.Instance.PlaySFXAtPoint("Horse Attack1", transform.position);
                break;
            case 1:
                AudioManager.Instance.PlaySFXAtPoint("Horse Attack 2", transform.position);
                break;
        }
    }

    protected override void PlayWalkSFX()
    {
        AudioManager.Instance.PlaySFXAtPoint("horse", gameObject.transform.position);
    }
}