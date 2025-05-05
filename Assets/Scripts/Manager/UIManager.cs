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
    [SerializeField] public InGameUIController inGameUIController;
    [SerializeField] public PopupUIController popupUIController;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.I))
        {
            ShowArtifactInventoryUI(null);
            ShowPlayerDetailUI();
        }
    }

    public void SetInGameUIController(InGameUIController inGameUIController)
    {
        this.inGameUIController = inGameUIController;
    }

    public void ReleaseInGameUIController()
    {
        inGameUIController = null;
    }

    public void SetPopupUIController(PopupUIController popupUIController)
    {
        this.popupUIController = popupUIController;
    }

    public void ReleasePopupUIController()
    {
        popupUIController = null;
    }

    public void ShowConfirmPopup(string title, string message, Action onConfirm, Action onCancel, bool isShowCancelButton = true)
    {
        popupUIController.confirmPopupUIController.ShowUI(title, message, onConfirm, onCancel, isShowCancelButton);
    }

    public void HideConfirmPopupUI()
    {
        popupUIController.confirmPopupUIController.HideUI();
    }

    public void ShowPauseMenuUI()
    {
        popupUIController.pauseMenuUIController.ShowUI();
        EnableCursor();
    }

    public void HidePauseMenuUI()
    {
        popupUIController.pauseMenuUIController.HideUI();
        DisableCursor();
    }


    public void ShowOptionUI()
    {
        popupUIController.optionUIController.ShowUI();
    }

    public void ShowArtifactInventoryUI(ArtifactDataSO artifactDataSO)
    {
        popupUIController.artifactInventoryUIController.ShowUI(artifactDataSO);
        ShowPlayerDetailUI();
        EnableCursor();
    }

    public void ShowGameOverUI()
    {
        popupUIController.gameOverUIController.ShowUI();
        EnableCursor();
    }

    public void ShowPlayerDetailUI()
    {
        popupUIController.playerDetailUIController.ShowUI();
        EnableCursor();
    }

    public void HidePlayerDetailUI()
    {
        popupUIController.playerDetailUIController.HideUI();
        DisableCursor();
    }

    public void ShowUpgradeStatUI()
    {
        popupUIController.upgradeStatUIController.ShowUI();
        EnableCursor();
    }

    public void HideUpgradeStatUI()
    {
        popupUIController.upgradeStatUIController.HideUI();
        DisableCursor();
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
        popupUIController.backgroundImage.gameObject.SetActive(isShow);
    }
}

