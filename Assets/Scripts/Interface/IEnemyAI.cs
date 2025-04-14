using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IEnemyAI
{
    // EnemyStateAI에서 작동하는 로직을 전략패턴으로 구현
    void OnEnter();
    void OnUpdate();
    void OnExit();
}
