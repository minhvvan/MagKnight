using System;
using System.Collections.Generic;
using System.Threading;
using Unity.VisualScripting;
using UnityEngine;

public class EnemyBlackboard : MonoBehaviour
{
    [SerializeField] private EnemyDataSO _enemyDataSO;
    [SerializeField] private EnemyStatSO _enemyStatSO;
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
    [HideInInspector] public float attackRange;
    [HideInInspector] public GameObject projectilePrefab;
    #endregion
    
    #region CurrentState
    [HideInInspector] public GameObject target;
    [HideInInspector] public LayerMask targetLayer;
    [HideInInspector] public IEnemyAI ai;
    [HideInInspector] public IEnemyAction action;
    [HideInInspector] public bool isDead;
    [HideInInspector] public int phase;
    #endregion
    
    #region CancellationToken

    [HideInInspector] public CancellationTokenSource onHitCancellation;
    [HideInInspector] public CancellationTokenSource actionRecoveryCancellation;
    [HideInInspector] public CancellationTokenSource staggerRecoveryCancellation;
    #endregion
    
    public Transform muzzleTransform;
    public Renderer enemyRenderer;
    [HideInInspector] public Texture baseColorTexture;
    [HideInInspector] public Texture2D onHitTexture;
    [HideInInspector] public Texture2D phase2Texture;
    [HideInInspector] public Texture2D phase3Texture;
    public Transform headTransform;
    public Transform leftHandTransform;


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
        attackRange = _enemyDataSO.attackRange;
        projectilePrefab = _enemyDataSO.projectilePrefab;
        phase = 1;
        
        targetLayer = LayerMask.GetMask("Player");

        baseColorTexture = enemyRenderer.material.GetTexture("_BaseColor");
        onHitTexture = new Texture2D(1, 1);
        onHitTexture.SetPixel(1, 1, Color.white);
        onHitTexture.Apply();
        phase2Texture = new Texture2D(1, 1);
        phase2Texture.SetPixel(1, 1, Color.red);
        phase2Texture.Apply();
        
        switch (aiType)
        {
            case EnemyAIType.MeleeNormal:
                ai = new MeleeNormalAI(_enemy);
                action = new MeleeNormalAction(_enemy);
                break;
            case EnemyAIType.RangedNormal:
                ai = new RangedNormalAI(_enemy);
                action = new RangedNormalAction(_enemy);
                break;
            case EnemyAIType.Boss:
                ai = new BossAI(_enemy);
                action = new BossAction(_enemy);
                break;
        }

        if (headTransform == null)
        {
            headTransform = transform.Find("HeadTarget");
            if (headTransform == null)
            {
                headTransform = transform;
            }
        }
    }

    private void InitializeAttributes()
    {
        abilitySystem.InitializeFromEnemyStat(_enemyStatSO.Stat);

        abilitySystem.GetAttributeSet<EnemyAttributeSet>().OnDeath += _enemy.OnDeath;
        abilitySystem.GetAttributeSet<EnemyAttributeSet>().OnStagger += _enemy.OnStagger;
        abilitySystem.GetAttributeSet<EnemyAttributeSet>().OnPhaseChange += _enemy.OnPhaseChange;
        abilitySystem.GetAttributeSet<EnemyAttributeSet>().OnHit += _enemy.OnHit;
        abilitySystem.GetAttributeSet<EnemyAttributeSet>().OnDamage += _enemy.OnDamaged;
    }
}
