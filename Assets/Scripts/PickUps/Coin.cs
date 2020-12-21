using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Coin : MonoBehaviour
{
    private readonly Vector3 animRotation = new Vector3(0f, 360f, 0f);
    private const float animDuration = 3f;
    private Tween tween;

    private void Start()
    {
        tween = transform.DORotate(animRotation, animDuration, RotateMode.LocalAxisAdd);
        tween.SetLoops(-1, LoopType.Incremental);
        tween.Play();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == 12)
        {
            tween.Kill();
            Destroy(this.gameObject);
        }
    }
}
