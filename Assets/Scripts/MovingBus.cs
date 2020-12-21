using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingBus : MonoBehaviour
{
    private Vector3 SPEED = new Vector3(0f, 0f, -6f);
    private Rigidbody rb;
    private bool isPlayerDead = false;

    private void StandStill()
    {
        rb.constraints |= RigidbodyConstraints.FreezeAll;
    }

    private void OnEnable()
    {
        Player.OnPlayerDeath += StandStill;
    }

    private void OnDisable()
    {
        Player.OnPlayerDeath -= StandStill;
    }

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        if (isPlayerDead) { return; }
           
        rb.velocity = SPEED;
    }
}
