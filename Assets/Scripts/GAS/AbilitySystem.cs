using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class AbilitySystem : MonoBehaviour
{
    [SerializeReference, SubclassPicker] public AttributeSet Attributes;
    
    private List<GameplayEffect> _activatedEffects = new List<GameplayEffect>();

    public void AddAttribute(AttributeType type, float value)
    {
        Attributes.AddAttribute(type, value);
    }
    
    
    // effect 개념으로 관리하고 싶은 것은 ApplyEffect를 한다.
    // e.g. 아티팩트 효과, 버프 디버프 등
    public void ApplyEffect(GameplayEffect gameplayEffect)
    {
        AttributeType attributeType = gameplayEffect.attributeType;
        
        Attributes.Modify(attributeType, gameplayEffect.amount, gameplayEffect.effectType);
        
        switch (gameplayEffect.effectType)
        {
            case EffectType.Instant:
                break;
            case EffectType.Duration:
                RemoveAfterDuration(gameplayEffect).Forget();
                break;
            case EffectType.Infinite:
                //RemoveAfterDuration(gameplayEffect).Forget();
                break;
        }
        
        Attributes.PostGameplayEffectExecute(gameplayEffect);
        
        if(gameplayEffect.tracking) _activatedEffects.Add(gameplayEffect);
    }
    
    public void RemoveEffect(GameplayEffect gameplayEffect)
    {
        if (gameplayEffect.tracking)
        {
            if (_activatedEffects.Contains(gameplayEffect))
            {
                Attributes.Modify(gameplayEffect.attributeType, -gameplayEffect.appliedAmount, gameplayEffect.effectType);
                _activatedEffects.Remove(gameplayEffect);
            }
            else // tracking 중인데 _activatedEffect가 없을수도
            {
                Debug.Log("해당 이펙트는 이미 제거되었습니다");
            }
        }
        else
            Attributes.Modify(gameplayEffect.attributeType, - gameplayEffect.appliedAmount,gameplayEffect.effectType);
    }

    // Attribute의 CurrentValue를 가져옴
    public float GetValue(AttributeType type)
    {
        return Attributes.GetValue(type);
    }

    // Attribute의 BaseValue를 세팅함
    public void SetValue(AttributeType type, float value)
    {
        Attributes.SetValue(type, value);
    }
    
    //Duration에서 사용됨
    private async UniTask RemoveAfterDuration(GameplayEffect gameplayEffect)
    {
        await UniTask.WaitForSeconds(gameplayEffect.duration);
        RemoveEffect(gameplayEffect);
    }
}

