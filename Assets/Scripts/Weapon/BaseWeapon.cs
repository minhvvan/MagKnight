
using System;
using UnityEngine;

public class BaseWeapon: MonoBehaviour, IObserver<HitInfo>
{
    protected HitDetector HitDetector;

    public Action<Enemy> OnEnemyHit;

    private void Awake()
    {
        HitDetector = GetComponent<HitDetector>();
        HitDetector.Subscribe(this);
    }

    public virtual void AttackStart()
    {
        HitDetector.StartDetection();
    }

    public virtual void AttackEnd()
    {
        HitDetector.StopDetection();
    }

    public virtual void ChangePolarity()
    {
    }

    public virtual void OnNext(HitInfo hitInfo)
    {
        
    }

    public virtual void OnError(Exception error)
    {
        Debug.LogError(error);
    }

    public virtual void OnCompleted()
    {
    }
}
