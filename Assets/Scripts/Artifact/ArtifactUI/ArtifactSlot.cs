using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class ArtifactSlot : MonoBehaviour, IDropHandler
{
    [SerializeField] private Image backgroundSprite;

    public ArtifactGAS artifact;
    
    public int Index { get; private set; }
    public Action<int, ArtifactGAS> OnArtifactModified;

    public void SetSlotIndex(int index) => Index = index;

    public void SetBackgroundColor(Color color) => backgroundSprite.color = color;
    
    public ArtifactGAS GetArtifact()
    {
        return artifact;
    }

    public void ModifyArtifact()
    {
        var child = Icon();
        if (child != null)
        {
            var artifactUI = child.GetComponent<ArtifactUI>();
            artifact = artifactUI.artifact;
        }
        else
        {
            artifact = null;
        }
        OnArtifactModified?.Invoke(Index, artifact);
    }

    GameObject Icon()
    {
        if(transform.childCount > 0)
            return transform.GetChild(0).gameObject;
        return null;
    }

    public void OnDrop(PointerEventData eventData)
    {
        var icon = Icon();
        // 슬롯에 아티팩트가 없을 때
        if (icon == null)
        {
            eventData.pointerDrag.transform.SetParent(transform);
            eventData.pointerDrag.transform.position = transform.position;
            artifact = eventData.pointerDrag.GetComponent<ArtifactUI>().artifact;
        }
        // 슬롯에 아티팩트가 있으면 Swap
        else
        {
            icon.GetComponent<ArtifactUI>().SetArtifactIcon(eventData.pointerDrag.GetComponent<ArtifactUI>().startParent);
            
            eventData.pointerDrag.transform.SetParent(transform);
            eventData.pointerDrag.transform.position = transform.position;
            artifact = eventData.pointerDrag.GetComponent<ArtifactUI>().artifact;
        }
    }
}
