using System;
using System.Collections;
using System.Collections.Generic;
using AYellowpaper.SerializedCollections;
using DG.Tweening;
using Jun;
using Moon;
using UnityEngine;
using UnityEngine.UI;

public class StatusUIController : MonoBehaviour
{
    [SerializeField] public BarController healthBar;
    [SerializeField] BarController skillBar;
    [SerializeField] Image magnetIcon;
    [SerializeField] RectTransform magnetIconRectTransform;
    [SerializeField] SerializedDictionary<WeaponType, RectTransform> weaponDictionary;
    
    private PlayerAttributeSet _attributeSet;
    

    void OnEnable()
    {
        PlayerEvent.OnPolarityChange += SetPolarityChange;
        PlayerEvent.OnWeaponChange += SetWeaponIcon;
    }

    void OnDisable()
    {
        PlayerEvent.OnPolarityChange -= SetPolarityChange;
        PlayerEvent.OnWeaponChange -= SetWeaponIcon;
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
        healthBar.SetValue(currentHealth, newMaxHealth);
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
        healthBar.SetValue(newHealth, maxHealth);     
    }
    
    private void ChangedMaxSkillGauge(float newMaxSkillGauge)
    {
        if (_attributeSet == null)
        {
            Debug.Log("Attribute set is null");
            return;
        }

        var currentSkillGauge = _attributeSet.GetValue(AttributeType.SkillGauge);
        
        skillBar.SetFillAmount(currentSkillGauge / newMaxSkillGauge, true);
        skillBar.SetValue(currentSkillGauge, newMaxSkillGauge);
    }

    private void ChangedCurrentSkillGauge(float newSkillGauge)
    {
        if (_attributeSet == null)
        {
            Debug.Log("Attribute set is null");
            return;
        }

        var maxSkillGauge = _attributeSet.GetValue(AttributeType.MaxSkillGauge);
        
        skillBar.SetFillAmount(newSkillGauge / maxSkillGauge, true);
        skillBar.SetValue(newSkillGauge, maxSkillGauge);
    }

    public void SetPolarityChange(MagneticType magneticType)
    {
        magnetIconRectTransform.DOKill();
        magnetIconRectTransform.localScale = Vector3.one;
        magnetIconRectTransform.DOPunchScale(Vector3.one * 0.1f, 0.5f, 1, 0.5f).OnComplete(() =>
        {
            magnetIconRectTransform.localScale = Vector3.one;
        });

        if (magneticType == MagneticType.S)
        {
            magnetIcon.DOColor(new Color(0, 0.4f, 1f, 1f), 0.5f);
        }
        else
        {
            //magnetIcon.color = Color.red;
            magnetIcon.DOColor(new Color(1f, 0.1f, 0, 1f), 0.5f);
        }
    }

    public void SetWeaponIcon(WeaponType weaponType)
    {
        foreach (var weapon in weaponDictionary)
        {
            if (weapon.Key == weaponType)
            {
                weapon.Value.gameObject.SetActive(true);
            }
            else
            {
                weapon.Value.gameObject.SetActive(false);
            }
        }
    }
}
