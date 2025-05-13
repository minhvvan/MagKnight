using System;
using System.Collections.Generic;
using UnityEngine.Serialization;

[Serializable]
public class PlayerStat
{
    public AttributePair MaxHP;
    public AttributePair HP;
    public AttributePair Strength;
    public AttributePair CriticalRate;
    public AttributePair Defense;
    public AttributePair CriticalDamage;
    public AttributePair Damage;
    public AttributePair MoveSpeed;
    public AttributePair AttackSpeed;
    public AttributePair Impulse;
    public AttributePair EndureImpulse;
    public AttributePair MaxSkillGauge;
    public AttributePair SkillGauge;
    public AttributePair MagneticRange;
    public AttributePair MagneticPower;
    
    // Dictionary에서 PlayerStat으로 변환하는 정적 메서드
    public static PlayerStat FromDictionary(PlayerStat playerDataStat, Dictionary<AttributeType, Attribute> attributeDictionary)
    {
        PlayerStat stat = (PlayerStat)playerDataStat.MemberwiseClone();
        stat.HP = new AttributePair(AttributeType.HP, attributeDictionary[AttributeType.HP].GetValue());
        stat.SkillGauge = new AttributePair(AttributeType.SkillGauge, attributeDictionary[AttributeType.SkillGauge].GetValue());
        
        return stat;
    }

    public bool IsValid()
    {
        // 모든 속성의 키를 해시셋에 추가하여 중복 확인
        HashSet<AttributeType> attributeTypes = new HashSet<AttributeType>();
    
        // 리플렉션을 사용하여 모든 필드 가져오기
        var fields = GetType().GetFields(System.Reflection.BindingFlags.Public | 
                                         System.Reflection.BindingFlags.Instance);
    
        foreach (var field in fields)
        {
            // AttributePair 타입의 필드만 확인
            if (field.FieldType == typeof(AttributePair))
            {
                // 필드 값 가져오기
                AttributePair attributePair = (AttributePair)field.GetValue(this);
            
                // Key 값이 이미 있는지 확인
                if (!attributeTypes.Add(attributePair.Key))
                {
                    // 중복된 키 발견
                    return false;
                }
            }
        }
    
        // 모든 필드의 키가 중복 없이 추가되었으면 true 반환
        return true;
    }

    public PlayerStat DeepCopy()
    {
        var copy = (PlayerStat)MemberwiseClone();
        // copy.MaxHP = MaxHP.DeepCopy();
        // copy.HP = HP.DeepCopy();
        // copy.Strength = Strength.DeepCopy();
        // copy.CriticalRate = CriticalRate.DeepCopy();
        // copy.Defense = Defense.DeepCopy();
        // copy.CriticalDamage = CriticalDamage.DeepCopy();
        // copy.Damage = Damage.DeepCopy();
        // copy.MoveSpeed = MoveSpeed.DeepCopy();
        // copy.AttackSpeed = AttackSpeed.DeepCopy();
        // copy.Impulse = Impulse.DeepCopy();
        // copy.EndureImpulse = EndureImpulse.DeepCopy();
        // copy.MaxSkillGauge = MaxSkillGauge.DeepCopy();
        // copy.SkillGauge = SkillGauge.DeepCopy();
        // copy.MagneticRange = MagneticRange.DeepCopy();
        // copy.MagneticPower = MagneticPower.DeepCopy();
        return copy;
    }
}

[Serializable]
public struct AttributePair
{
    public AttributeType Key;
    public float Value;
    
    public AttributePair(AttributeType key, float value)
    {
        Key = key;
        Value = value;
    }

    public AttributePair DeepCopy()
    {
        return (AttributePair)MemberwiseClone();
    }
}