using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class PowerUp : MonoBehaviour
{
    [SerializeField] private AudioClip pickUpAudioClip;
    private readonly Vector3 animRotation = new Vector3(0f, 360f, 0f);
    private const float animDuration = 4f;
    private Tween tween;
    private ObjectPooler objectPooler;
    private Transform cameraTransform;

    private void Start()
    {
        cameraTransform = Camera.main.transform;
        objectPooler = ObjectPooler.Instance;

        tween = transform.DORotate(animRotation, animDuration, RotateMode.LocalAxisAdd);
        tween.SetLoops(-1, LoopType.Incremental);
        tween.Play();
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
