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
}

public enum TriggerEventType
{
    OnHit,
    OnDamage,
    OnAttack,
    OnDeath,
}