using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TextCore.Text;

[RequireComponent(typeof(AbilitySystem))]
public class CharacterBlackBoardPro : MonoBehaviour
{
    [SerializeField] private AbilitySystem abilitySystem;
    
    void Awake()
    {
        Initalized();
    }
    
    void Initalized()
    {
        abilitySystem = GetComponent<AbilitySystem>();
        
        abilitySystem.Attributes.AddAttribute(AttributeType.MaxHP, 100);
        abilitySystem.Attributes.AddAttribute(AttributeType.HP, 100, onPostModify:UpdateHp);
        abilitySystem.Attributes.AddAttribute(AttributeType.STR, 10);
        abilitySystem.Attributes.AddAttribute(AttributeType.INT, 10);
        abilitySystem.Attributes.AddAttribute(AttributeType.LUK, 10);
        abilitySystem.Attributes.AddAttribute(AttributeType.DEF, 10);
        abilitySystem.Attributes.AddAttribute(AttributeType.CRT, 10);
        abilitySystem.Attributes.AddAttribute(AttributeType.DMG, 10);
        abilitySystem.Attributes.AddAttribute(AttributeType.SPD, 10);
        abilitySystem.Attributes.AddAttribute(AttributeType.BAS, 10);
    }

    void UpdateHp()
    {
        if (abilitySystem.Attributes.GetValue(AttributeType.HP) > abilitySystem.Attributes.GetValue(AttributeType.MaxHP))
        {
            abilitySystem.Attributes.Set(AttributeType.HP, abilitySystem.Attributes.GetValue(AttributeType.MaxHP));
            return;
        }

        if (abilitySystem.Attributes.GetValue(AttributeType.HP) <= 0)
        {
            // Dead
            return;
        }
        
        // TODO: HP바 업데이트
    }
}
