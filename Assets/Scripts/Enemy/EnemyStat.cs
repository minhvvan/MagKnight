using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class EnemyStat
{
    public AttributePair MaxHP;
    public AttributePair HP;
    public AttributePair Strength;
    public AttributePair MoveSpeed;
    public AttributePair MaxResistance;
    public AttributePair Gold;
    public AttributePair Resistance;
    public AttributePair Damage;
    public AttributePair ResistanceDamage;
    public AttributePair Defense;
    
    public static EnemyStat FromDictionary(Dictionary<AttributeType, Attribute> attributeDictionary)
    {
        EnemyStat stat = new EnemyStat();
        
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
                case AttributeType.MoveSpeed:
                    stat.MoveSpeed = new AttributePair(AttributeType.MoveSpeed, entry.Value.GetValue());
                    break;
                case AttributeType.MaxResistance:
                    stat.MaxResistance = new AttributePair(AttributeType.MaxResistance, entry.Value.GetValue());
                    break;
                case AttributeType.Gold:
                    stat.Gold = new AttributePair(AttributeType.Gold, entry.Value.GetValue());
                    break;
                case AttributeType.Resistance:
                    stat.Resistance = new AttributePair(AttributeType.Resistance, entry.Value.GetValue());
                    break;
                case AttributeType.Damage:
                    stat.Damage = new AttributePair(AttributeType.Damage, entry.Value.GetValue());
                    break;
                case AttributeType.ResistanceDamage:
                    stat.ResistanceDamage = new AttributePair(AttributeType.ResistanceDamage, entry.Value.GetValue());
                    break;
                case AttributeType.Defense:
                    stat.Defense = new AttributePair(AttributeType.Defense, entry.Value.GetValue());
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
