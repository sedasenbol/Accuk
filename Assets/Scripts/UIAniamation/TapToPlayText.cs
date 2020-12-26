using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class TapToPlayText : MonoBehaviour
{
    private const float ROTATION_TIME = 0.5f;
    private const float ROTATION_DIVISOR = 2f;

    private readonly Vector3 startingRotation = new Vector3(0, 0, 30);
    private readonly Vector3 rotationAngle = new Vector3(0, 0, 5);

    private Transform xform;
    private Sequence sequence;

    private void OnEnable()
    {
        sequence = DOTween.Sequence();
        sequence.Append(xform.DORotate(startingRotation - rotationAngle, ROTATION_TIME));
        sequence.Append(xform.DORotate(startingRotation + rotationAngle / ROTATION_DIVISOR, ROTATION_TIME / ROTATION_DIVISOR));
        sequence.Append(xform.DORotate(startingRotation - rotationAngle / Mathf.Pow(ROTATION_DIVISOR, 2), ROTATION_TIME / Mathf.Pow(ROTATION_DIVISOR, 2)));
        sequence.Append(xform.DORotate(startingRotation, ROTATION_TIME / Mathf.Pow(ROTATION_DIVISOR, 3)));
        sequence.AppendInterval(2);
        sequence.SetLoops(-1);
    }

    private void Awake()
    {
        xform = transform;
    }
}
