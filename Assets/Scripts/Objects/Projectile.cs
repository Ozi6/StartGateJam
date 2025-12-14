using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    private Person target;
    private float damage;
    private bool isFriendly;
    private float speed;

    [SerializeField] private List<GameObject> impactVFXPrefabs;

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

        Vector3 direction = target.transform.position - transform.position;
        transform.up = direction;

        transform.position = Vector3.MoveTowards(
            transform.position,
            target.transform.position,
            speed * Time.deltaTime
        );

        if (direction.sqrMagnitude < 0.01f)
        {
            Impact();
        }
    }
    private void Impact()
    {
        target.TakeDamage(damage);

        if (impactVFXPrefabs != null && impactVFXPrefabs.Count > 0)
        {
            GameObject prefab = impactVFXPrefabs[Random.Range(0, impactVFXPrefabs.Count)];
            GameObject vfxGO = Instantiate(
                prefab,
                transform.position,
                Quaternion.LookRotation(-transform.forward)
            );

            ParticleSystem ps = vfxGO.GetComponent<ParticleSystem>();
            Destroy(vfxGO, ps.main.duration + ps.main.startLifetime.constantMax);
        }
        Destroy(gameObject);
    }
}
