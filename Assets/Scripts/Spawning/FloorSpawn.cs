using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloorSpawn : MonoBehaviour
{
    [SerializeField] private Transform streetPrefab;
    [SerializeField] private Transform crosswalkPrefab;
    [SerializeField] private Transform grassPrefab;
    [SerializeField] private Transform playerTransform;
    [SerializeField] private Transform grassContainer;
    [SerializeField] private Transform streetContainer;

    private float streetZSize;
    private float crosswalkZSize;
    private Vector3 grassSize;
    private const float SPAWN_OFFSET = 5f;

    private const int STREET_LIST_SIZE = 10;
    private const int ALIGNED_GRASS_COUNT = 7;
    private const int GRASS_LIST_SIZE = ALIGNED_GRASS_COUNT * 10;
    private int streetListIndex = 0;
    private int grassListIndex = 0;
    private List<Transform> streetList = new List<Transform>();
    private List<Transform> grassList = new List<Transform>();
    private Transform crosswalk;

    private Vector3 initialSpawnPos = new Vector3(0f, 0f, -6f);

    private void InitialSpawn()
    {
        for (int i = 0; i < STREET_LIST_SIZE; i++)
        {
            streetList.Add(Instantiate(streetPrefab, initialSpawnPos + new Vector3(0f, 0f, i * streetZSize), Quaternion.identity, streetContainer));
        }
        crosswalk = Instantiate(crosswalkPrefab, new Vector3(0, 0, streetList[STREET_LIST_SIZE-1].position.z + streetZSize/2 + crosswalkZSize/2), Quaternion.identity, streetContainer);

        for (int i=0; i < GRASS_LIST_SIZE/ALIGNED_GRASS_COUNT; i++)
        {
            grassList.Add(Instantiate(grassPrefab, initialSpawnPos + new Vector3(0f, 0f, i * grassSize.z), Quaternion.identity, grassContainer));

            for (int j = 0; j < ALIGNED_GRASS_COUNT - 1; j += 2)
            {
                grassList.Add(Instantiate(grassPrefab, initialSpawnPos + new Vector3((j / 2 + 1) * grassSize.x, 0f, i * grassSize.z), Quaternion.identity, grassContainer));
                grassList.Add(Instantiate(grassPrefab, initialSpawnPos + new Vector3(-(j / 2 + 1) * grassSize.x, 0f, i * grassSize.z), Quaternion.identity, grassContainer));
            }

        }
    }

    private void ChangeStreetPosition()
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

    private void ChangeGrassPosition()
    {
        if (playerTransform.position.z < grassList[grassListIndex].position.z + SPAWN_OFFSET) { return; }

        grassList[grassListIndex].position = new Vector3(0, 0, grassList[(grassListIndex + GRASS_LIST_SIZE - 1) % GRASS_LIST_SIZE].position.z + grassSize.z);

        for (int j = 0; j < ALIGNED_GRASS_COUNT - 1; j += 2)
        {
            grassList[grassListIndex + j + 1 % GRASS_LIST_SIZE].position = new Vector3((j / 2 + 1) * grassSize.x, 0, grassList[(grassListIndex + GRASS_LIST_SIZE - 1) % GRASS_LIST_SIZE].position.z + grassSize.z);
            grassList[grassListIndex + j + 2 % GRASS_LIST_SIZE].position = new Vector3(-(j / 2 + 1) * grassSize.x, 0, grassList[(grassListIndex + GRASS_LIST_SIZE - 1) % GRASS_LIST_SIZE].position.z + grassSize.z);
        }

        /*grassList[grassListIndex + 1 % GRASS_LIST_SIZE].position = new Vector3(grassSize.x, 0, grassList[(grassListIndex + GRASS_LIST_SIZE - 1) % GRASS_LIST_SIZE].position.z + grassSize.z);
        grassList[grassListIndex + 2 % GRASS_LIST_SIZE].position = new Vector3(-grassSize.x, 0, grassList[(grassListIndex + GRASS_LIST_SIZE - 1) % GRASS_LIST_SIZE].position.z + grassSize.z);
        grassList[grassListIndex + 3 % GRASS_LIST_SIZE].position = new Vector3(2 * grassSize.x, 0, grassList[(grassListIndex + GRASS_LIST_SIZE - 1) % GRASS_LIST_SIZE].position.z + grassSize.z);
        grassList[grassListIndex + 4 % GRASS_LIST_SIZE].position = new Vector3(-2 * grassSize.x, 0, grassList[(grassListIndex + GRASS_LIST_SIZE - 1) % GRASS_LIST_SIZE].position.z + grassSize.z);
        grassList[grassListIndex + 5 % GRASS_LIST_SIZE].position = new Vector3(3 * grassSize.x, 0, grassList[(grassListIndex + GRASS_LIST_SIZE - 1) % GRASS_LIST_SIZE].position.z + grassSize.z);
        grassList[grassListIndex + 6 % GRASS_LIST_SIZE].position = new Vector3(-3 * grassSize.x, 0, grassList[(grassListIndex + GRASS_LIST_SIZE - 1) % GRASS_LIST_SIZE].position.z + grassSize.z);*/

        grassListIndex = (grassListIndex + ALIGNED_GRASS_COUNT) % GRASS_LIST_SIZE;

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
        ChangeStreetPosition();
        ChangeGrassPosition();
    }
}
