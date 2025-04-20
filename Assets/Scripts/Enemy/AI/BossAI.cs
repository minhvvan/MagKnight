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
    public void OnEnter()
    {
        throw new System.NotImplementedException();
    }

    public void OnUpdate()
    {
        throw new System.NotImplementedException();
    }

    public void OnExit()
    {
        throw new System.NotImplementedException();
    }
}
