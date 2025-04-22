using System;
using System.Collections;
using System.Collections.Generic;
using AYellowpaper.SerializedCollections;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Random = UnityEngine.Random;

public class AbilitySystem : MonoBehaviour
{
    [SerializeReference, SubclassPicker] public AttributeSet Attributes;
    
    // 기존 GameplayEffect는 가지면서 기존 GameplayEffect에 영향을 주지않기 위해 Hash를 가져와서 사용
    // 저장되는 GameplayEffect는 실제 적용된 Gameplay의 Instance
    // Remove를 요청할 때는 요청자는 단순히 나의 GE를 삭제해달라고 요청해주면 된다.
    [SerializeField] SerializedDictionary<int, GameplayEffect> _activatedEffects = new SerializedDictionary<int, GameplayEffect>();
    
    // Passive를 담고 있는 Dicitionary
    [SerializeField] SerializedDictionary<int, PassiveEffectData> _registeredPassiveEffects = new SerializedDictionary<int, PassiveEffectData>();


    #region Attribute
    
    public void AddAttribute(AttributeType type, float value)
    {
        Attributes.AddAttribute(type, value);
    }

    // Attribute의 CurrentValue를 가져옴
    public float GetValue(AttributeType type)
    {
        return Attributes.GetValue(type);
    }
    
    #endregion
    
    #region Effect
    
    // effect 개념으로 관리하고 싶은 것은 ApplyEffect를 한다.
    // e.g. 아티팩트 효과, 버프 디버프 등
    public void ApplyEffect(GameplayEffect gameplayEffect)
    {
        // instance로 만드는 이유 : gameplayEffect를 직접적으로 수정하지 않도록
        // AttributeSet 안에 PreAttributeChange에서 수정 위험 요소 있음
        var instanceGE = gameplayEffect.DeepCopy();
        
        
        Attributes.Modify(instanceGE);
        
        if(gameplayEffect.effectType == EffectType.Duration)
        {
            if (gameplayEffect.period > 0f)
            {
                ApplyPeriodicEffect(instanceGE);
            }
            else
            {
                RemoveAfterDuration(instanceGE);
            }
        }
        
        // Infinite는 항상 저장
        if (instanceGE.tracking || gameplayEffect.effectType == EffectType.Infinite)
        {
            // 이미 존재하는 이펙트면
            if (!_activatedEffects.TryAdd(gameplayEffect.GetHashCode(), instanceGE))
            {
                // 중첩되도록 amount 추가
                _activatedEffects[gameplayEffect.GetHashCode()].amount += instanceGE.amount;
            }
        }
    }
    
    public void RemoveEffect(GameplayEffect gameplayEffect)
    {
        if (gameplayEffect.tracking || gameplayEffect.effectType == EffectType.Infinite)
        {
            if (_activatedEffects.TryGetValue(gameplayEffect.GetHashCode(), out var effect))
            {
                effect.amount = -effect.amount; 
                Attributes.Modify(effect);
                _activatedEffects.Remove(gameplayEffect.GetHashCode());
            }
            else // tracking 중인데 _activatedEffect가 없을수도
            {
                Debug.Log("해당 이펙트는 이미 제거되었습니다");
            }
        }
        else
        {
            var instanceGE = gameplayEffect.DeepCopy();
            instanceGE.amount = -instanceGE.amount;
            Attributes.Modify(instanceGE);
        }
    }
    
    //Duration에서 사용됨
    private async UniTask RemoveAfterDuration(GameplayEffect gameplayEffect)
    {
        await UniTask.WaitForSeconds(gameplayEffect.duration);
        RemoveEffect(gameplayEffect);
    }
    
    private async UniTaskVoid ApplyPeriodicEffect(GameplayEffect gameplayEffect)
    {
        float elapsed = 0f;
        while (elapsed < gameplayEffect.duration)
        {
            var tickEffect = gameplayEffect.DeepCopy();
            Attributes.Modify(tickEffect);

            await UniTask.Delay(System.TimeSpan.FromSeconds(gameplayEffect.period));
            elapsed += gameplayEffect.period;
        }
    }
    
    #endregion
    
    #region Passive
    
    public void RegisterPassiveEffect(PassiveEffectData data)
    {
        _registeredPassiveEffects.TryAdd(data.GetHashCode(), data);
    }

    public void RemovePassiveEffect(PassiveEffectData data)
    {
        if(_registeredPassiveEffects.ContainsKey(data.GetHashCode()))
            _registeredPassiveEffects.Remove(data.GetHashCode());
        else
        {
            Debug.Log("해당 이펙트는 이미 제거되었습니다");
        }
    }
    
    public void TriggerEvent(TriggerEventType eventType, AbilitySystem target)
    {
        foreach (var passiveEffect in _registeredPassiveEffects)
        {
            if (passiveEffect.Value.triggerEvent == eventType)
            {
                if (Random.value <= passiveEffect.Value.triggerChance && target != null)
                {
                    target.ApplyEffect(passiveEffect.Value.effect);
                }
            }   
        }
    }
    
    #endregion
    
}

