using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.EventSystems;

public class Player : MonoBehaviour
{
    private const float INITIAL_ROTATION_TIME = 2f;
    private readonly Vector3 initialRotation = new Vector3(0f, 0f, 0f);

    private readonly Vector3 slideRotation = new Vector3(-80f, 0f, 0f);
    private const float SLIDING_TIME = 1f;

    private const float SPEED = 5f;

    private const float LANE_SWITCH_TIME = 0.5f;
    private const float LANE_WIDTH = 0.55f;
    private const float LANE_SWITCH_MARGIN = 0.01f;

    private readonly Vector3 jumpForce = new Vector3(0f, 2000f, 0f);
    private readonly Vector3 jumpRotationX = new Vector3(360f, 0f, 0f);
    private readonly Vector3 jumpRotationY = new Vector3(0f, 360f, 0f);
    private const float JUMP_ROTATION_TIME = 1f;

    private Rigidbody rb;
    private Animator anim;

    private bool isAlive = true;
    private Lane lane = Lane.Middle;
    private enum Lane
    {
        Left,
        Middle,
        Right,
    }

    private void MoveForward()
    {
        rb.velocity = Vector3.forward * SPEED;
    }

    private void DetermineDragDirection(PointerEventData eventData)
    {
        if (!isAlive) { return; }

        Vector3 dragDirection = (eventData.position - eventData.pressPosition).normalized;

        if (Mathf.Abs(dragDirection.x) > Mathf.Abs(dragDirection.y))
        {
            SwitchLane(dragDirection.x > 0);
        }
        else if (dragDirection.y > 0)
        {
            Jump();
        }
        else
        {
            Slide();
        }
    }
    
    private void SwitchLane(bool isRight)
    {
        float xPosition = rb.position.x;

        if (xPosition > -LANE_SWITCH_MARGIN && xPosition < LANE_SWITCH_MARGIN)
        {
            lane = Lane.Middle;
        }
        else if (xPosition > LANE_WIDTH - LANE_SWITCH_MARGIN && xPosition < LANE_WIDTH + LANE_SWITCH_MARGIN)
        {
            lane = Lane.Right;
        }
        else if (xPosition > -LANE_WIDTH - LANE_SWITCH_MARGIN && xPosition < -LANE_WIDTH + LANE_SWITCH_MARGIN)
        {
            lane = Lane.Left;
        }

        if (isRight)
        {
            if (lane == Lane.Left)
            {
                rb.DOMoveX(0, LANE_SWITCH_TIME); //.OnComplete(() => lane = Lane.Middle);
            }
            else if (lane == Lane.Middle)
            {
                rb.DOMoveX(LANE_WIDTH, LANE_SWITCH_TIME); //.OnComplete(() => lane = Lane.Right);
            }             
        }
        else
        {
            if (lane == Lane.Right)
            {
                rb.DOMoveX(0, LANE_SWITCH_TIME); //.OnComplete(() => lane = Lane.Middle);
            }
            else if (lane == Lane.Middle)
            {
                rb.DOMoveX(-LANE_WIDTH, LANE_SWITCH_TIME); //.OnComplete(() => lane = Lane.Left);
            }
        }
    }

    private void Jump()
    {
        rb.AddForce(jumpForce, ForceMode.Force);
        anim.SetTrigger("Jump");

        int randomRotation = Random.Range(0, 5);
        switch (0)
        {
            case 0:
                rb.constraints &= ~RigidbodyConstraints.FreezeRotationX;
                
                Sequence sequence = DOTween.Sequence();
                sequence.Append(rb.DORotate(jumpRotationX, JUMP_ROTATION_TIME, RotateMode.FastBeyond360)).SetRelative().SetEase(Ease.Linear);
                sequence.OnComplete(() => rb.constraints |= RigidbodyConstraints.FreezeRotationX);
                break;
        }
    }

    private void Slide()
    {
        rb.constraints &= ~RigidbodyConstraints.FreezeRotationX;
        rb.constraints |= RigidbodyConstraints.FreezePositionX;

        Sequence sequence = DOTween.Sequence();
        sequence.Append(rb.DORotate(slideRotation, SLIDING_TIME, RotateMode.Fast));
        sequence.Append(rb.DORotate(initialRotation, SLIDING_TIME, RotateMode.Fast));
        sequence.OnComplete(() => rb.constraints = ~RigidbodyConstraints.FreezePositionX & (rb.constraints | RigidbodyConstraints.FreezeRotationX));

        anim.SetTrigger("Slide");
    }

    private void OnEnable()
    {
        TouchController.OnEndDragMovement += DetermineDragDirection;
    }

    private void OnDisable()
    {
        TouchController.OnEndDragMovement -= DetermineDragDirection;
    }

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.DORotate(initialRotation, INITIAL_ROTATION_TIME).OnComplete(() => rb.constraints |= RigidbodyConstraints.FreezeRotationY);

        anim = GetComponent<Animator>();
    }

    private void Update()
    {
        if (!isAlive) { return; }
        MoveForward();
    }
}
