﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloorSpawn : MonoBehaviour
{
    [SerializeField] private Transform streetPrefab;
    [SerializeField] private Transform crosswalkPrefab;
    [SerializeField] private Transform grassPrefab;
    [SerializeField] private Transform streetLampPrefab;
    [SerializeField] private Transform benchPrefab;
    [SerializeField] private Transform playerTransform;
    [SerializeField] private Transform grassContainer;
    [SerializeField] private Transform streetContainer;
    [SerializeField] private Transform streetLampContainer;
    [SerializeField] private Transform benchContainer;

    private float streetZSize;
    private float crosswalkZSize;
    private Vector3 grassSize;
    private const float SPAWN_OFFSET = 15f;
    private readonly Vector3 streetLampSpawnDistance = new Vector3(0f, 0f, 35f);
    private readonly Vector3 benchStreetLampDistance = new Vector3(0f, 0f, 1f);
    private const int STREET_LAMP_LIST_SIZE = 2 * 5;
    private const int BENCH_LIST_SIZE = 2 * STREET_LAMP_LIST_SIZE;

    private const int STREET_LIST_SIZE = 10;
    private const int ALIGNED_GRASS_COUNT = 3;
    private const int GRASS_LIST_SIZE = ALIGNED_GRASS_COUNT * 10;
    private int benchListIndex = 0;
    private int streetListIndex = 0;
    private int grassListIndex = 0;
    private int streetLampIndex = 0;

    private List<Transform> streetList = new List<Transform>();
    private List<Transform> grassList = new List<Transform>();
    private List<Transform> streetLampList = new List<Transform>();
    private List<Transform> benchList = new List<Transform>();
    private Transform crosswalk;

    private readonly Vector3 initialStreetSpawnPos = new Vector3(0f, 0f, -6f);
    private readonly Vector3 leftStreetLampSpawnPos = new Vector3(-1.25f, 0f, -6f);
    private readonly Vector3 rightStreetLampSpawnPos = new Vector3(1.25f, 0f, -6f);
    private readonly Quaternion leftStreetLampRotation = Quaternion.Euler(0f, 180f, 0f);
    private readonly Quaternion rightStreetLampRotation = Quaternion.Euler(0f, 0f, 0f);
    private readonly Quaternion leftBenchRotation = Quaternion.Euler(0f, 90f, 0f);
    private readonly Quaternion rightBenchRotation = Quaternion.Euler(0f, 270f, 0f);

    private void InitialSpawn()
    {
        for (int i = 0; i < STREET_LIST_SIZE; i++)
        {
            streetList.Add(Instantiate(streetPrefab, initialStreetSpawnPos + new Vector3(0f, 0f, i * streetZSize), Quaternion.identity, streetContainer));
        }
        crosswalk = Instantiate(crosswalkPrefab, new Vector3(0, 0, streetList[STREET_LIST_SIZE-1].position.z + streetZSize/2 + crosswalkZSize/2), Quaternion.identity, streetContainer);

        for (int i=0; i < GRASS_LIST_SIZE/ALIGNED_GRASS_COUNT; i++)
        {
            grassList.Add(Instantiate(grassPrefab, initialStreetSpawnPos + new Vector3(0f, 0f, i * grassSize.z), Quaternion.identity, grassContainer));

            for (int j = 0; j < ALIGNED_GRASS_COUNT - 1; j += 2)
            {
                grassList.Add(Instantiate(grassPrefab, initialStreetSpawnPos + new Vector3((j / 2 + 1) * grassSize.x, 0f, i * grassSize.z), Quaternion.identity, grassContainer));
                grassList.Add(Instantiate(grassPrefab, initialStreetSpawnPos + new Vector3(-(j / 2 + 1) * grassSize.x, 0f, i * grassSize.z), Quaternion.identity, grassContainer));
            }
        }

        for (int i = 0; i < STREET_LAMP_LIST_SIZE / 2; i++)
        {
            streetLampList.Add(Instantiate(streetLampPrefab, leftStreetLampSpawnPos + i * streetLampSpawnDistance, leftStreetLampRotation, streetLampContainer));
            streetLampList.Add(Instantiate(streetLampPrefab, rightStreetLampSpawnPos + i * streetLampSpawnDistance, rightStreetLampRotation, streetLampContainer));
            benchList.Add(Instantiate(benchPrefab, leftStreetLampSpawnPos + i * streetLampSpawnDistance - benchStreetLampDistance, leftBenchRotation, benchContainer));
            benchList.Add(Instantiate(benchPrefab, leftStreetLampSpawnPos + i * streetLampSpawnDistance + benchStreetLampDistance, leftBenchRotation, benchContainer));
            benchList.Add(Instantiate(benchPrefab, leftStreetLampSpawnPos + i * streetLampSpawnDistance - benchStreetLampDistance, rightBenchRotation, benchContainer));
            benchList.Add(Instantiate(benchPrefab, leftStreetLampSpawnPos + i * streetLampSpawnDistance + benchStreetLampDistance, rightBenchRotation, benchContainer));
        }
    }

    private void ChangeStreetPositions()
    {
        if (playerTransform.position.z < streetList[streetListIndex].position.z + SPAWN_OFFSET) { return; }

        float spawnPosition = Mathf.Max(streetList[(streetListIndex + STREET_LIST_SIZE - 1) % STREET_LIST_SIZE].position.z + streetZSize/2, crosswalk.position.z + crosswalkZSize/2);

        int randomSpawnNumber = Random.Range(0, 6);

        if (randomSpawnNumber == 0 && playerTransform.position.z > crosswalk.position.z + SPAWN_OFFSET)
        {
            crosswalk.position = new Vector3(0, 0, spawnPosition + crosswalkZSize / 2);
        }
        else
        {
            streetList[streetListIndex].position = new Vector3(0, 0, spawnPosition + streetZSize / 2);
            
            streetListIndex = (streetListIndex + 1) % STREET_LIST_SIZE;
        }

    }

    private void ChangeGrassPositions()
    {
        if (playerTransform.position.z < grassList[grassListIndex].position.z + SPAWN_OFFSET) { return; }

        grassList[grassListIndex].position = new Vector3(0, 0, grassList[(grassListIndex + GRASS_LIST_SIZE - 1) % GRASS_LIST_SIZE].position.z + grassSize.z);

        for (int j = 0; j < ALIGNED_GRASS_COUNT - 1; j += 2)
        {
            grassList[grassListIndex + j + 1 % GRASS_LIST_SIZE].position = new Vector3((j / 2 + 1) * grassSize.x, 0, grassList[(grassListIndex + GRASS_LIST_SIZE - 1) % GRASS_LIST_SIZE].position.z + grassSize.z);
            grassList[grassListIndex + j + 2 % GRASS_LIST_SIZE].position = new Vector3(-(j / 2 + 1) * grassSize.x, 0, grassList[(grassListIndex + GRASS_LIST_SIZE - 1) % GRASS_LIST_SIZE].position.z + grassSize.z);
        }

        grassListIndex = (grassListIndex + ALIGNED_GRASS_COUNT) % GRASS_LIST_SIZE;

    }

    private void ChangeStreetLampPositions()
    {
        if (playerTransform.position.z < streetLampList[streetLampIndex].position.z + SPAWN_OFFSET) { return; }

        streetLampList[streetLampIndex].position += streetLampSpawnDistance * (STREET_LAMP_LIST_SIZE / 2 - 1);
        streetLampList[streetLampIndex + 1].position += streetLampSpawnDistance * (STREET_LAMP_LIST_SIZE / 2 - 1);

        streetLampIndex = (streetLampIndex += 2) % STREET_LAMP_LIST_SIZE;
    }

    private void Start()
    {
        streetZSize = streetPrefab.GetChild(0).GetComponent<Renderer>().bounds.size.z;
        crosswalkZSize = crosswalkPrefab.GetChild(0).GetChild(0).GetComponent<Renderer>().bounds.size.z;
        grassSize = grassPrefab.GetComponent<Renderer>().bounds.size;

        InitialSpawn();    
    }

    private void Update()
    {
        ChangeStreetPositions();
        ChangeGrassPositions();
        ChangeStreetLampPositions();
    }
}
