using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class ArtifactSlot : MonoBehaviour
{
    [SerializeField] private Image backgroundSprite;
    [SerializeField] private Image icon;
    
    public int Index { get; private set; }
    public bool HasItem => icon.sprite != null;
    
    private void ShowIcon() => icon.gameObject.SetActive(true);
    private void HideIcon() => icon.gameObject.SetActive(false);

    public void SetSlotIndex(int index) => Index = index;
}
