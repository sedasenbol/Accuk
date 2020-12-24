using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

public class ObjectPooler : MonoBehaviour
{
    [Serializable] public class Pool
    {
        public objectTag tag;
        public GameObject prefab;
        public Transform container;
        public int poolSize;
    }

    public enum objectTag
    {
        building1,
        building2,
        building3,
        building4,
        bus,
        movingBus,
        busWRamp,
        stopSign,
        warningStand,
        lowWarningStand,
        coin,
        redMagnetPowerUp,
        greenHighJumpPowerUp,
        blueFlyingPowerUp,
        empty,
    }

    public List<Pool> pools;

    public Dictionary<objectTag, Queue<GameObject>> poolDictionary;

    public Dictionary<objectTag, float> zSizes;

    public static ObjectPooler Instance;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        poolDictionary = new Dictionary<objectTag, Queue<GameObject>>();

        zSizes = new Dictionary<objectTag, float>();

        foreach (Pool pool in pools)
        {
            Queue<GameObject> objectPool = new Queue<GameObject>();

            for (int i = 0; i < pool.poolSize; i++)
            {
                GameObject obj = Instantiate(pool.prefab, pool.container);
                obj.SetActive(false);
                objectPool.Enqueue(obj);
            }

            poolDictionary.Add(pool.tag, objectPool);

            if (pool.prefab.GetComponent<Renderer>())
            {
                zSizes.Add(pool.tag, pool.prefab.GetComponent<Renderer>().bounds.size.z);
            }
            else if (pool.tag == objectTag.busWRamp)
            {
                float totalZLength = pool.prefab.transform.GetChild(0).GetChild(0).GetComponent<Renderer>().bounds.size.z + pool.prefab.transform.GetChild(1).GetChild(0).GetChild(0).GetComponent<Renderer>().bounds.size.z;
                zSizes.Add(pool.tag, totalZLength);
            }
            else
            {
                List<float> rendererZLengths = new List<float>();
                foreach (Renderer renderer in pool.prefab.GetComponentsInChildren<Renderer>())
                {
                    rendererZLengths.Add(renderer.bounds.size.z);
                }
                zSizes.Add(pool.tag, rendererZLengths.Max());
            }
        }
    }

    private GameObject ReturnInactiveObject(objectTag tag)
    {
        if (poolDictionary[tag].Peek().activeSelf)
        {
            Debug.LogError($"{tag} size should be increased.");
            poolDictionary[tag].Enqueue(poolDictionary[tag].Dequeue());
            return ReturnInactiveObject(tag);
        }
        return poolDictionary[tag].Dequeue();
    }

    public void SpawnFromPool(objectTag tag, Vector3 position, Quaternion rotation, string name = null)
    {

        GameObject objectToSpawn = ReturnInactiveObject(tag);
        objectToSpawn.SetActive(true);
        objectToSpawn.transform.position = position;
        objectToSpawn.transform.rotation = rotation;
        if (name != null) 
        {
            objectToSpawn.name = name; 
        }

        poolDictionary[tag].Enqueue(objectToSpawn);
    }

    public void DeactivateSpawnedObject(GameObject gameObject)
    {
        gameObject.SetActive(false);
    }
}
