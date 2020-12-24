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
        isPlayerDead = true;
        rb.constraints |= RigidbodyConstraints.FreezeAll;
    }

    private void OnEnable()
    {
        Player.OnPlayerHitTheObstacle += StandStill;
    }

    private void OnDisable()
    {
        Player.OnPlayerHitTheObstacle -= StandStill;
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
