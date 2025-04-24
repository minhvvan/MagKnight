using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IEnemyAction
{
    void OnEnter();
    void OnUpdate();
    void OnExit();
}
