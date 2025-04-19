using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Serialization;

public enum EffectType
{
    Instant, //어트리뷰트에 즉각적으로 적용되는 게임플레이 이펙트. BaseValue를 수정 ex) 데미지, 포션, 스탯 증가 등
    Duration, // 지정한 시간 동안 동작하는 게임플레이 이펙트. CurrentValue를 수정 ex) 버프, 디버프, 틱 데미지
    Infinite // 명시적으로 종료하지 않으면 계속 동작하는 게임플레이 이펙트. CurrentValue를 수정 ex) 방에 입장시 체력 -20 나가면 해제 등
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
