using UnityEngine;

public class Throwable : MonoBehaviour
{
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
        int groundLayer = LayerMask.NameToLayer("Ground");
        if (collision.gameObject.layer == groundLayer)
        {
            Debug.Log("Throwable hit Ground layer.");
        }
        else
        {
            Debug.Log("Throwable hit other layer: " + LayerMask.LayerToName(collision.gameObject.layer));
        }
    }
}