using System.Collections;
using System.Collections.Generic;
using Jun;
using Moon;
using UnityEngine;
using UnityEngine.TextCore.Text;

[RequireComponent(typeof(AbilitySystem))]
public class CharacterBlackBoardPro : MonoBehaviour
{
    [SerializeField] private AbilitySystem abilitySystem;
    
    PlayerController _playerController;
    
    void Awake()
    {
        Initalized();
    }
    
    void Initalized()
    {
        _playerController = GetComponent<PlayerController>();

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

        PlayerAttributeSet playerAttributeSet = abilitySystem.Attributes as PlayerAttributeSet;
        if (playerAttributeSet != null)
        {
            playerAttributeSet.OnDead += () =>
            {
                _playerController.Death();
            };

            playerAttributeSet.OnDamaged += () =>
            {
                _playerController.Damaged();
            };
        }
    }

    public AbilitySystem GetAbilitySystem() => abilitySystem;
}
