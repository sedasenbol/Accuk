using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnStreetSpawn : MonoBehaviour
{
    [SerializeField] private Transform playerTransform;
    private ObjectPooler objectPooler;
    private List<Vector3> spawnPositions = new List<Vector3> { new Vector3(-0.65f, 0f, 0f), new Vector3(0f, 0f, 0f), new Vector3(0.65f, 0f, 0f) };
    private List<Vector3> stopSignSpawnPositions = new List<Vector3> { new Vector3(-0.335f, 0f, 0f), new Vector3(0.335f, 0f, 0f) };
    private float SPAWN_LENGTH = 100;
    private List<Side> unusedSides = new List<Side> { Side.left, Side.middle, Side.right };
    private readonly List<Side> allSides = new List<Side> { Side.left, Side.middle, Side.right };
    private float busLength;
    private float playerForwardSpeed = 7f;
    private const int MIN_HARD_SIDE_COUNT = 1;
    private const int MAX_HARD_SIDE_COUNT = 2;
    private const int MIN_HARD_SIDE_LENGTH = 2;
    private const int MAX_HARD_SIDE_LENGTH = 8;
    private const float MOVING_BUS_SPEED = 6f;
    private const float MIN_MOVING_BUS_SPACE = 7f;
    private const float MAX_MOVING_BUS_SPACE = 12f;
    private const float MIN_EMPTY_STREET_DISTANCE = 15f;
    private const float MAX_EMPTY_STREET_DISTANCE = 25f;
    private const float MIN_MOVING_BUS_EMPTINESS = 20f;
    private const float MIN_EASY_OBSTACLE_DISTANCE = 5f;
    private const int STOP_SIGN_INVERSE_FREQ = 7;
    private readonly Vector3 distanceBtwHardStreet = new Vector3(0f, 0f, 5f);
    private Vector3 stopSignSpawnFreq = new Vector3(0f, 0f, 5f);
    private bool isLastSpawnHard = false;

    private enum Side
    {
        left = 0,
        middle = 1,
        right = 2,
    }

    private void SpawnStopSign()
    {
        if (playerTransform.position.z < Mathf.Min(stopSignSpawnPositions[0].z, stopSignSpawnPositions[1].z) - SPAWN_LENGTH) { return; }

        int spawnStopSign = Random.Range(0, STOP_SIGN_INVERSE_FREQ);
        if (spawnStopSign == 1)
        {
            int stopSignSide = Random.Range(0, stopSignSpawnPositions.Count);
            objectPooler.SpawnFromPool(ObjectPooler.objectTag.stopSign, stopSignSpawnPositions[stopSignSide], Quaternion.identity);
        }
        stopSignSpawnPositions[0] += stopSignSpawnFreq;
    }

    private void SpawnHardStreet()
    {
        int randomHardSideCount = Random.Range(MIN_HARD_SIDE_COUNT, MAX_HARD_SIDE_COUNT + 1);
        List<Side> hardSides = new List<Side>();
        for (int i = 0; i < randomHardSideCount; i++)
        {
            AddRandomSides(hardSides);
        }
        int randomHardSideLength = Random.Range(MIN_HARD_SIDE_LENGTH, MAX_HARD_SIDE_LENGTH + 1);
        for (int i = 0; i < randomHardSideCount; i++)
        {
            for (int j = 0; j < randomHardSideLength; j++)
            {
                Vector3 randomHardSideOffset = Vector3.zero;
                if (j == 0)
                {
                    randomHardSideOffset = new Vector3(0f, 0f, Random.Range(0f, 6f));
                }
                int randomBusWRamp = Random.Range(0, 2);
                if (j == 0 && randomBusWRamp == 1)
                {
                    objectPooler.SpawnFromPool(ObjectPooler.objectTag.busWRamp, spawnPositions[(int)hardSides[0]] + randomHardSideOffset, Quaternion.identity, "Bus W Ramp At The Beginning of a Hard Side");
                    spawnPositions[(int)hardSides[0]] += new Vector3(0f, 0f, busLength * 1.5f) + randomHardSideOffset;
                }
                else if (j == randomHardSideLength - 1 && i == 0)
                {
                    int randomShorterHardSide = Random.Range(0, 2);
                    if (randomShorterHardSide != 1)
                    {
                        objectPooler.SpawnFromPool(ObjectPooler.objectTag.bus, spawnPositions[(int)hardSides[0]], Quaternion.identity, "Bus Unshortened At The Hard Side");
                    }
                    spawnPositions[(int)hardSides[0]] += new Vector3(0f, 0f, busLength);
                }
                else
                {
                    objectPooler.SpawnFromPool(ObjectPooler.objectTag.bus, spawnPositions[(int)hardSides[0]], Quaternion.identity, "Bus Usual At The Hard Side");
                    spawnPositions[(int)hardSides[0]] += new Vector3(0f, 0f, busLength);
                }
            }
            unusedSides.Remove(hardSides[0]);
            hardSides.RemoveAt(0);
        }
        foreach (Side side in unusedSides)
        {
            Vector3 randomObstacleOffset = new Vector3(0f, 0f, Random.Range(0f, (randomHardSideLength - 1) * busLength));
            ObjectPooler.objectTag randomObject = ConvertIntToTag(Random.Range(0, 3));
            objectPooler.SpawnFromPool(randomObject, spawnPositions[(int)side] + randomObstacleOffset, Quaternion.identity, "Easy Obstacle At The Unused Side");
            spawnPositions[(int)side] += new Vector3(0f, 0f, randomHardSideLength * busLength);
        }
        unusedSides.Clear();
        unusedSides.AddRange(allSides);
        MakeSpawnPositionsEqual();
    }

    private void SpawnEmptyOrMovingBusStreet(float randomEmptyStreetDistance)
    {

        float movingBusMinimumDistance = ((spawnPositions[0].z + MIN_MOVING_BUS_SPACE - playerTransform.position.z) / playerForwardSpeed) * MOVING_BUS_SPEED;
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
                objectPooler.SpawnFromPool(ObjectPooler.objectTag.movingBus, spawnPos, Quaternion.identity, "Moving Bus");
                unusedSides.Remove(movingBusSideList[i]);
            }
            for (int i = 0; i < unusedSides.Count; i++)
            {
                objectPooler.SpawnFromPool(ConvertIntToTag(Random.Range(0, 3)), spawnPositions[(int)unusedSides[i]] + new Vector3(0f, 0f, Random.Range(MIN_EASY_OBSTACLE_DISTANCE, randomEmptyStreetDistance - MIN_EASY_OBSTACLE_DISTANCE)), Quaternion.identity, "Easy Obstacle at The Moving Bus Unused Side");
            }
        }
        else
        {
            objectPooler.SpawnFromPool(ConvertIntToTag(Random.Range(0, 3)), spawnPositions[0] + new Vector3(0f, 0f, Random.Range(MIN_EASY_OBSTACLE_DISTANCE, randomEmptyStreetDistance - MIN_EASY_OBSTACLE_DISTANCE)), Quaternion.identity, "Empty Street Easy Obstacle");
            objectPooler.SpawnFromPool(ConvertIntToTag(Random.Range(0, 3)), spawnPositions[1] + new Vector3(0f, 0f, Random.Range(MIN_EASY_OBSTACLE_DISTANCE, randomEmptyStreetDistance - MIN_EASY_OBSTACLE_DISTANCE)), Quaternion.identity, "Empty Street Easy Obstacle");
            objectPooler.SpawnFromPool(ConvertIntToTag(Random.Range(0, 3)), spawnPositions[2] + new Vector3(0f, 0f, Random.Range(MIN_EASY_OBSTACLE_DISTANCE, randomEmptyStreetDistance - MIN_EASY_OBSTACLE_DISTANCE)), Quaternion.identity, "Empty Street Easy Obstacle");
        }
        unusedSides.Clear();
        unusedSides.AddRange(allSides);
        for (int i = 0; i < spawnPositions.Count; i++)
        {
            spawnPositions[i] += new Vector3(0f, 0f, randomEmptyStreetDistance);
        }
    }

    private void SpawnOthers()
    {
        if (playerTransform.position.z < Mathf.Min(spawnPositions[0].z, spawnPositions[1].z, spawnPositions[2].z) - SPAWN_LENGTH) { return; }

        int randomStreet = Random.Range(0, 2);

        if (randomStreet == 1)
        {
            if (isLastSpawnHard)
            {
                spawnPositions[0] += distanceBtwHardStreet;
                MakeSpawnPositionsEqual();
            }
            SpawnHardStreet();
            isLastSpawnHard = true;
            return;
        }
        int randomStreetWMovingBus = Random.Range(0, 2);
        if (randomStreetWMovingBus == 1)
        {
            SpawnEmptyOrMovingBusStreet(Random.Range(MIN_EMPTY_STREET_DISTANCE, MAX_EMPTY_STREET_DISTANCE));
        }
        else
        {
            SpawnEmptyOrMovingBusStreet(10f);
        }
        isLastSpawnHard = false;
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
            case 2:
                return ObjectPooler.objectTag.busWRamp;
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
