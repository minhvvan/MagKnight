using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Managers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InteractIndicator : MonoBehaviour
{
    [SerializeField] private CanvasGroup indicator;
    [SerializeField] private TMP_Text interactText;
    
    private InteractTextSO _interactTextSO;

    private async void Awake()
    {
        indicator.DOFade(0f, 0f);
        _interactTextSO = await DataManager.Instance.LoadScriptableObjectAsync<InteractTextSO>(Addresses.Data.Interact.InteractText);
    }

    public void InteractSelected(InteractType type)
    {
        interactText.text = _interactTextSO.text[type];
        indicator.DOFade(1f, 0.5f);
    }
    
    public void InteractUnSelected()
    {
        interactText.text = "";
        indicator.DOFade(0f, 0.5f);
    }
}

public enum InteractType
{
    None,
    Dialogue,
    Buy,
    Open,
    Loot,
    Max
}