using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 1. 매 프레임 시전 조건을 만족하는 패턴이 있는지 체크
/// 2. 조건을 만족하는 패턴이 있을 경우 해당 Action을 시전
/// 3. 조건 만족 패턴이 여러개일 경우, 우선순위가 높은 패턴을 시전
/// 4. 패턴 시전 후에는 1번부터 반복
/// </summary>
public class BossAI : IEnemyAI
{
    private Enemy _enemy;
    private EnemyBlackboard _blackboard;
    private PatternController _patternController;
    
    private float destinationUpdateInterval = 0.1f;
    private float _destinationTimer = 0f;

    public BossAI(Enemy enemy)
    {
        _enemy = enemy;
        _blackboard = _enemy.blackboard;
        _patternController = _enemy.GetComponent<PatternController>();
    }
    
    public void OnEnter()
    {
        _enemy.Anim.SetBool("Trace", true);
        _enemy.Agent.SetDestination(_blackboard.target.transform.position);
    }

    public void OnUpdate()
    {
        PatternDataSO patternDataSO = _patternController.GetAvailablePattern(_enemy.transform, _blackboard.target.transform);
        if(patternDataSO != null)
        {
            _enemy.SetState(_enemy.actionState);
        }
        else
        {
            _destinationTimer += Time.deltaTime;
            if (_destinationTimer >= destinationUpdateInterval)
            {
                _enemy.Agent.SetDestination(_blackboard.target.transform.position);
                _destinationTimer = 0f;
            }
        }
    }

    public void OnExit()
    {
        _enemy.Anim.SetBool("Trace", false);
    }
}
