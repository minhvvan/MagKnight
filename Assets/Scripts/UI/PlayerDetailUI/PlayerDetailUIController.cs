using System;
using DG.Tweening;
using hvvan;
using Moon;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class PlayerDetailUIController : MonoBehaviour, IBasePopupUIController
{
    [SerializeField] private GameObject magCorePrefab;
    [SerializeField] private Transform weaponBackGround;
    [SerializeField] private BarController hpBarController;
    [SerializeField] private TMP_Text HpText;
    [SerializeField] private BarController skillBarController;
    [SerializeField] private TMP_Text SkillText;
    
    [SerializeField] private TMP_Text StrengthText;
    [SerializeField] private TMP_Text DefenseText;
    [SerializeField] private TMP_Text EndureImpulseText;
    [SerializeField] private TMP_Text CriticalRateText;
    [SerializeField] private TMP_Text CriticalDamageText;
    [SerializeField] private TMP_Text AttackSpeedText;
    [SerializeField] private TMP_Text MoveSpeedText;
    [SerializeField] private TMP_Text MagneticPowerText;
    [SerializeField] private TMP_Text MagneticRangeText;
    
    [SerializeField] RectTransform rectTransform;
    
    private float _baseStrength;
    private float _baseDefense;
    private float _baseEndureImpulse;
    private float _baseCriticalRate;
    private float _baseCriticalDamage;
    private float _baseAttackSpeed;
    private float _baseMoveSpeed;
    private float _baseMagneticPower;
    private float _baseMagneticRange;

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
    }
    
    public void ShowUI()
    {
        rectTransform.DOKill();
        rectTransform.DOScale(1, 0.1f);
        gameObject.SetActive(true);
        UpdateUI();
    }

    public void HideUI()
    {
        rectTransform.DOKill();
        rectTransform.DOScale(0, 0.1f).OnComplete(() => { gameObject.SetActive(false); });
    }

    public async void UpdateUI()
    {
        PlayerStat playerBaseStat = await GameManager.Instance.GetPlayerStat();
        
        _baseStrength = playerBaseStat.Strength.Value;
        _baseDefense = playerBaseStat.Defense.Value;
        _baseEndureImpulse = playerBaseStat.EndureImpulse.Value;
        _baseCriticalRate = playerBaseStat.CriticalRate.Value;
        _baseCriticalDamage = playerBaseStat.CriticalDamage.Value;
        _baseAttackSpeed = playerBaseStat.AttackSpeed.Value;
        _baseMoveSpeed = playerBaseStat.MoveSpeed.Value;
        _baseMagneticPower = playerBaseStat.MagneticPower.Value;
        _baseMagneticRange = playerBaseStat.MagneticRange.Value;
        
        var playerASC = GameManager.Instance.Player.AbilitySystem;
        // TODO : WeaponImage 연결
        var magCore = GameManager.Instance.Player.WeaponHandler.currentMagCore;
        if (!magCore.IsUnityNull())
        {
            Instantiate(magCorePrefab, weaponBackGround).GetComponent<MagCoreUI>().SetIcon();
        }
        
        hpBarController.SetFillAmount(playerASC.GetValue(AttributeType.HP) / playerASC.GetValue(AttributeType.MaxHP), false);
        HpText.text = "[" + playerASC.GetValue(AttributeType.HP) + "/" + playerASC.GetValue(AttributeType.MaxHP) + "]";
        skillBarController.SetFillAmount(playerASC.GetValue(AttributeType.SkillGauge)/playerASC.GetValue(AttributeType.MaxSkillGauge), false);
        SkillText.text = "[" + playerASC.GetValue(AttributeType.SkillGauge) + "/" + playerASC.GetValue(AttributeType.MaxSkillGauge) + "]";
        
        StrengthText.text = playerASC.GetValue(AttributeType.Strength).ToString();
        if (playerASC.GetValue(AttributeType.Strength) > _baseStrength)
        {
            StrengthText.color = Color.green;
        }
        else if (playerASC.GetValue(AttributeType.Strength) < _baseStrength)
        {
            StrengthText.color = Color.red;
        }
        else
        {
            StrengthText.color = Color.white;
        }
        
        DefenseText.text = playerASC.GetValue(AttributeType.Defense).ToString();
        if (playerASC.GetValue(AttributeType.Defense) > _baseDefense)
        {
            DefenseText.color = Color.green;
        }
        else if (playerASC.GetValue(AttributeType.Defense) < _baseDefense)
        {
            DefenseText.color = Color.red;
        }
        else
        {
            DefenseText.color = Color.white;
        }
        
        EndureImpulseText.text = playerASC.GetValue(AttributeType.EndureImpulse).ToString();
        if (playerASC.GetValue(AttributeType.EndureImpulse) > _baseEndureImpulse)
        {
            EndureImpulseText.color = Color.green;
        }
        else if (playerASC.GetValue(AttributeType.EndureImpulse) < _baseEndureImpulse)
        {
            EndureImpulseText.color = Color.red;
        }
        else
        {
            EndureImpulseText.color = Color.white;
        }
        
        CriticalRateText.text = Math.Round(playerASC.GetValue(AttributeType.CriticalRate) * 100f, 2) + "%";
        if (Math.Round(playerASC.GetValue(AttributeType.CriticalRate),2) > Math.Round(_baseCriticalRate,2))
        {
            CriticalRateText.color = Color.green;
        }
        else if (playerASC.GetValue(AttributeType.CriticalRate) < _baseCriticalRate)
        {
            CriticalRateText.color = Color.red;
        }
        else
        {
            CriticalRateText.color = Color.white;
        }
        
        CriticalDamageText.text = Math.Round(playerASC.GetValue(AttributeType.CriticalDamage) * 100f,2) + "%";
        if (playerASC.GetValue(AttributeType.CriticalDamage) > _baseCriticalDamage)
        {
            CriticalDamageText.color = Color.green;
        }
        else if (playerASC.GetValue(AttributeType.CriticalDamage) < _baseCriticalDamage)
        {
            CriticalDamageText.color = Color.red;
        }
        else
        {
            CriticalDamageText.color = Color.white;
        }
        
        AttackSpeedText.text = Math.Round(playerASC.GetValue(AttributeType.AttackSpeed) * 100f,2) + "%";
        if (playerASC.GetValue(AttributeType.AttackSpeed) > _baseAttackSpeed)
        {
            AttackSpeedText.color = Color.green;
        }
        else if (playerASC.GetValue(AttributeType.AttackSpeed) < _baseAttackSpeed)
        {
            AttackSpeedText.color = Color.red;
        }
        else
        {
            AttackSpeedText.color = Color.white;
        }
        
        MoveSpeedText.text = Math.Round(playerASC.GetValue(AttributeType.MoveSpeed) * 100f,2) + "%";
        if (playerASC.GetValue(AttributeType.MoveSpeed) > _baseMoveSpeed)
        {
            MoveSpeedText.color = Color.green;
        }
        else if (playerASC.GetValue(AttributeType.MoveSpeed) < _baseMoveSpeed)
        {
            MoveSpeedText.color = Color.red;
        }
        else
        {
            MoveSpeedText.color = Color.white;
        }
        
        MagneticPowerText.text = playerASC.GetValue(AttributeType.MagneticPower).ToString();
        if (playerASC.GetValue(AttributeType.MagneticPower) > _baseMagneticPower)
        {
            MagneticPowerText.color = Color.green;
        }
        else if (playerASC.GetValue(AttributeType.MagneticPower) < _baseMagneticPower)
        {
            MagneticPowerText.color = Color.red;
        }
        else
        {
            MagneticPowerText.color = Color.white;
        }
        
        MagneticRangeText.text = playerASC.GetValue(AttributeType.MagneticRange).ToString();
        if (playerASC.GetValue(AttributeType.MagneticRange) > _baseMagneticRange)
        {
            MagneticRangeText.color = Color.green;
        }
        else if (playerASC.GetValue(AttributeType.MagneticRange) < _baseMagneticRange)
        {
            MagneticRangeText.color = Color.red;
        }
        else
        {
            MagneticRangeText.color = Color.white;
        }
    }
}
