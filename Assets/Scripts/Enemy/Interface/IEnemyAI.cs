using System;
using UnityEngine;

[Serializable]
public abstract class EnemyAI : ScriptableObject
{
    public abstract void ExecuteBehavior();
}
