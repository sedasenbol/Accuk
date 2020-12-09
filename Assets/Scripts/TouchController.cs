using UnityEngine;
using UnityEngine.EventSystems;
using System;

public class TouchController : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerDownHandler
{
    public static event Action<PointerEventData> OnBeginDragMovement;
    public static event Action<PointerEventData> OnDragMovement;
    public static event Action<PointerEventData> OnEndDragMovement;
    public static event Action OnPointerDownMovement;

    public void OnBeginDrag(PointerEventData eventData)
    {
        OnBeginDragMovement?.Invoke(eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        OnDragMovement?.Invoke(eventData);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        OnEndDragMovement?.Invoke(eventData);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        OnPointerDownMovement?.Invoke();
    }
}