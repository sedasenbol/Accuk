using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.EventSystems;
using System;

public class Player : MonoBehaviour
{
    public static event Action OnPlayerDeath;
    public static event Action OnCoinPickUp;
    public static event Action OnRedMagnetPickUp;
    public static event Action OnGreenHighJumpPickUp;
    public static event Action OnBlueDoubleScorePickUp;
    public static event Action OnPlayerHitTheObstacle;
    public static event Action OnDeactivateDoubleTap;

    [SerializeField] private Material redMagnetMaterial;
    [SerializeField] private Material normalMaterial;
    [SerializeField] private Material greenHighJumpMaterial;
    [SerializeField] private Material blueDoubleScoreMaterial;
    [SerializeField] private Material doubleTapMaterial;

    private const float INITIAL_ROTATION_TIME = 1f;
    private readonly Vector3 initialRotation = new Vector3(0f, 0f, 0f);
    
    private const float FORWARD_SPEED = 7f;
    private const float ZERO_VELOCITY_ERROR = 0.01f;

    private readonly Vector3 slideRotation = new Vector3(-80f, 0f, 0f);
    private const float SLIDING_TIME = 0.5f;

    private const float LANE_SWITCH_TIME = 0.25f;
    private const float LANE_WIDTH = 0.65f;
    private const float LANE_ERROR_MARGIN = 0.01f;
    private readonly Vector3 switchLaneRotation = new Vector3(0f, 0f, 20f);

    private readonly Vector3 jumpForce = new Vector3(0f, 65f, 0f);
    private readonly Vector3 highJumpForce = new Vector3(0f, 100f, 0f);
    private readonly Vector3 pullDownForce = new Vector3(0f, -10f, 0f);
    private readonly Vector3 jumpGravity = new Vector3(0f, -9.81f, 0f);
    private readonly Vector3 normalGravity = new Vector3(0f, -100f, 0f);
    private const float JUMP_ROTATION_TIME = 0.5f;
    private readonly Vector3 jumpRotationX = new Vector3(360f, 0f, 0f);
    private readonly Vector3 jumpRotationY = new Vector3(0f, 360f, 0f);

    private const float DEATH_ROTATION_TIME = 0.1f;
    private readonly Vector3 deathXRotation = new Vector3(-90f, 0f, 0f);
    private const float MAX_DEATH_Y_ROTATION = 30f;

    private Quaternion rampRotation = Quaternion.Euler(-62f, 0f, 0f);

    private Rigidbody rb;
    private Animator anim;
    private SkinnedMeshRenderer skinnedMeshRenderer;
    private Transform xform;

    private bool isAlive = true;
    private bool isRotating = true;
    private bool ishighJumpPowerUpActive = false;
    private bool isJumping = false;
    private bool isSliding = false;
    private bool isSwitchingLanes = false;
    private bool isGrounded = true;
    private bool isOnSlope = false;
    private bool isDoubleTapActive = false;
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
        if (!isOnSlope & !isJumping)
        {
            rb.velocity = new Vector3(rb.velocity.x, 0, FORWARD_SPEED);
        }
        else if (isJumping)
        {
            rb.velocity = new Vector3(rb.velocity.x, rb.velocity.y, FORWARD_SPEED);
        }
        else
        {
            rb.velocity = new Vector3(rb.velocity.x, FORWARD_SPEED * -Mathf.Sin(rampRotation.x) / Mathf.Cos(rampRotation.x), FORWARD_SPEED);
        }
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
        if (isSwitchingLanes) { return; }

        float xPosition = rb.position.x;

        UpdateLane(xPosition);

        float finalXPosition = DetermineFinalXPosition(isRight, xPosition);

        if (finalXPosition != xPosition)
        {
            rb.constraints &= ~RigidbodyConstraints.FreezePositionX;

            if (isRotating) 
            {
                isSwitchingLanes = true;
                rb.DOMoveX(finalXPosition, LANE_SWITCH_TIME).OnComplete(()=> 
                {
                    isSwitchingLanes = false;
                    rb.constraints |= RigidbodyConstraints.FreezePositionX;
                }); 
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
                rb.constraints |= RigidbodyConstraints.FreezePositionX;
            });
        }
        else
        {
            Die();
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
                lane = Lane.Middle;
                return 0;
            }
            else if (lane == Lane.Middle)
            {
                lane = Lane.Right;
                return LANE_WIDTH;
            }
        }
        else
        {
            if (lane == Lane.Right)
            {
                lane = Lane.Middle;
                return 0;
            }
            else if (lane == Lane.Middle)
            {
                lane = Lane.Left;
                return -LANE_WIDTH;
            }
        }
        return xPosition;
    }

    private void Jump()
    {
        if (!isGrounded || isSliding || isRotating || isJumping) { return; }
        isJumping = true;

        Physics.gravity = jumpGravity;

        /*rb.useGravity = false;
        Sequence sequence = DOTween.Sequence();
        sequence.Append(rb.DOMoveY(1, 1)).OnComplete(() => rb.useGravity = true);*/
        
        if (ishighJumpPowerUpActive)
        {
            rb.AddForce(highJumpForce, ForceMode.Impulse);
        }
        else
        {
            rb.AddForce(jumpForce, ForceMode.Impulse);
        }
        
        anim.SetBool("isJumping", true);
    }

    private void RotateWhileJumping()
    {
        if (rb.position.y < 0.75f && !ishighJumpPowerUpActive ) { return; }
        if (rb.position.y < 1.2f && ishighJumpPowerUpActive) { return; }

        int randomJumpRotation = 1;// UnityEngine.Random.Range(0, 2);// JUMP_ROTATION_RANDOM_COUNT);
        switch (randomJumpRotation)
        {
            case 0:
                isRotating = true;
                //rb.constraints &= ~RigidbodyConstraints.FreezeRotationX;

                Tween tween0 = rb.DORotate(jumpRotationX, JUMP_ROTATION_TIME, RotateMode.FastBeyond360);
                tween0.SetRelative();//.SetEase(Ease.Linear);
                tween0.OnComplete(() =>
                {
                    //rb.constraints |= RigidbodyConstraints.FreezeRotationX;
                    isRotating = false;
                });
                break;
            case 1:
                isRotating = true;

                Tween tween1 = rb.DORotate(jumpRotationY, JUMP_ROTATION_TIME, RotateMode.FastBeyond360);
                tween1.SetRelative();//.SetEase(Ease.Linear);
                tween1.OnComplete(() => isRotating = false);
                break;
        }
    }

    private void PullDown()
    {
        rb.AddForce(pullDownForce, ForceMode.Impulse);
    }

    private void Slide()
    {
        if (isJumping || !isGrounded) { return; }

        isSliding = true;
        isRotating = true;
        
        anim.SetBool("isSliding", true);

        Sequence sequence = DOTween.Sequence();
        sequence.Append(rb.DORotate(slideRotation, SLIDING_TIME));
        sequence.AppendInterval(0.15f);
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

    private void HandleDoubleTapCollision()
    {
        isDoubleTapActive = false;
        Collider[] hitColliders = Physics.OverlapSphere(xform.position, 10f);
        foreach (var collider in hitColliders)
        {
            if (new List<int> { 9, 11, 13, 14, 15, 16, 17, 18}.Contains(collider.gameObject.layer))
            {
                collider.gameObject.SetActive(false);
            }
        }
        OnDeactivateDoubleTap?.Invoke();
    }

    private void Die()
    {
        if (isDoubleTapActive)
        {
            HandleDoubleTapCollision();
            return;
        }
        OnPlayerHitTheObstacle?.Invoke();
        Physics.gravity = new Vector3(0f, -1000f, 0f);
        isAlive = false;
        anim.SetBool("isDead", true);

        DOTween.KillAll();
        float randomYRotation = UnityEngine.Random.Range(-MAX_DEATH_Y_ROTATION, MAX_DEATH_Y_ROTATION);
        Vector3 deathRotation = deathXRotation + new Vector3(0f, randomYRotation, 0f);
        rb.DORotate(deathRotation, DEATH_ROTATION_TIME).OnComplete(()=> 
        {
            rb.constraints = RigidbodyConstraints.FreezeAll;
            OnPlayerDeath?.Invoke();
        });
        
    }

    private bool CheckZeroVelocity()
    {
        float forwardVelocity = rb.velocity.z;
        if (forwardVelocity < ZERO_VELOCITY_ERROR)
        {
            return true;
        }
        return false;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (new List<int> { 8, 9, 11 }.Contains(collision.gameObject.layer))
        {
            isGrounded = true;
            isJumping = false;
            Physics.gravity = normalGravity;
            anim.SetBool("isJumping", false);
        }
        if (collision.gameObject.layer == 9)
        {
            isOnSlope = true;
        }
        if (new List<int> { 13, 18 }.Contains(collision.collider.gameObject.layer))
        {
            Die();
        }
    }

    private void OnCollisionStay(Collision collision)
    {
        if (new List<int> { 8, 9, 11 }.Contains(collision.gameObject.layer))
        {
            isGrounded = true;
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (new List<int> { 8, 9, 11 }.Contains(collision.gameObject.layer))
        {
            isGrounded = false;
        }
        if (collision.gameObject.layer == 9)
        {
            isOnSlope = false;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == 14)
        {
            OnCoinPickUp?.Invoke();
        }
        else if (other.gameObject.layer == 15)
        {
            skinnedMeshRenderer.material = redMagnetMaterial;
            OnRedMagnetPickUp?.Invoke();
        }
        else if (other.gameObject.layer == 16)
        {
            skinnedMeshRenderer.material = greenHighJumpMaterial;
            OnGreenHighJumpPickUp?.Invoke();
            ishighJumpPowerUpActive = true;
        }
        else if (other.gameObject.layer == 17)
        {
            skinnedMeshRenderer.material = blueDoubleScoreMaterial;
            OnBlueDoubleScorePickUp?.Invoke();
        }
    }

    private void DeactivateGreenHighJump()
    {
        ishighJumpPowerUpActive = false;
        if (!skinnedMeshRenderer.material == greenHighJumpMaterial) { return; }
            
        skinnedMeshRenderer.material = normalMaterial;
    }

    private void DeactivateRedMagnet()
    {
        if (!skinnedMeshRenderer.material == redMagnetMaterial) { return; }
        
        skinnedMeshRenderer.material = normalMaterial;
    }

    private void DeactivateBlueDoubleScore()
    {
        if (!skinnedMeshRenderer.material == blueDoubleScoreMaterial) { return; }

        skinnedMeshRenderer.material = normalMaterial;
    }

    private void ActivateDoubleTap()
    {
        skinnedMeshRenderer.material = doubleTapMaterial;
        isDoubleTapActive = true;
    }

    private void DeactivateDoubleTap()
    {
        skinnedMeshRenderer.material = normalMaterial;
        isDoubleTapActive = false;
    }

    private void OnEnable()
    {
        TouchController.OnEndDragMovement += DetermineDragDirection;
        LevelManager.OnDeactivateGreenHighJump += DeactivateGreenHighJump;
        LevelManager.OnDeactivateRedMagnet += DeactivateRedMagnet;
        LevelManager.OnDeactivateBlueDoubleScore += DeactivateBlueDoubleScore;
        LevelManager.OnActivateDoubleTap += ActivateDoubleTap;
        LevelManager.OnDeactivateDoubleTap += DeactivateDoubleTap;
    }

    private void OnDisable()
    {
        TouchController.OnEndDragMovement -= DetermineDragDirection;
        LevelManager.OnDeactivateGreenHighJump -= DeactivateGreenHighJump;
        LevelManager.OnDeactivateRedMagnet -= DeactivateRedMagnet;
        LevelManager.OnDeactivateBlueDoubleScore -= DeactivateBlueDoubleScore;
        LevelManager.OnActivateDoubleTap -= ActivateDoubleTap;
        LevelManager.OnDeactivateDoubleTap -= DeactivateDoubleTap;
    }

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        anim = GetComponent<Animator>();
        skinnedMeshRenderer = GetComponentInChildren<SkinnedMeshRenderer>();
        xform = GetComponent<Transform>();

        InitialRotation();
    }

    private void Update()
    {
        if (!isAlive) { return; }
        if (isJumping && !isRotating)
        {
            RotateWhileJumping();
        }
    }

    private void FixedUpdate()
    {
        if (!isAlive) { return; }
        MoveForward();
        //if (CheckZeroVelocity())
        {
          //  Die();
        }
    }
}
