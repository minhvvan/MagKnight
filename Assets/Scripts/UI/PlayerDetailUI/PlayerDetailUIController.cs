using System.Collections;
using System.Collections.Generic;
using hvvan;
using Moon;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerDetailUIController : MonoBehaviour, IBasePopupUIController
{
    [SerializeField] private Image weaponImage;
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
    
    private readonly float _baseStrength = 20;
    private readonly float _baseDefense = 0;
    private readonly float _baseEndureImpulse = 0;
    private readonly float _baseCriticalRate = 0.1f;
    private readonly float _baseCriticalDamage = 2;
    private readonly float _baseAttackSpeed = 1;
    private readonly float _baseMoveSpeed = 1;
    private readonly float _baseMagneticPower = 0;
    private readonly float _baseMagneticRange = 0;

    public void ShowUI()
    {
        gameObject.SetActive(true);
        UpdateUI();
    }

    public void HideUI()
    {
        gameObject.SetActive(false);
    }

    public void UpdateUI()
    {
        var playerASC = GameManager.Instance.Player.AbilitySystem;
        // TODO : WeaponImage 연결
        
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
        
        CriticalRateText.text = playerASC.GetValue(AttributeType.CriticalRate) * 100f + "%";
        if (playerASC.GetValue(AttributeType.CriticalRate) > _baseCriticalRate)
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
        
        CriticalDamageText.text = playerASC.GetValue(AttributeType.CriticalDamage) * 100f + "%";
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
        
        AttackSpeedText.text = playerASC.GetValue(AttributeType.AttackSpeed) * 100f + "%";
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
        
        MoveSpeedText.text = playerASC.GetValue(AttributeType.MoveSpeed) * 100f + "%";
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
