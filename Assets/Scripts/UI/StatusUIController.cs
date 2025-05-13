using System;
using System.Collections;
using System.Collections.Generic;
using Jun;
using Moon;
using UnityEngine;
using UnityEngine.UI;

public class StatusUIController : MonoBehaviour
{
    [SerializeField] public BarController healthBar;
    [SerializeField] private BarController skillBar;
    [SerializeField] private Image magnetIcon;
    
    private PlayerAttributeSet _attributeSet;
    

    void OnEnable()
    {
        PlayerEvent.OnPolarityChange += SetPolarityChange;
    }

    void OnDisable()
    {
        PlayerEvent.OnPolarityChange -= SetPolarityChange;
    }

    public void BindAttributeChanges(AbilitySystem abilitySystem)
    {
        //attributeSet 받아오기
        if (abilitySystem.TryGetAttributeSet<PlayerAttributeSet>(out _attributeSet))
        {
            _attributeSet.SubscribeAttributeChanged(AttributeType.HP, ChangedCurrentHealth);
            _attributeSet.SubscribeAttributeChanged(AttributeType.MaxHP, ChangedMaxHealth);
            
            _attributeSet.SubscribeAttributeChanged(AttributeType.SkillGauge, ChangedCurrentSkillGauge);
            _attributeSet.SubscribeAttributeChanged(AttributeType.MaxSkillGauge, ChangedMaxSkillGauge);
        }
        
        //UI Update
        UpdateUI();
    }

    public void UnbindAttributeChanges()
    {
        if (_attributeSet == null) return;
        
        _attributeSet.UnsubscribeAttributeChanged(AttributeType.HP, ChangedCurrentHealth);
        _attributeSet.UnsubscribeAttributeChanged(AttributeType.MaxHP, ChangedMaxHealth);
        
        _attributeSet.UnsubscribeAttributeChanged(AttributeType.SkillGauge, ChangedCurrentSkillGauge);
        _attributeSet.UnsubscribeAttributeChanged(AttributeType.MaxSkillGauge, ChangedMaxSkillGauge);
    }

    private void UpdateUI()
    {
        if(_attributeSet == null) return;
        
        //ui상태 갱신
        ChangedMaxHealth(_attributeSet.GetValue(AttributeType.MaxHP));
        ChangedCurrentHealth(_attributeSet.GetValue(AttributeType.HP));
        ChangedCurrentSkillGauge(_attributeSet.GetValue(AttributeType.SkillGauge));
        ChangedMaxSkillGauge(_attributeSet.GetValue(AttributeType.MaxSkillGauge));
    }

    private void ChangedMaxHealth(float newMaxHealth)
    {
        if (_attributeSet == null)
        {
            Debug.Log("Attribute set is null");
            return;
        }

        var currentHealth = _attributeSet.GetValue(AttributeType.HP);
        
        //현재 체력 변경
        healthBar.SetFillAmount(currentHealth / newMaxHealth, true);
    }

    private void ChangedCurrentHealth(float newHealth)
    {
        if (_attributeSet == null)
        {
            Debug.Log("Attribute set is null");
            return;
        }

        var maxHealth = _attributeSet.GetValue(AttributeType.MaxHP);
        
        //현재 체력 변경
        healthBar.SetFillAmount(newHealth / maxHealth, true);
    }
    
    private void ChangedMaxSkillGauge(float newMaxSkillGauge)
    {
        if (_attributeSet == null)
        {
            Debug.Log("Attribute set is null");
            return;
        }

        var currentSkillGauge = _attributeSet.GetValue(AttributeType.SkillGauge);
        
        //현재 체력 변경
        skillBar.SetFillAmount(currentSkillGauge / newMaxSkillGauge, true);
    }

    private void ChangedCurrentSkillGauge(float newSkillGauge)
    {
        if (_attributeSet == null)
        {
            Debug.Log("Attribute set is null");
            return;
        }

        var maxSkillGauge = _attributeSet.GetValue(AttributeType.MaxSkillGauge);
        
        //현재 체력 변경
        skillBar.SetFillAmount(newSkillGauge / maxSkillGauge, true);
    }

    public void SetPolarityChange(MagneticType magneticType)
    {
        if (magneticType == MagneticType.S)
        {
            magnetIcon.color = new Color(0, 0.4f, 1f, 1f);
        }
        else
        {
            magnetIcon.color = Color.red;
        }
    }
}
