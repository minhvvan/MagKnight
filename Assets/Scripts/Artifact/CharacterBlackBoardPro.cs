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
        abilitySystem.AddAttribute(AttributeType.MaxHP, 100);
        abilitySystem.AddAttribute(AttributeType.HP, 100, onPostModify:UpdateHp);
        abilitySystem.AddAttribute(AttributeType.STR, 10);
        abilitySystem.AddAttribute(AttributeType.INT, 10);
        abilitySystem.AddAttribute(AttributeType.LUK, 10);
        abilitySystem.AddAttribute(AttributeType.DEF, 10);
        abilitySystem.AddAttribute(AttributeType.CRT, 10);
        abilitySystem.AddAttribute(AttributeType.DMG, 10);
        abilitySystem.AddAttribute(AttributeType.SPD, 10);
        abilitySystem.AddAttribute(AttributeType.BAS, 10);
    }

    public AbilitySystem GetAbilitySystem() => abilitySystem;

    void UpdateHp()
    {
        Debug.Log(abilitySystem.GetValue(AttributeType.HP));
        if (abilitySystem.GetValue(AttributeType.HP) > abilitySystem.GetValue(AttributeType.MaxHP))
        {
            abilitySystem.SetValue(AttributeType.HP, abilitySystem.GetValue(AttributeType.MaxHP));
            return;
        }

        if (abilitySystem.GetValue(AttributeType.HP) <= 0)
        {
            // Dead
            return;
        }
        
        // TODO: HP바 업데이트
    }
}
