using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour, IPointerDownHandler
{
    [SerializeField] private TextMeshProUGUI tapArea;
    [SerializeField] private TextMeshProUGUI tapToPlayText;
    [SerializeField] private Button topRunButton;
    public void OnPointerDown(PointerEventData eventData)
    {
        OnPlayerTapped();
    }

    private void OnEnable()
    {

    }

    private void OnDisable()
    {
        
    }

    private void OnPlayerTapped()
    {
        
    }

    private void Start()
    {
        
    }

    private void Update()
    {
        
    }
}
