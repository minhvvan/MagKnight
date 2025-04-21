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
    
    // attribute로 관리되지 않는 데이터
    // 변동이 없거나 다른 object와의 상호작용 요소가 없음
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
    [HideInInspector] public GameObject projectilePrefab;
    #endregion
    
    #region CurrentState
    [HideInInspector] public GameObject target;
    [HideInInspector] public LayerMask targetLayer;
    [HideInInspector] public IEnemyAI ai;
    [HideInInspector] public bool isDead;
    #endregion
    
    #region CancellationToken

    [HideInInspector] public CancellationTokenSource actionRecoveryCancellation;
    [HideInInspector] public CancellationTokenSource staggerRecoveryCancellation;
    #endregion
    
    #region Transform

    public Transform muzzleTransform;
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
        projectilePrefab = _enemyDataSO.projectilePrefab;
        
        targetLayer = LayerMask.GetMask("Player");

        switch (aiType)
        {
            case EnemyAIType.MeleeNormal:
                ai = new MeleeNormalAI(_enemy);
                break;
            case EnemyAIType.RangedNormal:
                ai = new RangedNormalAI(_enemy);
                break;
        }
    }

    private void InitializeAttributes()
    {
        // 버프, 디버프, 상호작용 가능 stat은 attribute로 관리
        abilitySystem.AddAttribute(AttributeType.MaxHP, _enemyDataSO.health);
        abilitySystem.AddAttribute(AttributeType.HP, _enemyDataSO.health);
        abilitySystem.AddAttribute(AttributeType.ATK, _enemyDataSO.atk);
        abilitySystem.AddAttribute(AttributeType.MoveSpeed, _enemyDataSO.moveSpeed);
        abilitySystem.AddAttribute(AttributeType.MAXRES, _enemyDataSO.staggerResistance);
        abilitySystem.AddAttribute(AttributeType.GOLD, _enemyDataSO.item);
        abilitySystem.AddAttribute(AttributeType.RES, _enemyDataSO.staggerResistance);

        // 1. 힐 과잉방지
        // abilitySystem.AddPostModify(AttributeType.HP, () =>
        // {
        //     float hp = abilitySystem.GetValue(AttributeType.HP);
        //     float max = abilitySystem.GetValue(AttributeType.MaxHP);
        //     if (hp > max)
        //         abilitySystem.SetValue(AttributeType.HP, max);
        // });
        
        // 2. 사망 처리
        // abilitySystem.AddPostModify(AttributeType.HP, () =>
        // {
        //     if (abilitySystem.GetValue(AttributeType.HP) <= 0)
        //     {
        //         _enemy.OnDeath();
        //     }
        // });
        
        // 1. resistance 회복 과잉방지
        // abilitySystem.AddPostModify(AttributeType.RES, () =>
        // {
        //     float res = abilitySystem.GetValue(AttributeType.RES);
        //     float max = abilitySystem.GetValue(AttributeType.MAXRES);
        //     if (res > max)
        //         abilitySystem.SetValue(AttributeType.RES, max);
        // });
        
        // 2. stagger 처리
        // abilitySystem.AddPostModify(AttributeType.RES, () =>
        // {
        //     if (!isDead && abilitySystem.GetValue(AttributeType.RES) <= 0)
        //     {
        //         _enemy.OnStagger();
        //     }
        // });
    }
}
