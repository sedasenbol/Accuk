using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectSpawned : MonoBehaviour
{
    ObjectPooler objectPooler;
    private Transform playerTransform;
    private Transform xform;
    private const float SPAWN_OFFSET = 5f;

    private void OnStayedBehindThePlayer()
    {
        if (playerTransform.position.z < xform.position.z + SPAWN_OFFSET) { return; }
        objectPooler.DeactivateSpawnedObject(this.gameObject);        
    }

    private void Start()
    {
        xform = GetComponent<Transform>();
        playerTransform = GameObject.Find("Player").transform;

        objectPooler = ObjectPooler.Instance;
    }

    private void Update()
    {
        OnStayedBehindThePlayer();
    }
}
