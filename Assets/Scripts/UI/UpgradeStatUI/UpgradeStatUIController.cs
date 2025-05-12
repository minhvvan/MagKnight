using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using hvvan;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UpgradeStatUIController : MonoBehaviour, IBasePopupUIController
{
    [SerializeField] TMP_Text CurrencyText;
    [SerializeField] UpgradeStatPanelController maxHpPanel;
    [SerializeField] UpgradeStatPanelController strengthPanel;
    [SerializeField] UpgradeStatPanelController defencePanel;
    [SerializeField] UpgradeStatPanelController endureImpulsePanel;
    [SerializeField] UpgradeStatPanelController criticalRatePanel;
    [SerializeField] UpgradeStatPanelController criticalDamagePanel;
    [SerializeField] UpgradeStatPanelController attackSpeedPanel;
    [SerializeField] UpgradeStatPanelController moveSpeedPanel;
    [SerializeField] UpgradeStatPanelController magneticRangePanel;
    [SerializeField] UpgradeStatPanelController magneticPowerPanel;
    [SerializeField] RectTransform rectTransform;
    [SerializeField] Button confirmButton;
    [SerializeField] Button returnButton;
    
    public PlayerData _playerData;

    private List<GameplayEffect> _activeEffects = new List<GameplayEffect>();

    private void Awake()
    {
        Initialize();
    }

    void Initialize()
    {
        maxHpPanel.OnClickButton += OnClickUpgradeButton;
        strengthPanel.OnClickButton += OnClickUpgradeButton;
        defencePanel.OnClickButton += OnClickUpgradeButton;
        endureImpulsePanel.OnClickButton += OnClickUpgradeButton;
        criticalRatePanel.OnClickButton += OnClickUpgradeButton;
        criticalDamagePanel.OnClickButton += OnClickUpgradeButton;
        attackSpeedPanel.OnClickButton += OnClickUpgradeButton;
        moveSpeedPanel.OnClickButton += OnClickUpgradeButton;
        magneticRangePanel.OnClickButton += OnClickUpgradeButton;
        magneticPowerPanel.OnClickButton += OnClickUpgradeButton;
        
        confirmButton.onClick.AddListener(OnClickConfirmButton);
        returnButton.onClick.AddListener(HideUI);
    }

    async void SetPlayerData()
    {
        _playerData = await GameManager.Instance.GetPlayerData();
        
        CurrencyText.text = _playerData.Currency.ToString();
        maxHpPanel.Initialized(_playerData);
        strengthPanel.Initialized(_playerData);
        defencePanel.Initialized(_playerData);
        endureImpulsePanel.Initialized(_playerData);
        criticalRatePanel.Initialized(_playerData);
        criticalDamagePanel.Initialized(_playerData);
        attackSpeedPanel.Initialized(_playerData);
        moveSpeedPanel.Initialized(_playerData);
        magneticRangePanel.Initialized(_playerData);
        magneticPowerPanel.Initialized(_playerData);
    }

    private void OnClickUpgradeButton(GameplayEffect effect)
    {
        CurrencyText.text = _playerData.Currency.ToString();
        
        _activeEffects.Add(effect);
        
        maxHpPanel.UpdateLevel(_playerData);
        strengthPanel.UpdateLevel(_playerData);
        defencePanel.UpdateLevel(_playerData);
        endureImpulsePanel.UpdateLevel(_playerData);
        criticalRatePanel.UpdateLevel(_playerData);
        criticalDamagePanel.UpdateLevel(_playerData);
        attackSpeedPanel.UpdateLevel(_playerData);
        moveSpeedPanel.UpdateLevel(_playerData);
        magneticRangePanel.UpdateLevel(_playerData);
        magneticPowerPanel.UpdateLevel(_playerData);
    }
    
    public void ShowUI()
    {
        SetPlayerData();
        rectTransform.DOKill();
        rectTransform.DOScale(1, 0.1f);
        gameObject.SetActive(true);
        GameManager.Instance.Player.InputHandler.ReleaseControl();
    }

    public void HideUI()
    {
        rectTransform.DOKill();
        rectTransform.DOScale(0, 0.1f).OnComplete(() => { gameObject.SetActive(false); });
        GameManager.Instance.Player.InputHandler.GainControl();
    }

    private async void OnClickConfirmButton()
    {
        if (_activeEffects.Count == 0)
        {
            HideUI();
            return;
        }
        
        foreach (var effect in _activeEffects)
        {
            GameManager.Instance.Player.AbilitySystem.ApplyEffect(effect);
        }
        _activeEffects.Clear();
        await GameManager.Instance.SetPlayerData(_playerData);
        await GameManager.Instance.SaveData(Constants.PlayerData);
        await GameManager.Instance.SaveData(Constants.CurrentRun);
        
        UIManager.Instance.inGameUIController.currencyUIController.UpdateUI();
        HideUI();
    }
}
