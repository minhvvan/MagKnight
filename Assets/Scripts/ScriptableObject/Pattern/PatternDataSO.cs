using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Serialization;

public class PatternContext
{
    public Transform executorTransform;
    public Transform targetTransform;

    public PatternContext(Transform executorTransform, Transform targetTransform)
    {
        this.executorTransform = executorTransform;
        this.targetTransform = targetTransform;
    }
}

public abstract class PatternDataSO : ScriptableObject
{
    public string patternName;
    public float damageMultiplier;
    public float range;
    public float priority;
    public float cooldown;
    public int phase;

    public abstract bool CanUse(Transform executorTransform, Transform targetTransform);
    public abstract void Execute(Animator animator);
}
