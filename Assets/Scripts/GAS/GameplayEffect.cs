using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Serialization;

public enum EffectType
{
    Static,
    Buff,
    Debuff
}

[System.Serializable]
public class GameplayEffect
{
    public EffectType effectType;
    public AttributeType attributeType;
    public float amount;
    public float duration;
    public bool tracking; // buff, 아이템같이 저장해두고 관리가 필요할 때 true

    // attribute에 실제로 effect를 적용시킬 때는 Modify에 의해 value 값이 그대로 적용되지 않을 수 있음
    // 나중에 remove를 할 때 예상하지 못하는 결과를 불러올 수 있다.
    // 즉 실제로 얼마나 변했는지를 따로 기록할 필요 있음
    public float appliedAmount;

    public GameplayEffect(EffectType effectType, AttributeType attributeType, float amount, float duration = 0f, bool tracking = false)
    {
        this.effectType = effectType;
        this.attributeType = attributeType;
        this.amount = amount;
        this.duration = duration;
        this.tracking = tracking;
    }
}
