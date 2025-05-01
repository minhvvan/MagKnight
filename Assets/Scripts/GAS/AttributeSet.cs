using System;
using System.Collections;
using System.Collections.Generic;
using AYellowpaper.SerializedCollections;
using UnityEngine;
using UnityEngine.Serialization;

/// <summary>
/// ⚠ 내부 전용 클래스입니다. 외부에서 직접 접근하지 마세요.
/// 반드시 <see cref="AbilitySystem"/>을 통해 Attribute를 수정해야 합니다.
/// </summary>
///


public abstract class AttributeSet
{
    protected Dictionary<AttributeType, Attribute> _attributeDictionary = new Dictionary<AttributeType, Attribute>();
    protected HashSet<string> tag = new HashSet<string>();
    
    public void AddAttribute(AttributeType type , float value)
    {
        if (_attributeDictionary.ContainsKey(type))
        {
            Debug.LogWarning($"{type}은 이미 존재하는 Attribute입니다");
            return;
        }
        Attribute instance = new Attribute();
        instance.InitAttribute(value);
        _attributeDictionary.Add(type, instance);
    }
    
    // CurrentValue를 Return
    public float GetValue(AttributeType type)
    {
        if (_attributeDictionary.ContainsKey(type))
            return _attributeDictionary[type].GetValue();
        
        Debug.LogError($"{type} not found");
        return 0;
    }
    
    // GameplayEffect 적용시 호출
    public void Modify(GameplayEffect gameplayEffect)
    {
        var type = gameplayEffect.attributeType;
        var effectType = gameplayEffect.effectType;
        
        if (_attributeDictionary.ContainsKey(type))
        {
            // PreAttributeChange에 의해 실제로 적용된 값으로 GE에 Update
            // 이를 통해 Remove시 실제 적용된 값만큼 다시 적용 가능
            gameplayEffect.amount = PreAttributeChange(type, gameplayEffect.amount);
            

            // 만약 EffectType이 Instant또는 Period를 가지는 Duration이면 BaseValue를 수정
            if(effectType == EffectType.Instant || (effectType == EffectType.Duration && gameplayEffect.period > 0))
                _attributeDictionary[type].ModifyBaseValue(gameplayEffect.amount);
            // 그 외 CurrentValue를 수정
            else
                _attributeDictionary[type].ModifyCurrentValue(gameplayEffect.amount);
            
            PostGameplayEffectExecute(gameplayEffect);
        }
        else
        {
            Debug.LogError($"{type} not found");
        }
    }
    
    // BaseValue를 수정 (EffectType Instant에서 사용됨)
    protected void SetValue(AttributeType type, float value)
    {
        if (_attributeDictionary.ContainsKey(type))
        {
            _attributeDictionary[type].SetValue(value);
        }
    }

    protected void SetCurrentValue(AttributeType type, float value)
    {
        if (_attributeDictionary.ContainsKey(type))
        {
            _attributeDictionary[type].SetCurrentValue(value);
        }
    }

    // Attribute 변경 시 Action 추가하기 위한 함수
    public void SubscribeAttributeChanged(AttributeType type, Action<float> action)
    {
        if (_attributeDictionary.ContainsKey(type))
        {
            _attributeDictionary[type].DelegateChangeAction(action);
        }
    }

    // Modify 적용 전에 호출
    protected virtual float PreAttributeChange(AttributeType type, float newValue)
    {
        return newValue;
    }

    // Effect 적용 후 호출
    protected virtual void PostGameplayEffectExecute(GameplayEffect effect){}
    
    // attributeDictionary의 키 목록을 가져오는 getter 추가
    public IEnumerable<AttributeType> GetAttributeTypes()
    {
        return _attributeDictionary.Keys;
    }
    
    // attributeDictionary가 특정 키를 포함하는지 확인하는 메서드
    public bool HasAttribute(AttributeType type)
    {
        return _attributeDictionary.ContainsKey(type);
    }

    public void ClearAllAttributes()
    {
        _attributeDictionary.Clear();
    }

    public void AddTag(string tag)
    {
        this.tag.Add(tag);
    }

    public void DeleteTag(string tag)
    {
        this.tag.Remove(tag);
    }
    
    public void ClearTag()
    {
        tag = null;
    }
}

