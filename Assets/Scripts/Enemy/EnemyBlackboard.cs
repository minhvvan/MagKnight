using System;
using System.Collections.Generic;
using System.Threading;
using Unity.VisualScripting;
using UnityEngine;

public class EnemyBlackboard : MonoBehaviour
{
    [SerializeField] private EnemyDataSO _enemyDataSO;
    [SerializeField] private Enemy _enemy;
    public AbilitySystem abilitySystem;
    
    #region Data
    [HideInInspector] public string enemyName;
    [HideInInspector] public string description;
    [HideInInspector] public EnemyAttckType attackType;
    [HideInInspector] public EnemyAIType aiType;
    [HideInInspector] public float weight;
    [HideInInspector] public float startupTime;
    [HideInInspector] public float recoveryTime;
    [HideInInspector] public float staggerRecoveryTime;
    [HideInInspector] public List<EnemyAbilityType> abilities;
    [HideInInspector] public int appearanceFloor; // todo: 몹을 미리 배치하기 때문에 제거 가능성 높음
    [HideInInspector] public float projectileSpeed;
    [HideInInspector] public float attackRange;
    #endregion
    
    #region CurrentState
    
    [HideInInspector] public GameObject target;
    [HideInInspector] public LayerMask targetLayer;
    [HideInInspector] public IEnemyAI ai;
    #endregion
    
    #region CancellationToken

    [HideInInspector] public CancellationTokenSource actionRecoveryCancellation;
    [HideInInspector] public CancellationTokenSource staggerRecoveryCancellation;
    #endregion

    private void Awake()
    {
        Initialize();
    }
    
    public void Initialize()
    {
        InitializeAttributes();
        
        name = _enemyDataSO.enemyName;
        description = _enemyDataSO.description;
        attackType = _enemyDataSO.attckType;
        aiType = _enemyDataSO.aiType;
        startupTime = _enemyDataSO.startupTime;
        recoveryTime = _enemyDataSO.recoveryTime;
        staggerRecoveryTime = _enemyDataSO.staggerRecoveryTime;
        abilities = _enemyDataSO.abilities;
        appearanceFloor = _enemyDataSO.appearanceFloor;
        projectileSpeed = _enemyDataSO.projectileSpeed;
        attackRange = _enemyDataSO.attackRange;
        
        targetLayer = LayerMask.GetMask("Player");

        switch (aiType)
        {
            case EnemyAIType.MeleeNormal:
                ai = new MeleeNormalAI(_enemy);
                break;
        }
    }

    private void InitializeAttributes()
    {
        // 버프, 디버프, 상호작용 가능 stat은 attribute로 관리
        abilitySystem.Attributes.AddAttribute(AttributeType.MaxHP, _enemyDataSO.health);
        abilitySystem.Attributes.AddAttribute(AttributeType.HP, _enemyDataSO.health);
        abilitySystem.Attributes.AddAttribute(AttributeType.ATK, _enemyDataSO.atk);
        abilitySystem.Attributes.AddAttribute(AttributeType.SPD, _enemyDataSO.moveSpeed);
        abilitySystem.Attributes.AddAttribute(AttributeType.MAXRES, _enemyDataSO.staggerResistance);
        abilitySystem.Attributes.AddAttribute(AttributeType.GOLD, _enemyDataSO.item);
        abilitySystem.Attributes.AddAttribute(AttributeType.RES, _enemyDataSO.staggerRecoveryTime);

        // 1. 힐 과잉방지
        abilitySystem.Attributes.AddPostModify(AttributeType.HP, () =>
        {
            float hp = abilitySystem.Attributes.GetValue(AttributeType.HP);
            float max = abilitySystem.Attributes.GetValue(AttributeType.MaxHP);
            if (hp > max)
                abilitySystem.Attributes.Set(AttributeType.HP, max);
        });
        
        // 2. 사망 처리
        abilitySystem.Attributes.AddPostModify(AttributeType.HP, () =>
        {
            if (abilitySystem.Attributes.GetValue(AttributeType.HP) <= 0)
                _enemy.OnDeath();
            
        });
        
        // 1. resistance 회복 과잉방지
        abilitySystem.Attributes.AddPostModify(AttributeType.RES, () =>
        {
            float res = abilitySystem.Attributes.GetValue(AttributeType.RES);
            float max = abilitySystem.Attributes.GetValue(AttributeType.MAXRES);
            if (res > max)
                abilitySystem.Attributes.Set(AttributeType.RES, max);
        });
        
        // 2. stagger 처리
        abilitySystem.Attributes.AddPostModify(AttributeType.RES, () =>
        {
            if (abilitySystem.Attributes.GetValue(AttributeType.RES) <= 0)
                _enemy.OnStagger();
        });
    }
}
