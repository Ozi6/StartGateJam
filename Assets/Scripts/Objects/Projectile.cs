using UnityEngine;

public class Projectile : MonoBehaviour
{
    private Person target;
    private float damage;
    private bool isFriendly;
    private float speed;

    public void SetTarget(Person targetEntity, float dmg, bool friendly, float projSpeed)
    {
        target = targetEntity;
        damage = dmg;
        isFriendly = friendly;
        speed = projSpeed;
    }

    private void Update()
    {
        if (target == null || target.Health <= 0)
        {
            Destroy(gameObject);
            return;
        }

        transform.position = Vector3.MoveTowards(transform.position, target.transform.position, speed * Time.deltaTime);

        if (Vector3.Distance(transform.position, target.transform.position) < 0.1f)
        {
            target.TakeDamage(damage);
            Destroy(gameObject);
        }
    }
}