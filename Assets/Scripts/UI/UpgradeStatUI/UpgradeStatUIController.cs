using System;
using System.Collections;
using System.Collections.Generic;
using hvvan;
using TMPro;
using UnityEngine;

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
    

    public PlayerData _playerData;


    private void Awake()
    {
        Initialize();
    }

    async void Initialize()
    {
        _playerData = await GameManager.Instance.GetPlayerData();
        
        CurrencyText.text = _playerData.Currency.ToString();
        
        maxHpPanel.Initialized(_playerData);
        maxHpPanel.OnClickButton += UpdateCurrencyText;
        strengthPanel.Initialized(_playerData);
        strengthPanel.OnClickButton += UpdateCurrencyText;
        defencePanel.Initialized(_playerData);
        defencePanel.OnClickButton += UpdateCurrencyText;
        endureImpulsePanel.Initialized(_playerData);
        endureImpulsePanel.OnClickButton += UpdateCurrencyText;
        criticalRatePanel.Initialized(_playerData);
        criticalRatePanel.OnClickButton += UpdateCurrencyText;
        criticalDamagePanel.Initialized(_playerData);
        criticalDamagePanel.OnClickButton += UpdateCurrencyText;
        attackSpeedPanel.Initialized(_playerData);
        attackSpeedPanel.OnClickButton += UpdateCurrencyText;
        moveSpeedPanel.Initialized(_playerData);
        moveSpeedPanel.OnClickButton += UpdateCurrencyText;
        magneticRangePanel.Initialized(_playerData);
        magneticRangePanel.OnClickButton += UpdateCurrencyText;
        magneticPowerPanel.Initialized(_playerData);
        magneticPowerPanel.OnClickButton += UpdateCurrencyText;
    }

    private void UpdateCurrencyText()
    {
        CurrencyText.text = _playerData.Currency.ToString();
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
        gameObject.SetActive(true);
        GameManager.Instance.Player.InputHandler.ReleaseControl();
    }

    public void HideUI()
    {
        gameObject.SetActive(false);
        GameManager.Instance.Player.InputHandler.GainControl();
    }
    
}
