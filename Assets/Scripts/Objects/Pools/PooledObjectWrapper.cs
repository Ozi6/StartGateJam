using UnityEngine;

public class PooledObjectWrapper
{
    public GameObject gameObject;
    public IPooledObject pooledObject;
    public PooledObjectWrapper(GameObject go)
    {
        gameObject = go;
        pooledObject = go.GetComponent<IPooledObject>();
    }
}