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
    public SerializedDictionary<AttributeType, Attribute> attributeDictionary = new SerializedDictionary<AttributeType, Attribute>();

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
            return attributeDictionary[type].CurrentValue;
        
        Debug.LogError($"{type} not found");
        return 0;
    }
    
    // GameplayEffect 적용시 호출
    public void Modify(AttributeType type, float amount, EffectType effectType)
    {
        if (attributeDictionary.ContainsKey(type))
        {
            var newValue = PreAttributeChange(type, amount);

            // 만약 EffectType이 Instant면 BaseValue를 수정
            if(effectType == EffectType.Instant)
                attributeDictionary[type].ModifyBaseValue(newValue);
            // 그 외 CurrentValue를 수정
            else
                attributeDictionary[type].ModifyCurrentValue(newValue);
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

    public virtual float PreAttributeChange(AttributeType type, float newValue)
    {
        return newValue;
    }

    public virtual void PostGameplayEffectExecute(GameplayEffect effect){}
}

