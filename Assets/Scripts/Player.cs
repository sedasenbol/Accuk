using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.EventSystems;
using System;

public class Player : MonoBehaviour
{
    public static event Action OnPlayerDeath;

    private const float INITIAL_ROTATION_TIME = 1f;
    private readonly Vector3 initialRotation = new Vector3(0f, 0f, 0f);
    
    private const float FORWARD_SPEED = 5f;
    private const float ZERO_VELOCITY_ERROR_MARGIN = 0.01f;

    private readonly Vector3 slideRotation = new Vector3(-80f, 0f, 0f);
    private const float SLIDING_TIME = 0.5f;

    private const float LANE_SWITCH_TIME = 0.5f;
    private const float LANE_WIDTH = 0.55f;
    private const float LANE_ERROR_MARGIN = 0.01f;
    private readonly Vector3 switchLaneRotation = new Vector3(0f, 0f, 20f);

    private readonly Vector3 jumpForce = new Vector3(0f, 5f, 0f);
    private readonly Vector3 pullDownForce = new Vector3(0f, -10f, 0f);
    private const float JUMP_ROTATION_TIME = 0.5f;
    private const int JUMP_ROTATION_RANDOM_COUNT = 5;
    private readonly Vector3 jumpRotationX = new Vector3(360f, 0f, 0f);
    private readonly Vector3 jumpRotationY = new Vector3(0f, 360f, 0f);

    private const float DEATH_ROTATION_TIME = 1f;
    private readonly Vector3 deathXRotation = new Vector3(-90f, 0f, 0f);
    private const float MAX_DEATH_Y_ROTATION = 30f;

    private Rigidbody rb;
    private Animator anim;

    private bool isAlive = true;
    private bool isRotating = true;
    private bool isJumping = false;
    private bool isSliding = false;
    private bool isSwitchingLanes = false;
    private bool isGrounded = true;
    private Lane lane = Lane.Middle;
    private enum Lane
    {
        Left,
        Middle,
        Right,
        InBetween,
    }

    private void MoveForward()
    {
        rb.velocity = new Vector3(rb.velocity.x, rb.velocity.y, FORWARD_SPEED);
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
        else if (isJumping)
        {
            PullDown();
        }
        else
        {
            Slide();
        }
    }
    
    private void SwitchLane(bool isRight)
    {
        if (isSwitchingLanes || isSliding) { return; }

        float xPosition = rb.position.x;

        UpdateLane(xPosition);

        float finalXPosition = DetermineFinalXPosition(isRight, xPosition);

        if (finalXPosition != xPosition)
        {
            if (isRotating) 
            {
                isSwitchingLanes = true;
                rb.DOMoveX(finalXPosition, LANE_SWITCH_TIME).OnComplete(()=> isSwitchingLanes = false); 
                return; 
            }

            isSwitchingLanes = true;
            isRotating = true;

            float direction = Mathf.Sign(xPosition - finalXPosition);

            Sequence sequence = DOTween.Sequence();
            sequence.Append(rb.DOMoveX(finalXPosition, LANE_SWITCH_TIME));
            sequence.Join(rb.DORotate(switchLaneRotation * direction, 3 * LANE_SWITCH_TIME / 4));
            sequence.Append(rb.DORotate(initialRotation, LANE_SWITCH_TIME / 4));
            sequence.OnComplete(() =>
            {
                isSwitchingLanes = false;
                isRotating = false;
            });
        }

    }

    private void UpdateLane(float xPosition)
    {
        if (xPosition > -LANE_ERROR_MARGIN && xPosition < LANE_ERROR_MARGIN)
        {
            lane = Lane.Middle;
        }
        else if (xPosition > LANE_WIDTH - LANE_ERROR_MARGIN && xPosition < LANE_WIDTH + LANE_ERROR_MARGIN)
        {
            lane = Lane.Right;
        }
        else if (xPosition > -LANE_WIDTH - LANE_ERROR_MARGIN && xPosition < -LANE_WIDTH + LANE_ERROR_MARGIN)
        {
            lane = Lane.Left;
        }
    }

    private float DetermineFinalXPosition(bool isRight, float xPosition)
    {
        if (isRight)
        {
            if (lane == Lane.Left)
            {
                return 0;
            }
            else if (lane == Lane.Middle)
            {
                return LANE_WIDTH;
            }
        }
        else
        {
            if (lane == Lane.Right)
            {
                return 0;
            }
            else if (lane == Lane.Middle)
            {
                return -LANE_WIDTH;
            }
        }
        return xPosition;
    }

    private void Jump()
    {
        if (!isGrounded || isSliding || isRotating || isJumping) { return; }

        isJumping = true;

        rb.useGravity = false;
        Sequence sequence = DOTween.Sequence();
        sequence.Append(rb.DOMoveY(1, 1)).OnComplete(() => rb.useGravity = true);
        

        //rb.AddForce(jumpForce, ForceMode.Impulse);
        anim.SetBool("isJumping", true);
    }

    private void RotateWhileJumping()
    {
        if (rb.position.y < 0.8f || rb.useGravity == true) { return; }

        int randomJumpRotation = UnityEngine.Random.Range(0, JUMP_ROTATION_RANDOM_COUNT);
        switch (0)
        {
            case 0:
                isRotating = true;
                rb.constraints &= ~RigidbodyConstraints.FreezeRotationX;

                Tween tween0 = rb.DORotate(jumpRotationX, JUMP_ROTATION_TIME, RotateMode.FastBeyond360);
                tween0.SetRelative().SetEase(Ease.Linear);
                tween0.OnComplete(() =>
                {
                    rb.constraints |= RigidbodyConstraints.FreezeRotationX;
                    isRotating = false;
                });
                break;
            case 1:
                isRotating = true;

                Tween tween1 = rb.DORotate(jumpRotationY, JUMP_ROTATION_TIME, RotateMode.FastBeyond360);
                tween1.SetRelative().SetEase(Ease.Linear);
                tween1.OnComplete(() => isRotating = false);
                break;
        }
    }

    private void Slide()
    {
        if (isSliding || isJumping || isRotating || !isGrounded) { return; }

        isSliding = true;
        isRotating = true;
        
        anim.SetBool("isSliding", true);

        Sequence sequence = DOTween.Sequence();
        sequence.Append(rb.DORotate(slideRotation, SLIDING_TIME));
        sequence.Append(rb.DORotate(initialRotation, SLIDING_TIME));
        sequence.SetEase(Ease.Linear);
        sequence.OnComplete(() => 
        {
            isSliding = false;
            isRotating = false;
            anim.SetBool("isSliding", false);
        });
        sequence.Play();
    }

    private void PullDown()
    {
        rb.AddForce(pullDownForce, ForceMode.Impulse);
    }

    private void InitialRotation()
    {
        Tween tween = rb.DORotate(initialRotation, INITIAL_ROTATION_TIME);
        tween.SetEase(Ease.Linear);
        tween.OnComplete(() => 
        {
            rb.constraints |= RigidbodyConstraints.FreezeRotationY;
            isRotating = false;
        });
        tween.Play();

    }

    private void Die()
    {
        isAlive = false;
        anim.SetBool("isDead", true);

        DOTween.KillAll();
        float randomYRotation = UnityEngine.Random.Range(-MAX_DEATH_Y_ROTATION, MAX_DEATH_Y_ROTATION);
        Vector3 deathRotation = deathXRotation + new Vector3(0f, randomYRotation, 0f);
        rb.DORotate(deathRotation, DEATH_ROTATION_TIME);
        
        OnPlayerDeath?.Invoke();
    }

    private bool CheckZeroVelocity()
    {
        float forwardVelocity = rb.velocity.z;
        if (forwardVelocity > - ZERO_VELOCITY_ERROR_MARGIN && forwardVelocity < ZERO_VELOCITY_ERROR_MARGIN)
        {
            return true;
        }
        return false;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.layer == 8)
        {
            isGrounded = true;
            isJumping = false;
            anim.SetBool("isJumping", false);
        }
        else if (collision.gameObject.layer == 9)
        {
            isGrounded = true;
            isJumping = false;
            anim.SetBool("isJumping", false);
        }
    }

    private void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.layer == 8)
        {
            isGrounded = true;
        }
        else if (collision.gameObject.layer == 9 && CheckZeroVelocity())
        {
            Die();
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.layer == 8)
        {
            isGrounded = false;
        }
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
        anim = GetComponent<Animator>();

        InitialRotation();
    }

    private void Update()
    {
        if (!isAlive) { return; }
        MoveForward();
        if (isJumping && !isRotating)
        {
            RotateWhileJumping();
        }
    }
}
