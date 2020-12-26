using System.Collections.Generic;
using UnityEngine;

public class CitySpawn : MonoBehaviour
{
    [SerializeField] private Transform playerTransform;

    private const float SPAWN_OFFSET = 10f;
    private const int INITIAL_SPAWN_COUNT = 40;
    private const int BUILDING_PREFAB_COUNT = 4;

    private readonly Quaternion leftBuildingRotation = Quaternion.Euler(0f, 0f, 0f);
    private readonly Quaternion rightBuildingRotation = Quaternion.Euler(0f, 180f, 0f);

    private readonly Queue<float> leftZpositions = new Queue<float>();
    private readonly Queue<float> rightZpositions = new Queue<float>();

    private Vector3 rightSpawnPos = new Vector3(5f, 0f, -6f);
    private Vector3 leftSpawnPos = new Vector3(-5f, 0f, -6f);

    private ObjectPooler objectPooler;
    private bool isPlayerAlive = true;

    private ObjectPooler.ObjectTag RandomTagConverter(int randomBuildingIndex)
    {
        switch (randomBuildingIndex)
        {
            case 1:
                return ObjectPooler.ObjectTag.building1;
            case 2:
                return ObjectPooler.ObjectTag.building2;
            case 3:
                return ObjectPooler.ObjectTag.building3;
            default:
                return ObjectPooler.ObjectTag.building4;
        }
    }

    private void InitialBuildingSpawn()
    {
        for (int i = 0; i < INITIAL_SPAWN_COUNT / 2; i++)
        {
            int randomLeftBuildingIndex = Random.Range(1, BUILDING_PREFAB_COUNT + 1);
            int randomRightBuildingIndex = Random.Range(1, BUILDING_PREFAB_COUNT + 1);

            ObjectPooler.ObjectTag leftTag = RandomTagConverter(randomLeftBuildingIndex);
            ObjectPooler.ObjectTag rightTag = RandomTagConverter(randomRightBuildingIndex);

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

        ObjectPooler.ObjectTag leftTag = RandomTagConverter(randomLeftBuildingIndex);

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

        ObjectPooler.ObjectTag rightTag = RandomTagConverter(randomRightBuildingIndex);

        rightSpawnPos += new Vector3(0f, 0f, objectPooler.zSizes[rightTag] / 2);
        objectPooler.SpawnFromPool(rightTag, rightSpawnPos, rightBuildingRotation);
        rightZpositions.Enqueue(rightSpawnPos.z);
        rightSpawnPos += new Vector3(0f, 0f, objectPooler.zSizes[rightTag] / 2);
    }

    private void OnEnable()
    {
        Player.OnPlayerDeath += (()=> 
        {
            leftZpositions.Clear(); 
            rightZpositions.Clear();
            isPlayerAlive = false;
        });
    }

    private void OnDisable()
    {
        Player.OnPlayerDeath -= (() =>
        {
            leftZpositions.Clear();
            rightZpositions.Clear();
            isPlayerAlive = false;
        });
    }

    private void Start()
    {
        objectPooler = ObjectPooler.Instance;

        InitialBuildingSpawn();
    }

    private void Update()
    {
        if (!isPlayerAlive) { return; }

        ChangeLeftBuildingsPositions();
        ChangeRightBuildingsPositions();
    }
}
