
using System;
using UnityEngine;

public class BaseWeapon: MonoBehaviour
{
    protected HitDetector HitDetector;

    private void Awake()
    {
        HitDetector = GetComponent<HitDetector>();
        HitDetector.OnHit += OnHit;
    }

    protected virtual void OnHit(HitInfo hit)
    {
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
}
