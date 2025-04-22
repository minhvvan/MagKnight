using System;
using System.Collections;
using System.Collections.Generic;
using AYellowpaper.SerializedCollections;
using Cysharp.Threading.Tasks;
using Managers;
using UnityEngine;

public class AbilitySystem : MonoBehaviour
{
    [SerializeReference, SubclassPicker] public AttributeSet Attributes;
    
    // 기존 GameplayEffect는 가지면서 기존 GameplayEffect에 영향을 주지않기 위해 Hash를 가져와서 사용
    // 저장되는 GameplayEffect는 실제 적용된 Gameplay의 Instance
    // Remove를 요청할 때는 요청자는 단순히 나의 GE를 삭제해달라고 요청해주면 된다.
    [SerializeField] SerializedDictionary<int, GameplayEffect> _activatedEffects = new SerializedDictionary<int, GameplayEffect>();

    // PlayerStat -> Attribute
    public void InitializeFromPlayerStat(PlayerStat playerStat = null)
    {
        if (playerStat == null)
        {
            Debug.LogError("PlayerStat is not assigned!");
            return;
        }
    
        // PlayerStat의 모든 필드를 순회
        foreach (var field in typeof(PlayerStat).GetFields())
        {
            // 필드 값을 AttributePair로 가져옴
            AttributePair attributePair = (AttributePair)field.GetValue(playerStat);
        
            // AbilitySystem에 값 추가
            AddAttribute(attributePair.Key, attributePair.Value);
        }
    }
    
    public void AddAttribute(AttributeType type, float value)
    {
        Attributes.AddAttribute(type, value);
    }
    
    
    // effect 개념으로 관리하고 싶은 것은 ApplyEffect를 한다.
    // e.g. 아티팩트 효과, 버프 디버프 등
    public void ApplyEffect(GameplayEffect gameplayEffect)
    {
        // instance로 만드는 이유 : gameplayEffect를 직접적으로 수정하지 않도록
        // AttributeSet 안에 PreAttributeChange에서 수정 위험 요소 있음
        var instanceGE = gameplayEffect.DeepCopy();
        
        
        Attributes.Modify(instanceGE);
        
        
        switch (gameplayEffect.effectType)
        {
            case EffectType.Instant:
                break;
            case EffectType.Duration:
                RemoveAfterDuration(instanceGE).Forget();
                break;
            case EffectType.Infinite:
                //RemoveAfterDuration(gameplayEffect).Forget();
                break;
        }

        if (instanceGE.tracking)
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
        if (gameplayEffect.tracking)
        {
            if (_activatedEffects.ContainsKey(gameplayEffect.GetHashCode()))
            {
                _activatedEffects[gameplayEffect.GetHashCode()].amount = -_activatedEffects[gameplayEffect.GetHashCode()].amount; 
                Attributes.Modify(_activatedEffects[gameplayEffect.GetHashCode()]);
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

    // Attribute의 CurrentValue를 가져옴
    public float GetValue(AttributeType type)
    {
        return Attributes.GetValue(type);
    }
    
    //Duration에서 사용됨
    private async UniTask RemoveAfterDuration(GameplayEffect gameplayEffect)
    {
        await UniTask.WaitForSeconds(gameplayEffect.duration);
        RemoveEffect(gameplayEffect);
    }
}

