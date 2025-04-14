using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IEnemyAI
{
    void OnEnter(Enemy enemy);
    void OnUpdate(Enemy enemy);
    void OnExit(Enemy enemy);
}
