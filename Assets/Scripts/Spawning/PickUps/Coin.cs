using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Coin : MonoBehaviour
{
    [SerializeField] private Transform playerTransform;
    [SerializeField] private AudioClip pickUpAudioClip;
    private readonly Vector3 animRotation = new Vector3(0f, 360f, 0f);
    private const float animDuration = 3f;
    private const float SPAWN_OFFSET = 5f;
    private Tween tween;
    private ObjectPooler objectPooler;
    private Transform cameraTransform;
    private Transform xform;
    private bool isMagnetActive = false;
    private const float coinSpeed = 0.1f;

    private void OnLeftBehindThePlayer()
    {
        if (playerTransform.position.z < xform.position.z + SPAWN_OFFSET) { return; }

        objectPooler.DeactivateSpawnedObject(this.gameObject);

    }

    private void Start()
    {
        cameraTransform = Camera.main.transform;
        xform = GetComponent<Transform>();
        objectPooler = ObjectPooler.Instance;

        tween = transform.DORotate(animRotation, animDuration, RotateMode.LocalAxisAdd);
        tween.SetLoops(-1, LoopType.Incremental);
        tween.Play();
    }

    private void Update()
    {
        OnLeftBehindThePlayer();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == 12)
        {
            AudioSource.PlayClipAtPoint(pickUpAudioClip, cameraTransform.position);
            tween.Kill();
            objectPooler.DeactivateSpawnedObject(this.gameObject);
        }
    }
}
