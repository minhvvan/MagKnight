using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EffectType
{
    Instant, //어트리뷰트에 즉각적으로 적용되는 게임플레이 이펙트. BaseValue를 수정 ex) 데미지, 포션, 스탯 증가 등
    Duration, // 지정한 시간 동안 동작하는 게임플레이 이펙트. CurrentValue를 수정 ex) 버프, 디버프, 틱 데미지
    Infinite // 명시적으로 종료하지 않으면 계속 동작하는 게임플레이 이펙트. CurrentValue를 수정 ex) 방에 입장시 체력 -20 나가면 해제 등
}

// 데미지 유형을 나타냄
public enum EffectDamageType
{
    Normal,
    Poison,
    Magnetic,  //여기서부터 미구현
    Fire,
    Ice,
    Electric,
    Lightning,
}

[System.Serializable]
public class GameplayEffect
{
    public EffectType effectType;
    public AttributeType attributeType;
    public float amount;
    public float duration;
    public float period = 0f; // 주기, Duration에서 틱 데미지와 같은 효과에 사용, BaseValue값을 변경, ex) duration : 1, period : 0.25일 때 총 5번의 효과 적용
    public DAMAGEType damageType = DAMAGEType.NORMAL; // 데미지 유형
    public bool tracking; // buff, 아이템같이 저장해두고 관리가 필요할 때 true
    

    public int maxStack = 1;
    [NonSerialized] public int currentStack;
    
    public ExtraData extraData = new ExtraData();
    
    public GameplayEffect(EffectType effectType, AttributeType attributeType, float amount, float duration = 0f, bool tracking = false)
    {
        this.effectType = effectType;
        this.attributeType = attributeType;
        this.amount = amount;
        this.duration = duration;
        this.tracking = tracking;
    }

    public GameplayEffect DeepCopy()
    {
        GameplayEffect copy = (GameplayEffect)MemberwiseClone();
        copy.extraData = extraData.DeepCopy();
        return copy;
    }
}


[System.Serializable]
public class ExtraData
{
    public Transform sourceTransform;
    public float weaponRange;
    public bool isCritical = false;
    public DAMAGEType damageType = DAMAGEType.NORMAL;
    public HitInfo hitInfo;
    public float finalAmount = 0;

    public ExtraData DeepCopy()
    {
        ExtraData copy = (ExtraData)MemberwiseClone();
        return copy;
    }
}
