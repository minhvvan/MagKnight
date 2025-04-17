using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class AbilitySystem : MonoBehaviour
{
    public AttributeSet Attributes = new AttributeSet();
    
    private List<GameplayEffect> _activatedEffects = new List<GameplayEffect>();

    public void AddAttribute(AttributeType type, float value, Action<float> onPreModify = null,
        Action onPostModify = null)
    {
        Attributes.AddAttribute(type, value, onPreModify, onPostModify);
    }

    public void AddPreModify(AttributeType type, Action<float> onPreModify)
    {
        Attributes.AddPreModify(type, onPreModify);
    }
    
    public void AddPostModify(AttributeType type, Action onPostModify)
    {
        Attributes.AddPostModify(type, onPostModify);
    }
    
    // effect 개념으로 관리하고 싶은 것은 ApplyEffect를 한다.
    // e.g. 아티팩트 효과, 버프 디버프 등
    public void ApplyEffect(GameplayEffect gameplayEffect)
    {
        AttributeType attributeType = gameplayEffect.attributeType;
        
        // 실제 적용 값 저장
        float originalAttributeValue = Attributes.GetValue(attributeType);
        Attributes.Modify(attributeType, gameplayEffect.amount);
        gameplayEffect.appliedAmount = Attributes.GetValue(attributeType) - originalAttributeValue;
        
        switch (gameplayEffect.effectType)
        {
            case EffectType.Static:
                break;
            case EffectType.Buff:
                RemoveAfterDuration(gameplayEffect).Forget();
                break;
            case EffectType.Debuff:
                RemoveAfterDuration(gameplayEffect).Forget();
                break;
        }
        
        if(gameplayEffect.tracking) _activatedEffects.Add(gameplayEffect);
    }

    public void RemoveEffect(GameplayEffect gameplayEffect)
    {
        if (gameplayEffect.tracking)
        {
            if (_activatedEffects.Contains(gameplayEffect))
            {
                Attributes.Modify(gameplayEffect.attributeType, - gameplayEffect.appliedAmount);
                _activatedEffects.Remove(gameplayEffect);
            }
            else // tracking 중인데 _activatedEffect가 없을수도
            {
                Debug.Log("해당 이펙트는 이미 제거되었습니다");
            }
        }
        else
            Attributes.Modify(gameplayEffect.attributeType, - gameplayEffect.appliedAmount);
    }

    public float GetValue(AttributeType type)
    {
        return Attributes.GetValue(type);
    }

    public void SetValue(AttributeType type, float value)
    {
        Attributes.Set(type, value);
    }
    
    private async UniTask RemoveAfterDuration(GameplayEffect gameplayEffect)
    {
        await UniTask.WaitForSeconds(gameplayEffect.duration);
        RemoveEffect(gameplayEffect);
    }
}

