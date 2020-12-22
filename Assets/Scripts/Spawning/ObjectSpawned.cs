using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectSpawned : MonoBehaviour
{
    private Transform playerTransform;
    private Transform xform;
    ObjectPooler objectPooler;
    private const float SPAWN_OFFSET = 5f;

    private void OnLeftBehindThePlayer()
    {
        if (playerTransform.position.z < xform.position.z + SPAWN_OFFSET) { return; }

        objectPooler.DeactivateSpawnedObject(this.gameObject);
        
    }

    private void Start()
    {
        playerTransform = GameObject.Find("Player").transform;
        xform = GetComponent<Transform>();

        objectPooler = ObjectPooler.Instance;
    }

    private void Update()
    {
        OnLeftBehindThePlayer();
    }
}
