using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class ArtifactSlot : MonoBehaviour, IDropHandler
{
    [SerializeField] private Image backgroundSprite;

    public ArtifactDataSO artifact;
    
    public int Index { get; private set; }
    public Action<int, ArtifactDataSO> OnArtifactModified;

    public void SetSlotIndex(int index) => Index = index;

    public void SetBackgroundColor(Color color) => backgroundSprite.color = color;
    
    public ArtifactDataSO GetArtifact()
    {
        return artifact;
    }

    public void ModifyArtifact()
    {
        var child = Icon();
        if (child != null)
        {
            var artifactUI = child.GetComponent<ArtifactUI>();
            artifact = artifactUI.GetArtifact();
        }
        else
        {
            artifact = null;
        }
        OnArtifactModified?.Invoke(Index, artifact);
    }

    public GameObject Icon()
    {
        if(transform.childCount > 0)
            return transform.GetChild(0).gameObject;
        return null;
    }

    public void OnDrop(PointerEventData eventData)
    {
        var icon = Icon();
        // 슬롯에 아티팩트가 있을 때 Swap
        if (icon != null)
        {
            icon.GetComponent<ArtifactUI>().SetArtifactIcon(eventData.pointerDrag.GetComponent<ArtifactUI>().startParent);
        }
        eventData.pointerDrag.GetComponent<ArtifactUI>().SetArtifactIcon(transform);
        ModifyArtifact();
        eventData.pointerDrag.GetComponent<ArtifactUI>().startParent.GetComponent<ArtifactSlot>().ModifyArtifact();
    }
}
