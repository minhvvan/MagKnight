using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using hvvan;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UpgradeStatPanelController : MonoBehaviour
{
    [SerializeField] TMP_Text levelText;
    [SerializeField] TMP_Text statText;
    [SerializeField] Image fillImage;
    [SerializeField] Button upgradeButton;
    [SerializeField] TMP_Text upgradeCostText;
    [SerializeField] AttributeType statType;

    public Action<GameplayEffect> OnClickButton;
    
    private readonly float _baseMaxHp = 200f;
    private readonly float _baseStrength = 10;
    private readonly float _baseDefense = 0;
    private readonly float _baseEndureImpulse = 0;
    private readonly float _baseCriticalRate = 0.1f;
    private readonly float _baseCriticalDamage = 2;
    private readonly float _baseAttackSpeed = 1;
    private readonly float _baseMoveSpeed = 1;
    private readonly float _baseMagneticPower = 0;
    private readonly float _baseMagneticRange = 0;

    private readonly int maxLevel = 9;

    private int currentLevel = 0;
    private GameplayEffect upgradeEffect;
    
    List<int> upgradeCosts = new List<int> { 100, 150, 200, 300, 500, 800, 1500, 3000, 5000, 10000 };
    
    public void Initialized(PlayerData playerData)
    {
        currentLevel = CalculateLevel(playerData.PlayerStat);
        UpdateLevel(playerData);
        upgradeButton.onClick.RemoveAllListeners();
        upgradeButton.onClick.AddListener(() => UpgradeStat(playerData));
    }
    
    private int CalculateLevel(PlayerStat playerStat)
    {
        switch (statType)
        {
            case AttributeType.MaxHP:
                upgradeEffect = new GameplayEffect(EffectType.Instant, AttributeType.MaxHP, 10f);
                statText.SetText($"{playerStat.MaxHP.Value}");
                return Mathf.RoundToInt((playerStat.MaxHP.Value - _baseMaxHp) / 10f);
            case AttributeType.Strength:
                upgradeEffect = new GameplayEffect(EffectType.Instant, AttributeType.Strength, 10f);
                statText.SetText($"{playerStat.Strength.Value}");
                return Mathf.RoundToInt((playerStat.Strength.Value - _baseStrength) / 10f);
            case AttributeType.Defense:
                upgradeEffect = new GameplayEffect(EffectType.Instant, AttributeType.Defense, 5f);
                statText.SetText($"{playerStat.Defense.Value}");
                return Mathf.RoundToInt((playerStat.Defense.Value - _baseDefense) / 5f);
            case AttributeType.EndureImpulse:
                upgradeEffect = new GameplayEffect(EffectType.Instant, AttributeType.EndureImpulse, 10f);
                statText.SetText($"{playerStat.EndureImpulse.Value}");
                return Mathf.RoundToInt((playerStat.EndureImpulse.Value - _baseEndureImpulse) / 10f);
            case AttributeType.CriticalRate:
                upgradeEffect = new GameplayEffect(EffectType.Instant, AttributeType.CriticalRate, 0.05f);
                statText.text = playerStat.CriticalRate.Value * 100 + "%";
                return Mathf.RoundToInt((playerStat.CriticalRate.Value - _baseCriticalRate) / 0.05f);
            case AttributeType.CriticalDamage:
                upgradeEffect = new GameplayEffect(EffectType.Instant, AttributeType.CriticalDamage, 0.1f);
                statText.text = playerStat.CriticalDamage.Value * 100 + "%";
                return Mathf.RoundToInt( (playerStat.CriticalDamage.Value - _baseCriticalDamage) / 0.1f);
            case AttributeType.AttackSpeed:
                upgradeEffect = new GameplayEffect(EffectType.Instant, AttributeType.AttackSpeed, 0.05f);
                statText.text = playerStat.AttackSpeed.Value * 100 + "%";
                return Mathf.RoundToInt((playerStat.AttackSpeed.Value - _baseAttackSpeed) / 0.05f );
            case AttributeType.MoveSpeed:
                upgradeEffect = new GameplayEffect(EffectType.Instant, AttributeType.MoveSpeed, 0.05f);
                statText.text = playerStat.MoveSpeed.Value * 100 + "%";
                return Mathf.RoundToInt((playerStat.MoveSpeed.Value - _baseMoveSpeed) / 0.05f);
            case AttributeType.MagneticPower:
                upgradeEffect = new GameplayEffect(EffectType.Instant, AttributeType.MagneticPower, 10f);
                statText.SetText($"{playerStat.MagneticPower.Value}");
                return Mathf.RoundToInt((playerStat.MagneticPower.Value - _baseMagneticPower) / 10f);
            case AttributeType.MagneticRange:
                upgradeEffect = new GameplayEffect(EffectType.Instant, AttributeType.MagneticRange, 5f);
                statText.SetText($"{playerStat.MagneticRange.Value}");
                return Mathf.RoundToInt((playerStat.MagneticRange.Value - _baseMagneticRange) / 10f);
        }
        return 0;
    }
    
    private void UpgradeStat(PlayerData playerData)
    {
        switch (statType)
        {
            case AttributeType.MaxHP:
                playerData.PlayerStat.MaxHP.Value += 10;
                playerData.PlayerStat.HP.Value += 10;
                statText.SetText($"{playerData.PlayerStat.MaxHP.Value}");
                break;
            case AttributeType.Strength:
                playerData.PlayerStat.Strength.Value += 10;
                statText.SetText($"{playerData.PlayerStat.Strength.Value}");
                break;
            case AttributeType.Defense:
                playerData.PlayerStat.Defense.Value += 5;
                statText.SetText($"{playerData.PlayerStat.Defense.Value}");
                break;
            case AttributeType.EndureImpulse:
                playerData.PlayerStat.EndureImpulse.Value += 10;
                statText.SetText($"{playerData.PlayerStat.EndureImpulse.Value}");
                break;
            case AttributeType.CriticalRate:
                playerData.PlayerStat.CriticalRate.Value += 0.05f;
                statText.text = playerData.PlayerStat.CriticalRate.Value * 100 + "%";
                break;
            case AttributeType.CriticalDamage:
                playerData.PlayerStat.CriticalDamage.Value += 0.1f;
                statText.text = playerData.PlayerStat.CriticalDamage.Value * 100 + "%";
                break;
            case AttributeType.AttackSpeed:
                playerData.PlayerStat.AttackSpeed.Value += 0.05f;
                statText.text = playerData.PlayerStat.AttackSpeed.Value * 100 + "%";
                break;
            case AttributeType.MoveSpeed:
                playerData.PlayerStat.MoveSpeed.Value += 0.05f;
                statText.text = playerData.PlayerStat.MoveSpeed.Value * 100 + "%";
                break;
            case AttributeType.MagneticPower:
                playerData.PlayerStat.MagneticPower.Value += 10;
                statText.SetText($"{playerData.PlayerStat.MagneticPower.Value}");
                break;
            case AttributeType.MagneticRange:
                playerData.PlayerStat.MagneticRange.Value += 10;
                statText.SetText($"{playerData.PlayerStat.MagneticRange.Value}");
                break;
        }
        
        playerData.Currency -= upgradeCosts[currentLevel];
        currentLevel++;
        
        OnClickButton?.Invoke(upgradeEffect);
        if(statType == AttributeType.MaxHP)
            OnClickButton?.Invoke(new GameplayEffect(EffectType.Instant, AttributeType.HP, 10f));
    }

    public void UpdateLevel(PlayerData playerData)
    {
        if (currentLevel == maxLevel)
            levelText.text = "Lv.MAX";
        else
            levelText.text = $"Lv.{currentLevel+1}";
        if (currentLevel > 0)
            statText.color = Color.green;
        else
            statText.color = Color.white;
        fillImage.fillAmount = currentLevel/10f;
        upgradeCostText.text = upgradeCosts[currentLevel] + "$";
        
        if(currentLevel == maxLevel || playerData.Currency < upgradeCosts[currentLevel])
            upgradeButton.interactable = false;
        else
        {
            upgradeButton.interactable = true;
        }
    }
}
