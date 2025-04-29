using System;
using System.Collections.Generic;

[Serializable]
public class PlayerStat
{
    public AttributePair MaxHP;
    public AttributePair HP;
    public AttributePair Strength;
    public AttributePair Intelligence;
    public AttributePair CriticalRate;
    public AttributePair Defense;
    public AttributePair CriticalDamage;
    public AttributePair Damage;
    public AttributePair MoveSpeed;
    public AttributePair AttackSpeed;
    public AttributePair Impluse;
    public AttributePair ImpulseThreshold;
    public AttributePair MaxSkillGauge;
    public AttributePair SkillGauge;
    
    // Dictionary에서 PlayerStat으로 변환하는 정적 메서드
    public static PlayerStat FromDictionary(Dictionary<AttributeType, Attribute> attributeDictionary)
    {
        PlayerStat stat = new PlayerStat();
        
        // 각 속성 매핑
        foreach (var entry in attributeDictionary)
        {
            switch (entry.Key)
            {
                case AttributeType.MaxHP:
                    stat.MaxHP = new AttributePair(AttributeType.MaxHP, entry.Value.GetValue());
                    break;
                case AttributeType.HP:
                    stat.HP = new AttributePair(AttributeType.HP, entry.Value.GetValue());
                    break;
                case AttributeType.Strength:
                    stat.Strength = new AttributePair(AttributeType.Strength, entry.Value.GetValue());
                    break;
                case AttributeType.Intelligence:
                    stat.Intelligence = new AttributePair(AttributeType.Intelligence, entry.Value.GetValue());
                    break;
                case AttributeType.CriticalRate:
                    stat.CriticalRate = new AttributePair(AttributeType.CriticalRate, entry.Value.GetValue());
                    break;
                case AttributeType.Defense:
                    stat.Defense = new AttributePair(AttributeType.Defense, entry.Value.GetValue());
                    break;
                case AttributeType.CriticalDamage:
                    stat.CriticalDamage = new AttributePair(AttributeType.CriticalDamage, entry.Value.GetValue());
                    break;
                case AttributeType.Damage:
                    stat.Damage = new AttributePair(AttributeType.Damage, entry.Value.GetValue());
                    break;
                case AttributeType.MoveSpeed:
                    stat.MoveSpeed = new AttributePair(AttributeType.MoveSpeed, entry.Value.GetValue());
                    break;
                case AttributeType.AttackSpeed:
                    stat.AttackSpeed = new AttributePair(AttributeType.AttackSpeed, entry.Value.GetValue());
                    break;
                case AttributeType.Impulse:
                    stat.Impluse = new AttributePair(AttributeType.Impulse, entry.Value.GetValue());
                    break;
                case AttributeType.EndureImpulse:
                    stat.ImpulseThreshold = new AttributePair(AttributeType.EndureImpulse, entry.Value.GetValue());
                    break;
                case AttributeType.MaxSkillGauge:
                    stat.MaxSkillGauge = new AttributePair(AttributeType.MaxSkillGauge, entry.Value.GetValue());
                    break;
                case AttributeType.SkillGauge:
                    stat.SkillGauge = new AttributePair(AttributeType.SkillGauge, entry.Value.GetValue());
                    break;
            }
        }
        
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
}