using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeleeNormalAI : IEnemyAI
{
    private Enemy _enemy;
    private EnemyBlackboard _blackboard;

    public MeleeNormalAI(Enemy enemy)
    {
        _enemy = enemy;
        _blackboard = _enemy.blackboard;
    }
    
    
    public void OnEnter(Enemy enemy)
    {
        throw new System.NotImplementedException();
    }

    public void OnUpdate(Enemy enemy)
    {
        throw new System.NotImplementedException();
    }

    public void OnExit(Enemy enemy)
    {
        throw new System.NotImplementedException();
    }
}
