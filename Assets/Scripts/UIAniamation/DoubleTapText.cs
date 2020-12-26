using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class DoubleTapText : MonoBehaviour
{
    private const float SCALING_TIME = 2f;
    private const float MAX_SCALING_SIZE = 1.75f;
    private const float MIN_SCALING_SCALE = 0.25f;

    private Transform xform;
    private Sequence sequence;

    private void OnEnable()
    {
        Player.OnPlayerDeath += () => this.gameObject.SetActive(false);
        
        sequence = DOTween.Sequence();
        sequence.Append(xform.DOScale(MAX_SCALING_SIZE, SCALING_TIME));
        sequence.Append(xform.DOScale(MIN_SCALING_SCALE, SCALING_TIME));
        sequence.OnComplete(()=> this.gameObject.SetActive(false));
    }

    private void OnDisable()
    {
        Player.OnPlayerDeath -= () => this.gameObject.SetActive(false);
    }

    private void Awake()
    {
        xform = transform;
    }
}
