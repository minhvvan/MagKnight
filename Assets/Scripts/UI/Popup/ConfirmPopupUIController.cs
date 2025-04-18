using System;
using System.Collections;
using System.Collections.Generic;
using AYellowpaper.SerializedCollections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ConfirmPopupUIController : MonoBehaviour, IBasePopupUIController
{
    [SerializeField] Button _confirmButton;
    [SerializeField] Button _cancelButton;

    [SerializeField] TextMeshProUGUI _titleText;
    [SerializeField] TextMeshProUGUI _messageText;

    private Action _onConfirm;
    private Action _onCancel;

    void Awake()
    {
        _confirmButton.onClick.AddListener(OnClickConfirm);
        _cancelButton.onClick.AddListener(OnClickCancel);
    }

    void OnClickConfirm()
    {
        _onConfirm?.Invoke();

        UIManager.Instance.HideConfirmPopupUI();          
    }

    void OnClickCancel()
    {
        _onCancel?.Invoke();

        UIManager.Instance.HideConfirmPopupUI();
    }

    public void ShowUI(string title, string message, System.Action onConfirm, System.Action onCancel, bool isShowCancelButton = true)
    {
        _cancelButton.gameObject.SetActive(isShowCancelButton);
        
        // Set the title and message text
        _titleText.text = title;
        _messageText.text = message;
        _onConfirm = onConfirm;
        _onCancel = onCancel;
        
        ShowUI();
    }

    public void ShowUI()
    {
        gameObject.SetActive(true);
    }

    public void HideUI()
    {
        gameObject.SetActive(false);
    }
}
