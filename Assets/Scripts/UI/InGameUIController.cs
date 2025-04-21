using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class InGameUIController : MonoBehaviour
{
    [SerializeField] FadeText _entranceText;
    [SerializeField] RectTransform _inGameUI;
    [SerializeField] public DialogueUIController dialogueUIController;

    void Awake()
    {
        SceneTransitionEvent.OnSceneTransitionComplete += OnSceneTransitionComplete;

    }

    void OnEnable()
    {
        UIManager.Instance.SetInGameUIController(this);
    }

    private void OnSceneTransitionComplete(string entranceName, bool isSetNewName)
    {
       _entranceText.SetTextAndShowFadeInAndOut(entranceName, isSetNewName);
    }

    public void ShowInGameUI()
    {
        _inGameUI.gameObject.SetActive(true);
    }

    public void HideInGameUI()
    {
        _inGameUI.gameObject.SetActive(false);
    }

    public void ShowDialogUI(NPCSO npcSO)
    {
        dialogueUIController.ShowDialogue(npcSO);
        // dialogueUIController.SetText("테스트 기본 대화 입니다. 이 부분에 대화에 관한 컨텍스트를 만들어서 넣을 예정입니다.");
    }
}