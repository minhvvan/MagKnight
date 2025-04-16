using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ArtifactInventoryUI : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
{
    
    
    public void OnPointerDown(PointerEventData eventData)
    {
        Debug.Log(eventData.hovered[3]);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
    }

    public void OnDrag(PointerEventData eventData)
    {
       
    }
}
