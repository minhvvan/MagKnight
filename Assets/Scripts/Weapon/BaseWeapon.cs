
using System;
using UnityEngine;

public class BaseWeapon: MonoBehaviour, IObserver<HitInfo>
{
    protected HitDetector HitDetector;
    public Action<HitInfo> OnHit;
    
    private void Awake()
    {
        HitDetector = GetComponent<HitDetector>();
        HitDetector.Subscribe(this);
    }

    public virtual void AttackStart(int hitboxGroupId)
    {
        HitDetector.StartDetection(hitboxGroupId);
    }

    public virtual void AttackEnd(int hitboxGroupId)
    {
        HitDetector.StopDetection(hitboxGroupId);
    }

    public virtual int OnSkill()
    {
        return 0;
    }

    public virtual void ChangePolarity()
    {
    }

    public virtual void OnNext(HitInfo hitInfo)
    {
        OnHit?.Invoke(hitInfo);
    }

    public virtual void OnError(Exception error)
    {
        Debug.LogError(error);
    }

    public virtual void OnCompleted()
    {
    }
}
