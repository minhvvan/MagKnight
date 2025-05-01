using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "EnemyData", menuName = "SO/Enemy/EnemyData")]
public class EnemyDataSO : ScriptableObject
{
    public string enemyName;
    public string description;
    public EnemyAttckType attckType;
    public EnemyAIType aiType;
    public float startupTime;
    public float recoveryTime;
    public float staggerRecoveryTime;
    public float attackRange;
    public GameObject projectilePrefab;
}