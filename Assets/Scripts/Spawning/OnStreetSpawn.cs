using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnStreetSpawn : MonoBehaviour
{
    [SerializeField] private Transform playerTransform;
    private ObjectPooler objectPooler;
    private List<Vector3> spawnPositions = new List<Vector3> { new Vector3(-0.65f, 0f, 0f), new Vector3(0f, 0f, 0f), new Vector3(0.65f, 0f, 0f) };
    private List<Vector3> stopSignSpawnPositions = new List<Vector3> { new Vector3(-0.335f, 0f, 0f), new Vector3(0.335f, 0f, 0f) };
    private float SPAWN_LENGTH = 70;
    private List<Side> unusedSides = new List<Side> { Side.left, Side.middle, Side.right };
    private readonly List<Side> allSides = new List<Side> { Side.left, Side.middle, Side.right };
    private float busLength;
    private float playerForwardSpeed = 5f;
    private const float MOVING_BUS_SPEED = 5f;
    private const float MIN_MOVING_BUS_SPACE = 7f;
    private const float MAX_MOVING_BUS_SPACE = 12f;
    private const float MIN_EMPTY_STREET_DISTANCE = 10f;
    private const float MAX_EMPTY_STREET_DISTANCE = 30f;
    private const float MIN_MOVING_BUS_EMPTINESS = 20f;
    private const float MIN_EASY_OBSTACLE_DISTANCE = 5f;
    private const int EASY_OBSTACLE_INVERSE_FREQ_IN_UNUSED_SIDE = 3;
    private Vector3 stopSignSpawnFreq = new Vector3(0f, 0f, 5f);

    private enum Side
    {
        left = 0,
        middle = 1,
        right = 2,
    }

    private void SpawnStopSign()
    {
        if (playerTransform.position.z < Mathf.Min(stopSignSpawnPositions[0].z, stopSignSpawnPositions[1].z) - SPAWN_LENGTH) { return; }

        int spawnStopSign = Random.Range(0, 7);
        if (spawnStopSign == 6)
        {
            int stopSignSide = Random.Range(0, 2);
            objectPooler.SpawnFromPool(ObjectPooler.objectTag.stopSign, stopSignSpawnPositions[stopSignSide], Quaternion.identity);
        }
        stopSignSpawnPositions[0] += stopSignSpawnFreq;
    }

    private void SpawnOthers()
    {
        if (playerTransform.position.z < Mathf.Min(spawnPositions[0].z, spawnPositions[1].z, spawnPositions[2].z) - SPAWN_LENGTH) { return; }

        int randomHardSideCount = Random.Range(1, 3);
        List<Side> hardSides = new List<Side>();
        for (int i = 0; i < randomHardSideCount; i++)
        {
            AddRandomSides(hardSides);
        }
        int randomHardSideLength = Random.Range(1, 5);
        for (int i = 0; i < randomHardSideCount; i++)
        {
            Vector3 randomHardSideOffset = new Vector3(0f, 0f, 0f);
            for (int j = 0; j < randomHardSideLength; j++)
            {
                if (j == 0)
                {
                    randomHardSideOffset = new Vector3(0f, 0f, Random.Range(0f, 6f));
                }
                int randomBusWRamp = Random.Range(0, 2);
                if (j == 0 && randomBusWRamp == 1)
                {
                    objectPooler.SpawnFromPool(ObjectPooler.objectTag.busWRamp, spawnPositions[(int)hardSides[0]] + randomHardSideOffset, Quaternion.identity);
                    spawnPositions[(int)hardSides[0]] += new Vector3(0f, 0f, busLength * 1.5f);
                }
                else
                {
                    objectPooler.SpawnFromPool(ObjectPooler.objectTag.bus, spawnPositions[(int)hardSides[0]] + randomHardSideOffset, Quaternion.identity);
                    spawnPositions[(int)hardSides[0]] += new Vector3(0f, 0f, busLength);
                }
            }
            unusedSides.Remove(hardSides[0]);
            hardSides.RemoveAt(0);    
        }
        foreach (Side side in unusedSides)
        {
            int randomEasyObstacle = Random.Range(0, EASY_OBSTACLE_INVERSE_FREQ_IN_UNUSED_SIDE);
            Vector3 randomObstacleOffset = new Vector3(0f, 0f, Random.Range(0f, randomHardSideLength * busLength));
            if (randomEasyObstacle == EASY_OBSTACLE_INVERSE_FREQ_IN_UNUSED_SIDE)
            {
                ObjectPooler.objectTag randomObject = ConvertIntToTag(Random.Range(0, 2));
                objectPooler.SpawnFromPool(randomObject, spawnPositions[(int)side] + randomObstacleOffset, Quaternion.identity);
            }
            spawnPositions[(int)side] += new Vector3(0f, 0f, randomHardSideLength * busLength); 
        }
        unusedSides.Clear();
        unusedSides.AddRange(allSides);
        MakeSpawnPositionsEqual();
        float movingBusMinimumDistance = ((spawnPositions[0].z + MIN_MOVING_BUS_SPACE - playerTransform.position.z) / playerForwardSpeed) * MOVING_BUS_SPEED;
        float randomEmptyStreetDistance = Random.Range(MIN_EMPTY_STREET_DISTANCE, MAX_EMPTY_STREET_DISTANCE);
        List<Side> movingBusSideList = new List<Side>();
        if (randomEmptyStreetDistance > MIN_MOVING_BUS_EMPTINESS)
        {
            int movingBusSideCount = Random.Range(1, 3);
            for (int i = 0; i < movingBusSideCount; i++)
            {
                AddRandomSides(movingBusSideList);
            }
            for (int i = 0; i < movingBusSideCount; i++)
            {
                Vector3 spawnPos = spawnPositions[(int)movingBusSideList[i]] + new Vector3(0f, 0f, movingBusMinimumDistance + Random.Range(MIN_MOVING_BUS_SPACE, MAX_MOVING_BUS_SPACE));
                objectPooler.SpawnFromPool(ObjectPooler.objectTag.movingBus, spawnPos, Quaternion.identity);
            }
        }
        if (movingBusSideList.Count == 0)
        {
            objectPooler.SpawnFromPool(ConvertIntToTag(Random.Range(0, 2)), spawnPositions[0] + new Vector3(0f, 0f, Random.Range(MIN_EASY_OBSTACLE_DISTANCE, randomEmptyStreetDistance - MIN_EASY_OBSTACLE_DISTANCE)), Quaternion.identity);
            objectPooler.SpawnFromPool(ConvertIntToTag(Random.Range(0, 2)), spawnPositions[1] + new Vector3(0f, 0f, Random.Range(MIN_EASY_OBSTACLE_DISTANCE, randomEmptyStreetDistance - MIN_EASY_OBSTACLE_DISTANCE)), Quaternion.identity);
            objectPooler.SpawnFromPool(ConvertIntToTag(Random.Range(0, 2)), spawnPositions[2] + new Vector3(0f, 0f, Random.Range(MIN_EASY_OBSTACLE_DISTANCE, randomEmptyStreetDistance - MIN_EASY_OBSTACLE_DISTANCE)), Quaternion.identity);
        }
        else
        {
            List<Side> sidesWithoutMovingBus = new List<Side>();
            for (int i = 0; i < allSides.Count; i++)
            {
                if (!movingBusSideList.Contains(allSides[i]))
                {
                    sidesWithoutMovingBus.Add(allSides[i]);
                }
            }
            foreach (Side side in sidesWithoutMovingBus)
            {
                objectPooler.SpawnFromPool(ConvertIntToTag(Random.Range(0, 2)), spawnPositions[(int)side] + new Vector3(0f, 0f, Random.Range(MAX_MOVING_BUS_SPACE + MIN_EASY_OBSTACLE_DISTANCE, randomEmptyStreetDistance - MIN_EASY_OBSTACLE_DISTANCE)), Quaternion.identity);
            }
                
        }
        for (int i= 0; i < spawnPositions.Count; i++)
        {
            spawnPositions[i] += new Vector3(0f, 0f, randomEmptyStreetDistance);
        }
    }

    private void MakeSpawnPositionsEqual()
    {
        float maxZPos = -Mathf.Infinity;
        foreach (Vector3 pos in spawnPositions)
        {
            if (pos.z > maxZPos)
            {
                maxZPos = pos.z;
            }
        }
        for (int i = 0; i < spawnPositions.Count; i++)
        {
            spawnPositions[i] = new Vector3(spawnPositions[i].x, spawnPositions[i].y, maxZPos);
        }
    }

    private List<Side> AddRandomSides(List<Side> listOfSides)
    {
        Side side = (Side)Random.Range(0, 3);
        if (!listOfSides.Contains(side))
        {
            listOfSides.Add(side);
            return listOfSides;
        }
        else
        {
            return AddRandomSides(listOfSides);
        }
    }

    private ObjectPooler.objectTag ConvertIntToTag(int randomObject)
    {
        switch (randomObject)
        {
            case 0:
                return ObjectPooler.objectTag.lowWarningStand;
            case 1:
                return ObjectPooler.objectTag.warningStand;
            default:
                return ObjectPooler.objectTag.lowWarningStand;
        }
    }

    private void Start()
    {
        objectPooler = ObjectPooler.Instance;
        busLength = objectPooler.zSizes[ObjectPooler.objectTag.bus];
    }

    private void Update()
    {
        SpawnStopSign();
        SpawnOthers();        
    }
}
