using System.Collections;
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

    protected override IEnumerator PerformAttack()
    {
        isAttacking = true;
        if (animator != null)
            animator.SetBool("Attacking", true);
        // Wait for half the duration to simulate wind-up before launching projectile
        yield return new WaitForSeconds(attackDuration / 2f);
        if (TargetEntity != null && projectilePrefab != null)
        {
            GameObject proj = Instantiate(projectilePrefab, transform.position + Vector3.up * 1f, Quaternion.identity);
            Projectile projectileScript = proj.GetComponent<Projectile>();
            if (projectileScript != null)
            {
                float a = CalculateDamage();
                projectileScript.SetTarget(TargetEntity, a, isFriendly, projectileSpeed);
            }
        }
        AttackSound();
        // Wait for the remaining duration to complete the attack
        yield return new WaitForSeconds(attackDuration / 2f);
        if (animator != null)
            animator.SetBool("Attacking", false);
        isAttacking = false;
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