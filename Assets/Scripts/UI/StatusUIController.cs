using System;
using System.Collections;
using System.Collections.Generic;
using Jun;
using Moon;
using UnityEngine;

public class StatusUIController : MonoBehaviour
{
    [SerializeField] private BarController healthBar;
    [SerializeField] private BarController skillBar;
    
    private PlayerAttributeSet _attributeSet;
    
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
}
