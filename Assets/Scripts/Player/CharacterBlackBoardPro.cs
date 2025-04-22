using System.Collections;
using System.Collections.Generic;
using hvvan;
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
        Initialize();
    }
    
    private void Initialize()
    {
        _playerController = GetComponent<PlayerController>();

        abilitySystem = GetComponent<AbilitySystem>();
        abilitySystem.InitializeFromPlayerStat(GameManager.Instance.PlayerStats);

        PlayerAttributeSet playerAttributeSet = abilitySystem.Attributes as PlayerAttributeSet;
        if (playerAttributeSet != null)
        {
            playerAttributeSet.OnDead += () =>
            {
                _playerController.Death();
            };
        }
    }

    public AbilitySystem GetAbilitySystem() => abilitySystem;
}
