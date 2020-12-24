using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class TopRun : MonoBehaviour
{
    private const float SCALING_TIME = 0.5f;
    private const float SCALING_SIZE = 4f;
    private const float INITIAL_SCALE = 3.5f;
    private Transform xform;
    private Sequence sequence;

    private void OnEnable()
    {
        sequence = DOTween.Sequence();
        sequence.Append(xform.DOScale(SCALING_SIZE, SCALING_TIME));
        sequence.Append(xform.DOScale(INITIAL_SCALE, SCALING_TIME));
        sequence.SetLoops(-1);
    }

    private void Awake()
    {
        xform = transform;
    }

}
