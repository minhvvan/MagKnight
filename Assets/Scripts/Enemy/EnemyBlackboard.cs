using System.Collections.Generic;
using UnityEngine;

public class EnemyBlackboard
{
    #region Data
    public string name;
    public string description;
    public float maxHealth;
    public float atk;
    public EnemyAttckType attackType;
    public float moveSpeed;
    public float weight;
    public float startupTime;
    public float recoveryTime;
    public float staggerResistance;
    public float staggerDuration;
    public List<EnemyAbility> abilities;
    public float item; // todo: gold및 아이템이 생기면 바꾸기
    public int appearanceFloor; // todo: 몹을 미리 배치하기 때문에 제거 가능성 높음
    public float projectileSpeed;
    public float attackRange;
    #endregion
    
    #region CurrentState
    public float currentHealth;
    public GameObject target;
    #endregion
    
    public void Initialize(EnemyDataSO enemyDataSO)
    {
        name = enemyDataSO.enemyName;
        description = enemyDataSO.description;
        maxHealth = enemyDataSO.health;
        atk = enemyDataSO.atk;
        attackType = enemyDataSO.attckType;
        moveSpeed = enemyDataSO.moveSpeed;
        weight = enemyDataSO.weight;
        startupTime = enemyDataSO.startupTime;
        recoveryTime = enemyDataSO.recoveryTime;
        staggerResistance = enemyDataSO.staggerResistance;
        staggerDuration = enemyDataSO.staggerDuration;
        abilities = enemyDataSO.abilities;
        item = enemyDataSO.item;
        appearanceFloor = enemyDataSO.appearanceFloor;
        projectileSpeed = enemyDataSO.projectileSpeed;
        attackRange = enemyDataSO.attackRange;
    }
}
