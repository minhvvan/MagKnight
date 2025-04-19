using System;
using System.Collections;
using System.Collections.Generic;
using AYellowpaper.SerializedCollections;
using UnityEngine;

/// <summary>
/// ⚠ 내부 전용 클래스입니다. 외부에서 직접 접근하지 마세요.
/// 반드시 <see cref="AbilitySystem"/>을 통해 Attribute를 수정해야 합니다.
/// </summary>
///


public abstract class AttributeSet
{
    [SerializeField] SerializedDictionary<AttributeType, Attribute> attributeDictionary = new SerializedDictionary<AttributeType, Attribute>();

    public void AddAttribute(AttributeType type , float value)
    {
        if (attributeDictionary.ContainsKey(type))
        {
            Debug.LogWarning($"{type}은 이미 존재하는 Attribute입니다");
            return;
        }
        Attribute instance = new Attribute();
        instance.InitAttribute(value);
        attributeDictionary.Add(type, instance);
    }
    
    // CurrentValue를 Return
    public float GetValue(AttributeType type)
    {
        if (attributeDictionary.ContainsKey(type))
            return attributeDictionary[type].GetValue();
        
        Debug.LogError($"{type} not found");
        return 0;
    }
    
    // GameplayEffect 적용시 호출
    public void Modify(GameplayEffect gameplayEffect)
    {
        var type = gameplayEffect.attributeType;
        var effectType = gameplayEffect.effectType;
        
        if (attributeDictionary.ContainsKey(type))
        {
            // PreAttributeChange에 의해 실제로 적용된 값으로 GE에 Update
            // 이를 통해 Remove시 실제 적용된 값만큼 다시 적용 가능
            gameplayEffect.amount = PreAttributeChange(type, gameplayEffect.amount);
            

            // 만약 EffectType이 Instant면 BaseValue를 수정
            if(effectType == EffectType.Instant)
                attributeDictionary[type].ModifyBaseValue(gameplayEffect.amount);
            // 그 외 CurrentValue를 수정
            else
                attributeDictionary[type].ModifyCurrentValue(gameplayEffect.amount);
            
            PostGameplayEffectExecute(gameplayEffect);
        }
        else
        {
            Debug.LogError($"{type} not found");
        }
    }
    
    // BaseValue를 수정 (EffectType Instant에서 사용됨)
    public void SetValue(AttributeType type, float value)
    {
        if (attributeDictionary.ContainsKey(type))
        {
            attributeDictionary[type].SetValue(value);
        }
    }

    // Attribute 변경 시 Action 추가하기 위한 함수
    public void DelegateAttributeChanged(AttributeType type, Action action)
    {
        if (attributeDictionary.ContainsKey(type))
        {
            attributeDictionary[type].DelegateChangeAction(action);
        }
    }

    // Modify 적용 전에 호출
    protected virtual float PreAttributeChange(AttributeType type, float newValue)
    {
        return newValue;
    }

    // Effect 적용 후 호출
    protected virtual void PostGameplayEffectExecute(GameplayEffect effect){}
}

