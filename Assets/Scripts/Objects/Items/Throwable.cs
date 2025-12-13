using UnityEngine;

public class Throwable : MonoBehaviour
{
    [Header("Area Effect")]
    [SerializeField] private float effectRadius = 5f;
    [SerializeField] private LayerMask personLayer ;

    public PowerUpType powerUpType;

    public virtual void OnThrown()
    {
    }

    private void OnCollisionEnter(Collision collision)
    {
        OnImpact(collision);
        Destroy(gameObject);
    }

    protected virtual void OnImpact(Collision collision)
    {

            Vector3 impactPoint = collision.contacts[0].point;
            Debug.Log($"Impact point: {impactPoint}");

            ApplyAreaPowerUp(impactPoint);
    }


    // ---------------- AREA POWER-UP ----------------

    private void ApplyAreaPowerUp(Vector3 center)
    {
        Debug.Log($"Applying {powerUpType} power-up at {center}");

        Collider[] hits = Physics.OverlapSphere(center, effectRadius);

        foreach (Collider hit in hits)
        {
            // This works even if the collider is on a child
            Person person = hit.GetComponentInParent<Person>();
            if (person == null) continue;

            Debug.Log($"PowerUp hit {person.name}");

            PowerUpEffectProcessor.Apply(powerUpType, person);
        }
    }



    // ---------------- DEBUG ----------------

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, effectRadius);
    }
}
