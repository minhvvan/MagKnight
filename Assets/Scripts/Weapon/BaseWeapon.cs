
using System;
using UnityEngine;

public class BaseWeapon: MonoBehaviour
{
    protected OverlapDetector _overlapDetector;

    private void Awake()
    {
        _overlapDetector = GetComponent<OverlapDetector>();
        _overlapDetector.OnHit += OnHit;
    }

    protected virtual void OnHit(HitInfo hit)
    {
        
    }

    public virtual void AttackStart()
    {
        _overlapDetector.StartDetection();
    }

    public virtual void AttackEnd()
    {
        _overlapDetector.StopDetection();
    }
}
