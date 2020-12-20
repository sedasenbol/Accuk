using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingBus : MonoBehaviour
{
    private readonly Vector3 SPEED = new Vector3(0f, 0f, -5f);
    private Rigidbody rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        rb.velocity = SPEED;
    }
}
