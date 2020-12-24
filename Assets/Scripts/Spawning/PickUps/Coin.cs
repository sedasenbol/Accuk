using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Coin : MonoBehaviour
{
    private Transform playerTransform;
    [SerializeField] private AudioClip pickUpAudioClip;
    private readonly Vector3 animRotation = new Vector3(0f, 360f, 0f);
    private const float ANIM_DURATION = 4f;
    private Tween tween;
    private ObjectPooler objectPooler;
    private Transform cameraTransform;
    private Transform xform;
    private GameState gameState;
    private Vector3 currentVelocity;
    private bool isPlayerHitTheObstacle = false;

    private void StopMoving()
    {
        isPlayerHitTheObstacle = true;
    }

    private void Start()
    {
        cameraTransform = Camera.main.transform;
        playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
        xform = GetComponent<Transform>();
        objectPooler = ObjectPooler.Instance;
        gameState = FindObjectOfType<GameManager>().stateOfTheGame;
        currentVelocity = Vector3.zero;

        tween = transform.DORotate(animRotation, ANIM_DURATION, RotateMode.LocalAxisAdd);
        tween.SetLoops(-1, LoopType.Incremental);
        tween.Play();
    }

    private void Update()
    {
        if (isPlayerHitTheObstacle || !gameState.IsRedMagnetActive || Mathf.Abs(xform.position.z - playerTransform.position.z) > 5f) { return; }

        MoveTowardsPlayer();
    }

    private void MoveTowardsPlayer()
    {
        xform.SetParent(playerTransform);

        var zDistance = Mathf.Abs(xform.position.z - playerTransform.position.z);

        var localPositionX = (zDistance > 1.5f) ? xform.localPosition.x : Mathf.SmoothDamp(xform.localPosition.x, 0, ref currentVelocity.x, 0.05f);
        var localPositionY = (zDistance > 1.5f) ? xform.localPosition.y : Mathf.SmoothDamp(xform.localPosition.y, 0, ref currentVelocity.y, 0.05f);
        var localPositionZ = Mathf.SmoothDamp(xform.localPosition.z, 0, ref currentVelocity.z, 3f);
        
        xform.localPosition = new Vector3(localPositionX, localPositionY, localPositionZ);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer != 12) { return; }

        AudioSource.PlayClipAtPoint(pickUpAudioClip, cameraTransform.position);
        tween.Kill();
        objectPooler.DeactivateSpawnedObject(this.gameObject);
    }

    private void OnEnable()
    {
        Player.OnPlayerHitTheObstacle += StopMoving;
    }

    private void OnDisable()
    {
        Player.OnPlayerHitTheObstacle -= StopMoving;
    }

}
