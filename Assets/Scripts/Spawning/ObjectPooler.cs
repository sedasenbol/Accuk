using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class ObjectPooler : MonoBehaviour
{
    [Serializable]
    public class Pool
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
        street,
        grass,
        streetLamp,
        bench,
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
            zSizes.Add(pool.tag, pool.prefab.GetComponent<Renderer>().bounds.size.z);
        }
    }

    public void SpawnFromPool(objectTag tag, Vector3 position, Quaternion rotation)
    {
        GameObject objectToSpawn = poolDictionary[tag].Dequeue();

        objectToSpawn.SetActive(true);
        objectToSpawn.transform.position = position;
        objectToSpawn.transform.rotation = rotation;

        poolDictionary[tag].Enqueue(objectToSpawn);
    }
}
