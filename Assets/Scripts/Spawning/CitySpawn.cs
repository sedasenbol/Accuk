using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CitySpawn : MonoBehaviour
{
    [SerializeField] private Transform playerTransform;

    private const float SPAWN_OFFSET = 5f;
    private const int INITIAL_SPAWN_COUNT = 80;
    private const int BUILDING_PREFAB_COUNT = 4;
    private readonly Quaternion leftBuildingRotation = Quaternion.Euler(0f, 0f, 0f);
    private readonly Quaternion rightBuildingRotation = Quaternion.Euler(0f, 180f, 0f);
    private Vector3 rightSpawnPos = new Vector3(5f, 0f, -6f);
    private Vector3 leftSpawnPos = new Vector3(-5f, 0f, -6f);
    private Queue<float> leftZpositions = new Queue<float>();
    private Queue<float> rightZpositions = new Queue<float>();

    ObjectPooler objectPooler;

    private ObjectPooler.objectTag RandomTagConverter(int randomBuildingIndex)
    {
        switch (randomBuildingIndex)
        {
            case 1:
                return ObjectPooler.objectTag.building1;
            case 2:
                return ObjectPooler.objectTag.building2;
            case 3:
                return ObjectPooler.objectTag.building3;
            case 4:
                return ObjectPooler.objectTag.building4;
            default:
                return ObjectPooler.objectTag.building1;
        }
    }

    private void InitialBuildingSpawn()
    {
        for (int i = 0; i < INITIAL_SPAWN_COUNT / 2; i++)
        {
            int randomLeftBuildingIndex = Random.Range(1, BUILDING_PREFAB_COUNT + 1);
            int randomRightBuildingIndex = Random.Range(1, BUILDING_PREFAB_COUNT + 1);

            ObjectPooler.objectTag leftTag = RandomTagConverter(randomLeftBuildingIndex);
            ObjectPooler.objectTag rightTag = RandomTagConverter(randomRightBuildingIndex);

            leftSpawnPos += new Vector3(0f, 0f, objectPooler.zSizes[leftTag] / 2);
            rightSpawnPos += new Vector3(0f, 0f, objectPooler.zSizes[rightTag] / 2);

            objectPooler.SpawnFromPool(leftTag, leftSpawnPos, leftBuildingRotation);
            objectPooler.SpawnFromPool(rightTag, rightSpawnPos, rightBuildingRotation);

            leftZpositions.Enqueue(leftSpawnPos.z);
            rightZpositions.Enqueue(rightSpawnPos.z);

            leftSpawnPos += new Vector3(0f, 0f, objectPooler.zSizes[leftTag] / 2);
            rightSpawnPos += new Vector3(0f, 0f, objectPooler.zSizes[rightTag] / 2);
       }
    }

    private void ChangeLeftBuildingsPositions()
    {
        if (playerTransform.position.z < leftZpositions.Peek() + SPAWN_OFFSET) { return; }

        leftZpositions.Dequeue();

        int randomLeftBuildingIndex = Random.Range(1, BUILDING_PREFAB_COUNT+1);

        ObjectPooler.objectTag leftTag = RandomTagConverter(randomLeftBuildingIndex);

        leftSpawnPos += new Vector3(0f, 0f, objectPooler.zSizes[leftTag] / 2);
        objectPooler.SpawnFromPool(leftTag, leftSpawnPos, leftBuildingRotation);
        leftZpositions.Enqueue(leftSpawnPos.z);
        leftSpawnPos += new Vector3(0f, 0f, objectPooler.zSizes[leftTag] / 2);
    }

    private void ChangeRightBuildingsPositions()
    {
        if (playerTransform.position.z < rightZpositions.Peek() + SPAWN_OFFSET) { return; }

        rightZpositions.Dequeue();

        int randomRightBuildingIndex = Random.Range(1, BUILDING_PREFAB_COUNT + 1);

        ObjectPooler.objectTag rightTag = RandomTagConverter(randomRightBuildingIndex);

        rightSpawnPos += new Vector3(0f, 0f, objectPooler.zSizes[rightTag] / 2);
        objectPooler.SpawnFromPool(rightTag, rightSpawnPos, rightBuildingRotation);
        rightZpositions.Enqueue(rightSpawnPos.z);
        rightSpawnPos += new Vector3(0f, 0f, objectPooler.zSizes[rightTag] / 2);
    }

    private void Start()
    {
        objectPooler = ObjectPooler.Instance;

        InitialBuildingSpawn();
    }


    private void Update()
    {
        ChangeLeftBuildingsPositions();
        ChangeRightBuildingsPositions();
    }
}
