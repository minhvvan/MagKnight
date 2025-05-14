using System;
using hvvan;
using Jun;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class InGameUIController : Singleton<InGameUIController>
{
    [SerializeField] FadeText _entranceText;
    [SerializeField] RectTransform _inGameUI;
    [SerializeField] public DialogueUIController dialogueUIController;
    [SerializeField] public StatusUIController statusUIController;
    [SerializeField] private BossStatusUIController bossStatusUIController;
    [SerializeField] public GateIndicatorUIController gateIndicatorUIController;
    [SerializeField] public CurrencyUIController currencyUIController;
    [SerializeField] public InteractIndicator interactIndicator;
    [SerializeField] public ComboUIController comboUIController;

    protected override void Initialize()
    {
        SceneTransitionEvent.OnSceneTransitionComplete += OnSceneTransitionComplete;
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode loadSceneMode)
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
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

    public void ShowDialogUI(DialogueDataSO dialogueData)
    {
        dialogueUIController.ShowDialogue(dialogueData);
        // dialogueUIController.SetText("테스트 기본 대화 입니다. 이 부분에 대화에 관한 컨텍스트를 만들어서 넣을 예정입니다.");
    }

    public void BindAttributeChanges(AbilitySystem abilitySystem)
    {
        //attributeSet에 묶인 UI 변경 추가
        statusUIController.BindAttributeChanges(abilitySystem);
        ShowInGameUI();
    }

    public void UnbindAttributeChanges()
    {
        //attributeSet에 묶인 UI 변경 해제
        statusUIController.UnbindAttributeChanges();
    }
    public void BindBossAttributeChanges(string bossName, AbilitySystem abilitySystem)
    {
        if(bossStatusUIController == null) return;
        
        bossStatusUIController.BindBossAttributeChanges(abilitySystem);
        bossStatusUIController.SetBossName(bossName);
        bossStatusUIController.Show();
    }
    public void UnbindBossAttributeChanges()
    {
        if(bossStatusUIController == null) return;

        bossStatusUIController.UnbindBossAttributeChanges();
        bossStatusUIController.Hide();
    }

    public void ImmediateHideInGameUI()
    {
        bossStatusUIController.gameObject.SetActive(false);
    }

    public void AddCombo()
    {
        comboUIController.AddCombo();
    }
}