using System.Collections;
using System.Collections.Generic;
using Jun;
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
        abilitySystem.AddAttribute(AttributeType.HP, 100);
        abilitySystem.AddAttribute(AttributeType.Strength, 10);
        abilitySystem.AddAttribute(AttributeType.Intelligence, 10);
        abilitySystem.AddAttribute(AttributeType.CriticalRate, 10);
        abilitySystem.AddAttribute(AttributeType.Defense, 10);
        abilitySystem.AddAttribute(AttributeType.CriticalDamage, 10);
        abilitySystem.AddAttribute(AttributeType.Damage, 0);
        abilitySystem.AddAttribute(AttributeType.MoveSpeed, 10);
        abilitySystem.AddAttribute(AttributeType.AttackSpeed, 10);
    }

    public AbilitySystem GetAbilitySystem() => abilitySystem;
}
