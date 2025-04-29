using System;
using System.Collections;
using System.Collections.Generic;
using AYellowpaper.SerializedCollections;
using UnityEngine;

[RequireComponent(typeof(HitDetector))]
public class Skill : MonoBehaviour, IObserver<HitInfo>
{
    //[SerializeField] float damageInterval = 0.5f;
    [SerializeField] protected GameplayEffect _damageEffect;
    [SerializeField] protected GameplayEffect _resEffect;
    //[SerializeField] private SerializedDictionary<Collider, float> damageTimers;

    private HitDetector _hitDetector;

    protected void Start()
    {
        _hitDetector = GetComponent<HitDetector>();
        _hitDetector.Subscribe(this);
        _damageEffect.sourceTransform = transform;
        _resEffect.sourceTransform = transform;
    }


    public virtual void OnNext(HitInfo hitInfo)
    {
        Debug.Log("OnNext");
        if (hitInfo.hit.collider.gameObject.layer == LayerMask.NameToLayer("Enemy"))
        {
            Debug.Log("OnHit");
            var enemyASC = hitInfo.hit.collider.gameObject.GetComponent<Enemy>().blackboard.abilitySystem;
            enemyASC.ApplyEffect(_damageEffect);
            enemyASC.ApplyEffect(_resEffect);
        }
    }

    public void OnError(Exception error)
    {
        Debug.LogError(error);
    }

    public void OnCompleted()
    {
    }
}
