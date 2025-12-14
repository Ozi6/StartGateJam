using UnityEngine;

public class Throwable : MonoBehaviour
{
    [Header("Area Effect")]
    [SerializeField] private float effectRadius = 5f;
    [SerializeField] private LayerMask personLayer;

    [Header("Mid-Air Rotation")]
    [SerializeField] private Vector3 rotationSpeed = new Vector3(0f, 720f, 0f);

    public PowerUpType powerUpType;

    private bool isThrown;

    public virtual void OnThrown()
    {
        isThrown = true;
    }

    private void Update()
    {
        if (!isThrown) return;

        transform.Rotate(rotationSpeed * Time.deltaTime, Space.Self);
    }

    private void OnCollisionEnter(Collision collision)
    {
        isThrown = false;
        OnImpact(collision);
        Destroy(gameObject);
    }

    protected virtual void OnImpact(Collision collision)
    {
        Vector3 impactPoint = collision.contacts[0].point;
        ApplyAreaPowerUp(impactPoint);
    }

    private void ApplyAreaPowerUp(Vector3 center)
    {
        Collider[] hits = Physics.OverlapSphere(center, effectRadius);

        foreach (Collider hit in hits)
        {
            Person person = hit.GetComponentInParent<Person>();
            if (person == null) continue;

            PowerUpEffectProcessor.Apply(powerUpType, person);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, effectRadius);
    }
}
