using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Managers;
using Moon;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UIManager : Singleton<UIManager>
{
    [SerializeField] public InGameUIController inGameUIController;
    [SerializeField] public PopupUIController popupUIController;


    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (popupUIController != null)
            {
                if (popupUIController.pauseMenuUIController.gameObject.activeSelf)
                {
                    HidePauseMenuUI();
                    //스택형으로 위에뜬 UI를 하나씩 숨기는 로직이 들어가는게 필요해보임
                }
                else
                {
                    ShowPauseMenuUI();
                }
            }
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

