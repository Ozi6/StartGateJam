using UnityEngine;

public class Archer : Person
{
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private float projectileSpeed = 10f;

    protected override void Start()
    {
        // Initialization if needed
    }

    protected override void OnDestroy()
    {
        // Cleanup if needed
    }

    protected override void Attack()
    {
        if (animator != null)
            animator.SetBool("Attacking", true);
        if (TargetEntity != null && projectilePrefab != null)
        {
            GameObject proj = Instantiate(projectilePrefab, transform.position + Vector3.up * 1f, Quaternion.identity);
            Projectile projectileScript = proj.GetComponent<Projectile>();
            if (projectileScript != null)
            {
                float a = CalculateDamage();
                projectileScript.SetTarget(TargetEntity, a, isFriendly, projectileSpeed);
                if (TargetEntity.health <= a)
                    animator.SetBool("Attacking", false);
            }
        }
        AttackSound();
    }

    protected override void AttackSound()
    {
        AudioManager.Instance.PlaySFXAtPoint("bow", gameObject.transform.position);
    }

    protected override void Die()
    {
        ObjectPooler.Instance.ReturnToPool(gameObject, "Archer");
        base.Die();
    }
}