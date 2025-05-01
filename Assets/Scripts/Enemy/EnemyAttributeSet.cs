using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAttributeSet : AttributeSet
{
    public Action<int> OnPhaseChange;
    public Action OnDeath;
    public Action OnStagger;
    public Action<ExtraData> OnHit;
    public Action OnDamage;
    
    private bool _phase70Triggered = false;
    private bool _phase30Triggered = false;
    
    protected override float PreAttributeChange(AttributeType type, float newValue)
    {
        float returnValue = newValue;

        if (type == AttributeType.Damage)
        {
            // 데미지가 음수면 0으로 처리
            returnValue = newValue < 0 ? 0 : newValue;
            
            // ex) 무적효과
            // if(무적효과 적용시)
            // returnValue = 0  -> 데미지를 0으로 초기화
        }
        
        if (type == AttributeType.ResistanceDamage)
        {
            // 데미지가 음수면 0으로 처리
            returnValue = newValue < 0 ? 0 : newValue;
        }
        
        return returnValue;
    }

    protected override void PostGameplayEffectExecute(GameplayEffect effect)
    {
        // 최대체력 증가시 그만큼 HP도 증가
        if (effect.attributeType == AttributeType.MaxHP)
        {
            SetValue(AttributeType.HP, GetValue(AttributeType.MaxHP));
        }

        // 체력 변경시 Clamp값으로
        if (effect.attributeType == AttributeType.HP)
        {
            SetValue(AttributeType.HP, Mathf.Clamp(GetValue(AttributeType.HP), 0f, GetValue(AttributeType.MaxHP)));
        }

        if (effect.attributeType == AttributeType.Damage)
        {
            // 체력은 항상 데미지를 통해서만 접근
            // 현재 체력에서 데미지를 뺀 값을 적용해서 Hp업데이트, 단 0보다 작거나, MaxHp보다 크지 않게
            SetValue(AttributeType.HP, Mathf.Clamp(GetValue(AttributeType.HP) - effect.amount, 0f, GetValue(AttributeType.MaxHP)));
            if(effect.extraData.sourceTransform != null)
                OnHit?.Invoke(effect.extraData);
            // 예시 실드가 있다면?
            // SetValue(AttributeType.HP, GetValue(AttributeType.Shield) + GetValue(AttributeType.HP) - effect.amount);
            OnDamage?.Invoke();
            
            if (GetValue(AttributeType.HP) <= 0)
            {
                // TODO : 사망로직
                OnDeath?.Invoke();
                return;
                // ex) OnDead?.Invoke(); OnDead는 PlayerAttribute에서 선언
            }
            
            // 메타 어트리뷰트는 적용 후 바로 0으로 값을 초기화하도록 설정
            SetValue(AttributeType.Damage, 0);
        }

        // 최대체력 증가시 그만큼 HP도 증가
        if (effect.attributeType == AttributeType.MaxResistance)
        {
            SetValue(AttributeType.Resistance, GetValue(AttributeType.MaxResistance));
        }

        if (effect.attributeType == AttributeType.Resistance)
        {
            SetValue(AttributeType.Resistance,
                Mathf.Clamp(GetValue(AttributeType.Resistance), 0f, GetValue(AttributeType.MaxResistance)));
        }

        if (effect.attributeType == AttributeType.ResistanceDamage)
        {
            SetValue(AttributeType.Resistance, 
                Mathf.Clamp(GetValue(AttributeType.Resistance) - effect.amount, 0f, GetValue(AttributeType.MaxResistance)));
            
            if (GetValue(AttributeType.Resistance) <= 0)
            {
                SetValue(AttributeType.Resistance, GetValue(AttributeType.MaxResistance));
                OnStagger?.Invoke();
            }
            
            SetValue(AttributeType.ResistanceDamage, 0);
        }


        
        else if (!_phase30Triggered && GetValue(AttributeType.HP) <= GetValue(AttributeType.MaxHP) * 0.3f)
        {
            _phase30Triggered = true;
            OnPhaseChange?.Invoke(3);
        }
        else if (!_phase70Triggered && GetValue(AttributeType.HP) <= GetValue(AttributeType.MaxHP) * 0.7f)
        {
            _phase70Triggered = true;
            OnPhaseChange?.Invoke(2);
        }
    }
}
