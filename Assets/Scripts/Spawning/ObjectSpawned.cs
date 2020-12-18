﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectSpawned : MonoBehaviour
{
    private Transform playerTransform;
    ObjectPooler objectPooler = ObjectPooler.Instance;
    private const float SPAWN_OFFSET = 5f;

    private void OnLeftBehindThePlayer()
    {
        if (playerTransform.position.z < transform.position.z + SPAWN_OFFSET) { return; }

        objectPooler.DeactivateSpawnedObject(this.gameObject);
        
    }

    private void Start()
    {
        playerTransform = GameObject.Find("Player").transform;
    }

    private void Update()
    {
        OnLeftBehindThePlayer();
    }
}
