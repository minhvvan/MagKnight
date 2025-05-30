using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

// Attribute를 Type으로 관리
public enum AttributeType
{
    MaxHP = 0,
    HP = 1,
    Strength = 2, // 힘
    Defense = 3, // 방어력
    CriticalRate= 4, // 치명타 확률
    CriticalDamage = 5, // 치명타 피해량
    MoveSpeed = 6, // 이동속도
    AttackSpeed = 7, // 공격속도
    Damage = 8, // 메타 Attribute, 받은 피해량
    MaxResistance = 9, // 최대저항력
    Resistance = 10,
    ResistanceDamage = 11,
    Gold = 12, // 드롭 골드량
    Impulse = 13, // 충격량
    EndureImpulse = 14, // 충격량 저항값
    MaxSkillGauge = 15,
    SkillGauge = 16,
    MagneticRange = 17, // 마그네틱 범위
    MagneticPower = 18, // 마그네틱 힘
}

/// <summary>
/// ⚠ 내부 전용 클래스입니다. 외부에서 직접 접근하지 마세요.
/// 반드시 <see cref="AbilitySystem"/>을 통해 Attribute를 수정해야 합니다.
/// </summary>
[System.Serializable]
public class Attribute
{
    //public Action<float> OnPreModify; // amount 값을 value에 적용하기 전에 전처리
    // 기본 값, 영구히 적용되는 고정 스탯 값을 관리하는데 사용
    [SerializeField] private float BaseValue;
    // 변동값, 버프등으로 임시적으로 변동된 값을 관리하는데 사용
    [SerializeField] private float CurrentValue;
    private Action<float> ChangeAction;
    
    // Attribute 초기화
    public void InitAttribute(float value)
    {
        BaseValue = value;
        CurrentValue = BaseValue;
    }
    
    // GameplayEffect Instant Type 이외 호출
    public void ModifyCurrentValue(float amount)
    {
        CurrentValue += amount;
        ChangeAction?.Invoke(CurrentValue);
    }
    
    // GameplayEffect Instant Type시 호출
    public void ModifyBaseValue(float amount)
    {
        BaseValue += amount;
        ModifyCurrentValue(amount);
    }

    // Attribute를 Set하는 함수
    // BaseValue를 value값으로 설정하고 그 값차이만큼 CurrentValue Update
    public void SetValue(float value)
    {
        var gapValue = value - BaseValue;
        CurrentValue = BaseValue + gapValue;
        BaseValue = value;
        ChangeAction?.Invoke(CurrentValue);
    }

    public void SetCurrentValue(float value)
    {
        CurrentValue = value;
    }
    
    public float GetValue()
    {
        return CurrentValue;
    }

    public float GetBaseValue()
    {
        return BaseValue;
    }

    // Delegate 구독 함수
    public void DelegateChangeAction(Action<float> action)
    {
        ChangeAction += action;
    }

    public void UnsubscribeChangeAction(Action<float> action)
    {
        ChangeAction -= action;
    }
}


