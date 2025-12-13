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
        if (TargetEntity != null && projectilePrefab != null)
        {
            GameObject proj = Instantiate(projectilePrefab, transform.position + Vector3.up * 1f, Quaternion.identity);
            Projectile projectileScript = proj.GetComponent<Projectile>();
            if (projectileScript != null)
            {
                projectileScript.SetTarget(TargetEntity, CalculateDamage(), isFriendly, projectileSpeed);
            }
        }
    }
    protected override void Die()
    {
        ObjectPooler.Instance.ReturnToPool(gameObject, "Archer");
        base.Die();
    }
}