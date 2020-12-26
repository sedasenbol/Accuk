using UnityEngine;
using DG.Tweening;

public class Coin : MonoBehaviour
{
    [SerializeField] private AudioClip pickUpAudioClip;

    private const float ANIM_DURATION = 4f;
    private readonly Vector3 animRotation = new Vector3(0f, 360f, 0f);

    private const float MAGNET_EFFECT_START_DISTANCE_Z = 5f;
    private const float MAGNET_EFFECT_START_DISTANCE_XY = 1.5f;
    private const float SMOOTH_TIME_XY = 0.05f;
    private const float SMOOTH_TIME_Z = 3f;

    private Tween tween;
    private Transform cameraTransform;
    private Transform playerTransform;
    private Transform xform;
    private ObjectPooler objectPooler;
    private LevelState levelState;

    private Vector3 currentVelocity;
    private bool isPlayerHitTheObstacle = false;

    private void StartAnimation()
    {
        tween = transform.DORotate(animRotation, ANIM_DURATION, RotateMode.LocalAxisAdd);
        tween.SetLoops(-1, LoopType.Incremental);
        tween.Play();
    }

    private void MoveTowardsPlayer()
    {
        xform.SetParent(playerTransform);

        var zDistance = Mathf.Abs(xform.position.z - playerTransform.position.z);

        var localPositionX = (zDistance > MAGNET_EFFECT_START_DISTANCE_XY) ? xform.localPosition.x : Mathf.SmoothDamp(xform.localPosition.x, 0, ref currentVelocity.x, SMOOTH_TIME_XY);
        var localPositionY = (zDistance > MAGNET_EFFECT_START_DISTANCE_XY) ? xform.localPosition.y : Mathf.SmoothDamp(xform.localPosition.y, 0, ref currentVelocity.y, SMOOTH_TIME_XY);
        var localPositionZ = Mathf.SmoothDamp(xform.localPosition.z, 0, ref currentVelocity.z, SMOOTH_TIME_Z);
        
        xform.localPosition = new Vector3(localPositionX, localPositionY, localPositionZ);
    }

    private void StopMoving()
    {
        isPlayerHitTheObstacle = true;
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

    private void Start()
    {
        cameraTransform = Camera.main.transform;
        playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
        xform = GetComponent<Transform>();

        objectPooler = ObjectPooler.Instance;
        levelState = FindObjectOfType<LevelManager>().StateOfTheLevel;

        currentVelocity = Vector3.zero;

        StartAnimation();
    }

    private void Update()
    {
        if (isPlayerHitTheObstacle || !levelState.IsRedMagnetActive || Mathf.Abs(xform.position.z - playerTransform.position.z) > MAGNET_EFFECT_START_DISTANCE_Z) { return; }

        MoveTowardsPlayer();
    }

}
