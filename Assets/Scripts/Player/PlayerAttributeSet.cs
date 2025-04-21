using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace Jun
{
    public class PlayerAttributeSet : AttributeSet
    {
        public Action OnDead;

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
                SetValue(AttributeType.HP,Mathf.Clamp(GetValue(AttributeType.HP), 0f, GetValue(AttributeType.MaxHP)));
            }
            
            if (effect.attributeType == AttributeType.Damage)
            {
                // 체력은 항상 데미지를 통해서만 접근
                // 현재 체력에서 데미지를 뺀 값을 적용해서 Hp업데이트, 단 0보다 작거나, MaxHp보다 크지 않게
                SetValue(AttributeType.HP, Mathf.Clamp(GetValue(AttributeType.HP) - effect.amount, 0f, GetValue(AttributeType.MaxHP)));
                // 예시 실드가 있다면?
                // SetValue(AttributeType.HP, GetValue(AttributeType.Shield) + GetValue(AttributeType.HP) - effect.amount);
                
                // 메타 어트리뷰트는 적용 후 바로 0으로 값을 초기화하도록 설정
                SetValue(AttributeType.Damage, 0);
                
            }
            
            if (GetValue(AttributeType.HP) == 0)
            {
                // TODO : 사망로직
                Debug.Log("Player Dead");
                // ex) OnDead?.Invoke(); OnDead는 PlayerAttribute에서 선언
                OnDead?.Invoke();
            }
        }
    }   
}
