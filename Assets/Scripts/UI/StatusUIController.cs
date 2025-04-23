using System;
using System.Collections;
using System.Collections.Generic;
using Jun;
using Moon;
using UnityEngine;

public class StatusUIController : MonoBehaviour
{
    [SerializeField] private BarController healthBar;

    private PlayerAttributeSet _attributeSet;
    
    public void BindAttributeChanges(AbilitySystem abilitySystem)
    {
        //attributeSet 받아오기
        if (abilitySystem.TryGetAttributeSet<PlayerAttributeSet>(out _attributeSet))
        {
            _attributeSet.DelegateAttributeChanged(AttributeType.HP, ChangedCurrentHealth);
            _attributeSet.DelegateAttributeChanged(AttributeType.MaxHP, ChangedMaxHealth);
        }
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
}
