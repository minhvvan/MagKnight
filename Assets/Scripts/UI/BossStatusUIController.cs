using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Jun;
using Moon;
using TMPro;
using UnityEngine;

public class BossStatusUIController : MonoBehaviour
{
    [SerializeField] private BarController _healthBar;
    [SerializeField] private TextMeshProUGUI _bossNameText;
    [SerializeField] private CanvasGroup _canvasGroup;
    
    private EnemyAttributeSet _attributeSet;
    

    public void SetBossName(string bossName)
    {
        if (_bossNameText == null) return;
        
        _bossNameText.text = bossName;
    }
    
    public void BindBossAttributeChanges(AbilitySystem abilitySystem)
    {
        //attributeSet 받아오기
        if (abilitySystem.TryGetAttributeSet<EnemyAttributeSet>(out _attributeSet))
        {
            _attributeSet.SubscribeAttributeChanged(AttributeType.HP, ChangedCurrentHealth);
            _attributeSet.SubscribeAttributeChanged(AttributeType.MaxHP, ChangedMaxHealth);
        }
        
        //UI Update
        UpdateUI();
    }

    public void UnbindBossAttributeChanges()
    {
        if (_attributeSet == null) return;
        
        _attributeSet.UnsubscribeAttributeChanged(AttributeType.HP, ChangedCurrentHealth);
        _attributeSet.UnsubscribeAttributeChanged(AttributeType.MaxHP, ChangedMaxHealth);
    }

    private void UpdateUI()
    {
        if(_attributeSet == null) return;
        
        //ui상태 갱신
        ChangedMaxHealth(_attributeSet.GetValue(AttributeType.MaxHP));
        ChangedCurrentHealth(_attributeSet.GetValue(AttributeType.HP));
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
        _healthBar.SetFillAmount(currentHealth / newMaxHealth, false);
        _healthBar.SetValue(currentHealth, newMaxHealth);
    }

    private void ChangedCurrentHealth(float newHealth)
    {
        if (_attributeSet == null)
        {
            Debug.Log("Attribute set is null");
            return;
        }

        var maxHealth = _attributeSet.GetValue(AttributeType.MaxHP);
        
        bool isSmooth = true;
        //현재 체력 변경
        if(newHealth == maxHealth)
        {
            isSmooth = false;
        }

        _healthBar.SetFillAmount(newHealth / maxHealth, isSmooth);
        _healthBar.SetValue(newHealth, maxHealth);
    }

    public void Show()
    {
        _canvasGroup.DOKill();
        gameObject.SetActive(true);
        _canvasGroup.DOFade(1f, 0.5f);
    }

    public void Hide()
    {
        _canvasGroup.DOKill();
        _canvasGroup.DOFade(0f, 0.5f).OnComplete(() =>
        {
            gameObject.SetActive(false);
        });
    }
}
