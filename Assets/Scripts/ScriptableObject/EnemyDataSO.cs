using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "EnemyData", menuName = "SO/Enemy/EnemyData")]
public class EnemyDataSO : ScriptableObject
{
    public string enemyName;
    public string description;
    public float health;
    public float atk;
    public EnemyAttckType attckType;
    public EnemyAIType aiType;
    public float moveSpeed;
    public float startupTime;
    public float recoveryTime;
    public float staggerResistance;
    public float staggerRecoveryTime;
    public List<EnemyAbilityType> abilities;
    public float item; // todo: gold및 아이템이 생기면 바꾸기
    public int appearanceFloor; // todo: 몹을 미리 배치하기 때문에 제거 가능성 높음
    public float projectileSpeed;
    public float attackRange;
    public GameObject projectilePrefab;
}