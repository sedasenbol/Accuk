using UnityEngine;
using UnityEngine.EventSystems;
using System;

public class TouchController : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerDownHandler
{
    public static event Action<PointerEventData> OnEndDragMovement;
    public static event Action OnDoubleTapMovement;

    private float tapped = 0;
    private float tapTime = 0;
    private float tapDelay = 0.5f;

    public void OnBeginDrag(PointerEventData eventData) { }
    public void OnDrag(PointerEventData eventData) { }
    public void OnEndDrag(PointerEventData eventData)
    {
        OnEndDragMovement?.Invoke(eventData);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        tapped++;
        
        if (tapped == 1)
        {
            tapTime = Time.time;
        }
        else if (tapped > 1 && Time.time - tapTime < tapDelay)
        {
            tapped = 0;
            tapTime = 0;
            OnDoubleTapMovement?.Invoke();
        }
        else if (tapped > 2 || Time.time - tapTime > 1 ) 
        { 
            tapped = 0; 
        }
    }
}