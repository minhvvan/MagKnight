using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    [SerializeField] private EnemyDataSO _enemyDataSO;
    public EnemyBlackboard blackboard;

    public void Initialize()
    {
        blackboard.Initialize(_enemyDataSO);
    }
}
