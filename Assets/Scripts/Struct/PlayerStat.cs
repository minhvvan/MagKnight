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
            }
        }
        
        return stat;
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