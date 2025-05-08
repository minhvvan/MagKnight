using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PassiveEffectData
{
    [Range(0,1)]public float triggerChance; // 확률
    public TriggerEventType triggerEvent; // 트리거 발생할 이벤트
    public GameplayEffect effect;
    public bool isTarget; // 자신이 아닌 타겟으로 적용할 것인지 아닌지
    public bool hasCount;
    public int triggerCount;

    public PassiveEffectData DeepCopy()
    {
        var copy = (PassiveEffectData)this.MemberwiseClone();
        return copy;
    }
}

public enum TriggerEventType
{
    OnHit,
    OnDamage,
    OnAttack,
    OnDeath,
    OnMagnetic,
    OnSkill,
    OnParry,
}