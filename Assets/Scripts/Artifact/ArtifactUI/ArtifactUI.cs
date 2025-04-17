using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ArtifactUI : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public ArtifactDataSO artifact;
    public Image icon;

    Vector3 startPos;
    [HideInInspector] public Transform startParent;
    
    [SerializeField] public Transform onDragParent;
    
    public void Initialized(ArtifactDataSO artifact, Transform onDragParent)
    {
        this.artifact = artifact;
        icon.sprite = artifact.icon;
        this.onDragParent = onDragParent;
    }

    
    public void SetArtifactIcon(Transform parent)
    {
        transform.SetParent(parent);
        transform.transform.position = parent.position;
    }
    

    public void OnBeginDrag(PointerEventData eventData)
    {
        startPos = transform.position;
        startParent = transform.parent;
        GetComponent<CanvasGroup>().blocksRaycasts = false;
        transform.SetParent(onDragParent);
    }

    public void OnDrag(PointerEventData eventData)
    {
        transform.position = Input.mousePosition;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        GetComponent<CanvasGroup>().blocksRaycasts = true;

        if (transform.parent == onDragParent)
        {
            transform.position = startPos;
            transform.SetParent(startParent);
        }
        else
        {
            startParent.GetComponent<ArtifactSlot>().ModifyArtifact();
            transform.parent.GetComponent<ArtifactSlot>().ModifyArtifact();
        }
    }
}
