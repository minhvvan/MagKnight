using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public enum AttributeType
{
    MaxHP,
    HP,
    STR,
    INT,
    DEF,
    LUK,
    CRT,
    DMG,
    SPD,
    BAS,
    ATK, // Enemybase공격력
    MAXRES, // 최대저항력
    RES,
    GOLD, // 드롭 골드량
}

/// <summary>
/// ⚠ 내부 전용 클래스입니다. 외부에서 직접 접근하지 마세요.
/// 반드시 <see cref="AbilitySystem"/>을 통해 Attribute를 수정해야 합니다.
/// </summary>
[System.Serializable]
public class Attribute
{
    public Action<float> OnPreModify; // amount 값을 value에 적용하기 전에 전처리
    public float Value;
    public Action OnPostModify; // 바뀐 value 값을 후처리
    

    public void Modify(float amount)
    {
        OnPreModify?.Invoke(amount);
        Value += amount;
        OnPostModify?.Invoke();
    }

    public void Set(float value)
    {
        OnPreModify?.Invoke(value);
        Value = value;
        OnPostModify?.Invoke();
    }
}


