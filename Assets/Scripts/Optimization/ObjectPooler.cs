using UnityEngine;
using System.Collections.Generic;

public class ObjectPooler : Singleton<ObjectPooler>
{
    [Header("Pools")]
    [SerializeField] private List<Pool> pools = new List<Pool>();
    private Dictionary<string, Queue<PooledObjectWrapper>> poolDictionary;
    private Dictionary<string, Pool> poolDefinitions;

    protected override void OnAwake()
    {
        poolDictionary = new Dictionary<string, Queue<PooledObjectWrapper>>();
        poolDefinitions = new Dictionary<string, Pool>();

        foreach (Pool pool in pools)
        {
            poolDefinitions.Add(pool.tag, pool);

            Queue<PooledObjectWrapper> objectPool = new Queue<PooledObjectWrapper>();
            for (int i = 0; i < pool.size; i++)
            {
                GameObject obj = Instantiate(pool.prefab);
                obj.SetActive(false);
                obj.transform.SetParent(transform);
                objectPool.Enqueue(new PooledObjectWrapper(obj));
            }
            poolDictionary.Add(pool.tag, objectPool);
        }
    }

    private PooledObjectWrapper CreateNewPooledObject(string tag)
    {
        if (poolDefinitions.TryGetValue(tag, out Pool poolDefinition))
        {
            GameObject obj = Instantiate(poolDefinition.prefab);
            obj.transform.SetParent(transform);
            obj.SetActive(false);
            return new PooledObjectWrapper(obj);
        }
        else
            return null;
    }



    public GameObject SpawnFromPool(string tag, Vector3 position, Quaternion rotation)
    {
        if (!poolDictionary.ContainsKey(tag))
        {
            Debug.LogWarning($"Pool with tag '{tag}' doesn't exist!");
            return null;
        }
        Queue<PooledObjectWrapper> objectQueue = poolDictionary[tag];
        PooledObjectWrapper wrapper;
        if (objectQueue.Count == 0)
        {
            wrapper = CreateNewPooledObject(tag);
            if (wrapper == null)
                return null;
        }
        else
            wrapper = objectQueue.Dequeue();
        wrapper.gameObject.SetActive(true);
        wrapper.gameObject.transform.position = position;
        wrapper.gameObject.transform.rotation = rotation;
        wrapper.pooledObject?.OnObjectSpawn();
        return wrapper.gameObject;
    }

    public GameObject SpawnFromPool(string tag, Vector3 position)
    {
        return SpawnFromPool(tag, position, Quaternion.identity);
    }

    public void ReturnToPool(GameObject obj, string tag)
    {
        if (!poolDictionary.ContainsKey(tag))
        {
            Debug.LogWarning($"Cannot return object. Pool with tag '{tag}' doesn't exist!");
            return;
        }
        if (obj.activeSelf)
        {
            PooledObjectWrapper wrapper = new PooledObjectWrapper(obj);
            obj.SetActive(false);
            poolDictionary[tag].Enqueue(wrapper);
        }
    }
}