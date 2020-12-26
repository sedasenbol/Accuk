using System.Collections.Generic;
using UnityEngine;

public class OnTheStreetSpawn : MonoBehaviour
{
    [SerializeField] private Transform playerTransform;

    private ObjectPooler objectPooler;

    private readonly List<Vector3> spawnPositions = new List<Vector3> { new Vector3(-0.65f, 0f, 0f), new Vector3(0f, 0f, 0f), new Vector3(0.65f, 0f, 0f) };
    private readonly List<Vector3> stopSignSpawnPositions = new List<Vector3> { new Vector3(-0.335f, 0f, 0f), new Vector3(0.335f, 0f, 0f) };

    private const float SPAWN_LENGTH = 50;

    private readonly List<Lane> unusedLanes = new List<Lane> { Lane.left, Lane.middle, Lane.right };
    private readonly List<Lane> allLanes = new List<Lane> { Lane.left, Lane.middle, Lane.right };

    private float busLength;
    private const float RAMP_OVER_BUS_LENGTH = 0.5f;

    private const float PLAYER_FORWARD_SPEED = 7f;
    private const float MOVING_BUS_SPEED = 6f;

    private const int MIN_HARD_LANE_COUNT = 1;
    private const int MAX_HARD_LANE_COUNT = 2;
    private const int MIN_HARD_LANE_LENGTH = 2;
    private const int MAX_HARD_LANE_LENGTH = 8;

    private bool isLastSpawnHard = false;

    private const float MIN_MOVING_BUS_SPACE = 7f;
    private const float MAX_MOVING_BUS_SPACE = 12f;

    private const float MIN_EASY_STREET_DISTANCE = 13f;
    private const float MAX_EASY_STREET_DISTANCE = 16f;

    private const float MIN_MOVING_BUS_STREET_DISTANCE = 20f;
    private const float MAX_MOVING_BUS_STREET_DISTANCE = 24f;

    private const float MIN_MOVING_BUS_EMPTINESS = 20f;
    private const float MIN_EASY_OBSTACLE_DISTANCE = 4f;

    private const int BUS_W_RAMP_INVERSE_FREQ = 3;
    private const int STOP_SIGN_INVERSE_FREQ = 7;

    private readonly Vector3 distanceBtwHardStreet = new Vector3(0f, 0f, 10f);
    private Vector3 stopSignSpawnFreq = new Vector3(0f, 0f, 5f);

    private const float COIN_SPAWN_OFFSET = 0.35f;
    private const float DISTANCE_BTW_PICK_UPS = 0.75f;
    private const float PICK_UP_HEIGHT = 0.4f;
    private const int EASY_OBSTACLE_COUNT = 3;
    private const float BUS_HEIGHT = 0.85f;
    private const int PICK_UP_COUNT_ON_TRAFFIC = 5;

    private bool isPlayerAlive = true;

    private enum Lane
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
            int stopSignLane = Random.Range(0, stopSignSpawnPositions.Count);
            objectPooler.SpawnFromPool(ObjectPooler.ObjectTag.stopSign, stopSignSpawnPositions[stopSignLane], Quaternion.identity);
        }
        stopSignSpawnPositions[0] += stopSignSpawnFreq;
        stopSignSpawnPositions[1] += stopSignSpawnFreq;
    }

    private void SpawnHardStreet()
    {
        int randomHardLaneCount = Random.Range(MIN_HARD_LANE_COUNT, MAX_HARD_LANE_COUNT + 1);
        List<Lane> hardLanes = new List<Lane>();

        for (int i = 0; i < randomHardLaneCount; i++)
        {
            AddRandomLanes(hardLanes);
        }

        int randomHardLaneBusCount = Random.Range(MIN_HARD_LANE_LENGTH, MAX_HARD_LANE_LENGTH + 1);
        for (int i = 0; i < randomHardLaneCount; i++)
        {
            SpawnPickUps(spawnPositions[(int)hardLanes[0]], ObjectPooler.ObjectTag.bus);
            for (int j = 0; j < randomHardLaneBusCount; j++)
            {
                Vector3 randomHardLaneOffset = Vector3.zero;
                if (j == 0)
                {
                    randomHardLaneOffset = new Vector3(0f, 0f, Random.Range(0f, 6f));
                }

                int randomBusWRamp = Random.Range(0, BUS_W_RAMP_INVERSE_FREQ);
                if (j == 0 && randomBusWRamp == 1)
                {
                    objectPooler.SpawnFromPool(ObjectPooler.ObjectTag.busWRamp, spawnPositions[(int)hardLanes[0]] + randomHardLaneOffset, Quaternion.identity, "Bus W Ramp At The Beginning of a Hard Lane");
                    SpawnPickUps(spawnPositions[(int)hardLanes[0]] + randomHardLaneOffset, ObjectPooler.ObjectTag.busWRamp, randomHardLaneBusCount);
                    spawnPositions[(int)hardLanes[0]] += new Vector3(0f, 0f, busLength * (RAMP_OVER_BUS_LENGTH + 1)) + randomHardLaneOffset;
                }
                else if (j == randomHardLaneBusCount - 1 && i == 0)
                {
                    int randomShorterHardLane = Random.Range(0, 2);
                    if (randomShorterHardLane != 1)
                    {
                        objectPooler.SpawnFromPool(ObjectPooler.ObjectTag.bus, spawnPositions[(int)hardLanes[0]], Quaternion.identity, "Bus Unshortened At The Hard Lane");
                        SpawnPickUps(spawnPositions[(int)hardLanes[0]], ObjectPooler.ObjectTag.bus);
                        spawnPositions[(int)hardLanes[0]] += new Vector3(0f, 0f, busLength);
                    }
                }
                else
                {
                    objectPooler.SpawnFromPool(ObjectPooler.ObjectTag.bus, spawnPositions[(int)hardLanes[0]], Quaternion.identity, "Bus Usual At The Hard Lane");
                    spawnPositions[(int)hardLanes[0]] += new Vector3(0f, 0f, busLength);
                }
            }

            unusedLanes.Remove(hardLanes[0]);
            hardLanes.RemoveAt(0);
        }
        foreach (Lane Lane in unusedLanes)
        {
            Vector3 randomObstacleOffset = new Vector3(0f, 0f, Random.Range(0f, (randomHardLaneBusCount - 1 - RAMP_OVER_BUS_LENGTH) * busLength));
            ObjectPooler.ObjectTag randomTag = ConvertIntToTag(Random.Range(0, EASY_OBSTACLE_COUNT));

            objectPooler.SpawnFromPool(randomTag, spawnPositions[(int)Lane] + randomObstacleOffset, Quaternion.identity, "Easy Obstacle At The Unused Lane");
            SpawnPickUps(spawnPositions[(int)Lane] + randomObstacleOffset, randomTag, 1);
            spawnPositions[(int)Lane] += new Vector3(0f, 0f, randomHardLaneBusCount * busLength);
        }

        unusedLanes.Clear();
        unusedLanes.AddRange(allLanes);
        MakeSpawnPositionsEqual();
    }

    private void SpawnMovingBusStreet(float streetDistance)
    {

        float movingBusMinimumDistance = ((spawnPositions[0].z - playerTransform.position.z) / PLAYER_FORWARD_SPEED) * MOVING_BUS_SPEED;
        List<Lane> movingBusLaneList = new List<Lane>();

        if (streetDistance > MIN_MOVING_BUS_EMPTINESS)
        {
            int movingBusLaneCount = Random.Range(1, 3);
            for (int i = 0; i < movingBusLaneCount; i++)
            {
                AddRandomLanes(movingBusLaneList);
                Vector3 spawnPos = spawnPositions[(int)movingBusLaneList[i]] + new Vector3(0f, 0f, movingBusMinimumDistance + Random.Range(MIN_MOVING_BUS_SPACE, MAX_MOVING_BUS_SPACE));
                objectPooler.SpawnFromPool(ObjectPooler.ObjectTag.movingBus, spawnPos, Quaternion.identity, "Moving Bus");
                spawnPositions[(int)movingBusLaneList[i]] += new Vector3(0f, 0f, streetDistance);
                unusedLanes.Remove(movingBusLaneList[i]);
            }

            foreach (Lane Lane in unusedLanes)
            {
                ObjectPooler.ObjectTag randomTag = ConvertIntToTag(Random.Range(0, EASY_OBSTACLE_COUNT));
                Vector3 spawnPos = spawnPositions[(int)Lane] + new Vector3(0f, 0f, Random.Range(MIN_EASY_OBSTACLE_DISTANCE, streetDistance - MIN_EASY_OBSTACLE_DISTANCE));

                objectPooler.SpawnFromPool(randomTag, spawnPos, Quaternion.identity, "Easy Obstacle at The Moving Bus Unused Lane");
                spawnPositions[(int)Lane] += new Vector3(0f, 0f, streetDistance);
                SpawnPickUps(spawnPos, randomTag, 1);
            }

            unusedLanes.Clear();
            unusedLanes.AddRange(allLanes);
        }
        else
        {
            SpawnEasyStreet(streetDistance);
        }
    }

    private void SpawnEasyStreet(float streetDistance)
    {
        for (int i = 0; i < 3; i++)
        {
            ObjectPooler.ObjectTag randomTag = ConvertIntToTag(Random.Range(0, EASY_OBSTACLE_COUNT));
            Vector3 pos = spawnPositions[i] + new Vector3(0f, 0f, Random.Range(MIN_EASY_OBSTACLE_DISTANCE, streetDistance - MIN_EASY_OBSTACLE_DISTANCE));

            objectPooler.SpawnFromPool(randomTag, pos, Quaternion.identity, "Empty Street Easy Obstacle");
            SpawnPickUps(pos, randomTag, 1);
            spawnPositions[i] += new Vector3(0f, 0f, streetDistance);
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
            SpawnMovingBusStreet(Random.Range(MIN_MOVING_BUS_STREET_DISTANCE, MAX_MOVING_BUS_STREET_DISTANCE));
        }
        else
        {
            SpawnEasyStreet(Random.Range(MIN_EASY_STREET_DISTANCE, MAX_EASY_STREET_DISTANCE));
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

    private List<Lane> AddRandomLanes(List<Lane> listOfLanes)
    {
        Lane Lane = (Lane)Random.Range(0, 3);
        if (!listOfLanes.Contains(Lane))
        {
            listOfLanes.Add(Lane);
            return listOfLanes;
        }
        else
        {
            return AddRandomLanes(listOfLanes);
        }
    }

    private ObjectPooler.ObjectTag ConvertIntToTag(int randomObject)
    {
        switch (randomObject)
        {
            case 0:
                return ObjectPooler.ObjectTag.lowWarningStand;
            case 1:
                return ObjectPooler.ObjectTag.warningStand;
            case 2:
                return ObjectPooler.ObjectTag.busWRamp;
            default:
                return ObjectPooler.ObjectTag.lowWarningStand;
        }
    }

    private void SpawnPickUps(Vector3 pos, ObjectPooler.ObjectTag tag, int objectCount = 1)
    {
        ObjectPooler.ObjectTag coinTag = ObjectPooler.ObjectTag.coin;
        ObjectPooler.ObjectTag powerUpTag = GenerateRandomPowerUp();

        switch (tag)
        {
            case ObjectPooler.ObjectTag.busWRamp:
                for (int j = 0; j < objectCount; j++)
                {
                    for (int i = 0; i < 2; i++)
                    {
                        objectPooler.SpawnFromPool(coinTag, pos + new Vector3(0f, PICK_UP_HEIGHT + BUS_HEIGHT, ((j * 2) + i) * DISTANCE_BTW_PICK_UPS + RAMP_OVER_BUS_LENGTH * busLength + COIN_SPAWN_OFFSET), Quaternion.identity);
                    }
                }

                if (powerUpTag == ObjectPooler.ObjectTag.empty || objectCount == 1) { break; }

                objectPooler.SpawnFromPool(powerUpTag, pos + new Vector3(0f, PICK_UP_HEIGHT + BUS_HEIGHT, (objectCount * 2 + 1) * DISTANCE_BTW_PICK_UPS + RAMP_OVER_BUS_LENGTH * busLength + COIN_SPAWN_OFFSET), Quaternion.identity);
                break;
            case ObjectPooler.ObjectTag.warningStand:
                for (int i = 0; i < PICK_UP_COUNT_ON_TRAFFIC; i++)
                {
                    if (i == 2 && powerUpTag != ObjectPooler.ObjectTag.empty)
                    {
                        objectPooler.SpawnFromPool(powerUpTag, pos + new Vector3(0f, PICK_UP_HEIGHT, -2 * DISTANCE_BTW_PICK_UPS + i * DISTANCE_BTW_PICK_UPS), Quaternion.identity);
                    }
                    else
                    {
                        objectPooler.SpawnFromPool(coinTag, pos + new Vector3(0f, PICK_UP_HEIGHT, -2 * DISTANCE_BTW_PICK_UPS + i * DISTANCE_BTW_PICK_UPS), Quaternion.identity);
                    }
                }
                break;
            case ObjectPooler.ObjectTag.lowWarningStand:
                for (int i = 0; i < PICK_UP_COUNT_ON_TRAFFIC; i++)
                {
                    objectPooler.SpawnFromPool(coinTag, pos + new Vector3(0f, PICK_UP_HEIGHT * (3 - Mathf.Abs(2 - i)), -2 * DISTANCE_BTW_PICK_UPS + i * DISTANCE_BTW_PICK_UPS), Quaternion.identity);
                }
                break;
        }
    }

    private ObjectPooler.ObjectTag GenerateRandomPowerUp()
    {
        int randomPowerUp = Random.Range(0, 6);
        switch (randomPowerUp)
        {
            case 0:
                return ObjectPooler.ObjectTag.redMagnetPowerUp;
            case 1:
                return ObjectPooler.ObjectTag.greenHighJumpPowerUp;
            case 2:
                return ObjectPooler.ObjectTag.blueFlyingPowerUp;
            default:
                return ObjectPooler.ObjectTag.empty;
        }
    }

    private void OnEnable()
    {
        Player.OnPlayerDeath += (()=> 
        {
            spawnPositions.Clear();
            stopSignSpawnPositions.Clear();
            unusedLanes.Clear();
            allLanes.Clear();
            isPlayerAlive = false;
        });
    }

    private void OnDisable()
    {
        Player.OnPlayerDeath += (() =>
        {
            spawnPositions.Clear();
            stopSignSpawnPositions.Clear();
            unusedLanes.Clear();
            allLanes.Clear();
            isPlayerAlive = false;
        });
    }

    private void Start()
    {
        objectPooler = ObjectPooler.Instance;
        busLength = objectPooler.zSizes[ObjectPooler.ObjectTag.bus];
    }

    private void Update()
    {
        if (!isPlayerAlive) { return; }

        SpawnStopSign();
        SpawnOthers();
    }
}
