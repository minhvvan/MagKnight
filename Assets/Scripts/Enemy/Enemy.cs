using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    [SerializeField] private EnemyDataSO _enemyDataSO;
    public EnemyBlackboard blackboard;

    // stateMachine
    private StateMachine _stateMachine;
    
    [NonSerialized] public EnemyStateSpawn spawnState;
    [NonSerialized] public EnemyStateAI aiState;
    [NonSerialized] public EnemyStateAction actionState;
    [NonSerialized] public EnemyStateHit hitState;
    [NonSerialized] public EnemyStateDead deadState;
    
    
    public void Initialize()
    {
        blackboard.Initialize(_enemyDataSO);
        InitializeState();
    }

    private void InitializeState()
    {
        _stateMachine = new StateMachine();

        spawnState = new EnemyStateSpawn(this);
        aiState = new EnemyStateAI(this);
        actionState = new EnemyStateAction(this);
        hitState = new EnemyStateHit(this);
        deadState = new EnemyStateDead(this);

        _stateMachine.ChangeState(spawnState);
    }
}
