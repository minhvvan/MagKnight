using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using hvvan;
using Managers;
using Moon;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UIManager : Singleton<UIManager>
{
    [SerializeField] InGameUIController _inGameUIController;
    [SerializeField] PopupUIController _popupUIController;

    public void SetInGameUIController(InGameUIController inGameUIController)
    {
        this._inGameUIController = inGameUIController;
    }

    public void ReleaseInGameUIController()
    {
        _inGameUIController = null;
    }

    public void SetPopupUIController(PopupUIController popupUIController)
    {
        this._popupUIController = popupUIController;
    }

    public void ReleasePopupUIController()
    {
        _popupUIController = null;
    }

    public void ShowConfirmPopup(string title, string message, Action onConfirm, Action onCancel, bool isShowCancelButton = true)
    {
        _popupUIController.confirmPopupUIController.ShowUI(title, message, onConfirm, onCancel, isShowCancelButton);
    }

    public void HideConfirmPopupUI()
    {
        _popupUIController.confirmPopupUIController.HideUI();
    }

    public void ShowPauseMenuUI()
    {
        _popupUIController.pauseMenuUIController.ShowUI();
        EnableCursor();
    }

    public void HidePauseMenuUI()
    {
        _popupUIController.pauseMenuUIController.HideUI();
        DisableCursor();
    }


    public void ShowOptionUI()
    {
        _popupUIController.optionUIController.ShowUI();
    }

    public void ShowArtifactInventoryUI(ArtifactDataSO artifactDataSO)
    {
        _popupUIController.artifactInventoryUIController.ShowUI(artifactDataSO);
        EnableCursor();
    }
    
    public void EnableCursor()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    public void DisableCursor()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    public void ShowBackgroundImage(bool isShow)
    {
        _popupUIController.backgroundImage.gameObject.SetActive(isShow);
    }
}

