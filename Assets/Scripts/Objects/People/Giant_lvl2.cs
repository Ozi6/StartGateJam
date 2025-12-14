using UnityEngine;

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

    protected override void AttackSound()
    {
        int roll = Random.Range(0, 3); // 0, 1, 2 or 3

        switch (roll)
        {
            case 0:
                AudioManager.Instance.PlaySFXAtPoint("Giant Attack 1", transform.position);
                break;
            case 1:
                AudioManager.Instance.PlaySFXAtPoint("Barbarian Attack 2", transform.position);
                break;
            case 2:
                AudioManager.Instance.PlaySFXAtPoint("Barbarian Attack 3", transform.position);
                break;
            case 3:
                AudioManager.Instance.PlaySFXAtPoint("Barbarian Attack 4", transform.position);
                break;
        }
    }

    protected override void PlayWalkSFX()
    {
        AudioManager.Instance.PlaySFXAtPoint("giant_step", gameObject.transform.position);
    }
}