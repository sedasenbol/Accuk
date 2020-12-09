using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    private const int STREET_NUMBER = 10;

    private readonly Vector3 streetLength = new Vector3(0f, 0f, 6f);
    private readonly Vector3 initialStreetPos = new Vector3(0f, 0f, -2f);

    [SerializeField] private Transform streetPrefab;
    [SerializeField] private Transform playerTransform;
    [SerializeField] private Transform stage;

    private Queue<Transform> streetQueue = new Queue<Transform>();

    private void InitialStreetSpawn()
    {
        for (int i = 0; i < STREET_NUMBER; i++)
        {
            streetQueue.Enqueue(Instantiate(streetPrefab, initialStreetPos + i * streetLength, Quaternion.identity, stage));
        }
    }

    private void StreetChangePosition()
    {
        if (playerTransform.position.z < streetQueue.Peek().position.z + streetLength.z) { return; }
        
        streetQueue.Peek().position += STREET_NUMBER * streetLength;
        streetQueue.Enqueue(streetQueue.Dequeue());
    }

    private void Start()
    {
        InitialStreetSpawn();
    }

    private void Update()
    {
        StreetChangePosition();
    }
}
