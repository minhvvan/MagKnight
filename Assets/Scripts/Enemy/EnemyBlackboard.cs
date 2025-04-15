using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class EnemyBlackboard
{
    #region Data
    public string name;
    public string description;
    public float maxHealth;
    public float atk;
    public EnemyAttckType attackType;
    public EnemyAIType aiType;
    public float moveSpeed;
    public float weight;
    public float startupTime;
    public float recoveryTime;
    public float staggerResistance;
    public float staggerRecoveryTime;
    public List<EnemyAbilityType> abilities;
    public float item; // todo: gold및 아이템이 생기면 바꾸기
    public int appearanceFloor; // todo: 몹을 미리 배치하기 때문에 제거 가능성 높음
    public float projectileSpeed;
    public float attackRange;
    #endregion
    
    #region CurrentState

    public Enemy enemy;
    public float currentHealth;
    public GameObject target;
    public float currentStaggerResistance;
    public LayerMask targetLayer;
    public IEnemyAI ai;
    #endregion
    
    #region CancellationToken

    public CancellationTokenSource actionDelayCancellation;
    #endregion
    
    public void Initialize(EnemyDataSO enemyDataSO, Enemy enemy)
    {
        name = enemyDataSO.enemyName;
        description = enemyDataSO.description;
        maxHealth = enemyDataSO.health;
        atk = enemyDataSO.atk;
        attackType = enemyDataSO.attckType;
        aiType = enemyDataSO.aiType;
        moveSpeed = enemyDataSO.moveSpeed;
        weight = enemyDataSO.weight;
        startupTime = enemyDataSO.startupTime;
        recoveryTime = enemyDataSO.recoveryTime;
        staggerResistance = enemyDataSO.staggerResistance;
        staggerRecoveryTime = enemyDataSO.staggerRecoveryTime;
        abilities = enemyDataSO.abilities;
        item = enemyDataSO.item;
        appearanceFloor = enemyDataSO.appearanceFloor;
        projectileSpeed = enemyDataSO.projectileSpeed;
        attackRange = enemyDataSO.attackRange;

        this.enemy = enemy;
        currentHealth = maxHealth;
        currentStaggerResistance = staggerResistance;
        targetLayer = LayerMask.GetMask("Player");

        switch (aiType)
        {
            case EnemyAIType.MeleeNormal:
                ai = new MeleeNormalAI(enemy);
                break;
        }
        
        actionDelayCancellation = new CancellationTokenSource();
    }
}
