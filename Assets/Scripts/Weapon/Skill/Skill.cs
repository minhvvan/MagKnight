using System;
using System.Collections;
using System.Collections.Generic;
using AYellowpaper.SerializedCollections;
using hvvan;
using UnityEngine;

[RequireComponent(typeof(HitDetector))]
public class Skill : MonoBehaviour, IObserver<HitInfo>
{
    //[SerializeField] float damageInterval = 0.5f;
    protected GameplayEffect _damageEffect;
    protected GameplayEffect _resistanceEffect;
    //[SerializeField] private SerializedDictionary<Collider, float> damageTimers;

    private HitDetector _hitDetector;

    protected void Start()
    {
        _hitDetector = GetComponent<HitDetector>();
        _hitDetector.Subscribe(this);
        _damageEffect = new GameplayEffect(EffectType.Instant, AttributeType.Damage, 0)
        {
            extraData = new ExtraData(){ sourceTransform = transform }
        };
        _resistanceEffect = new GameplayEffect(EffectType.Instant, AttributeType.ResistanceDamage, 10)
        {
            extraData = new ExtraData(){ sourceTransform = transform }
        };
    }


    public virtual void OnNext(HitInfo hitInfo)
    {
        if (hitInfo.collider.gameObject.layer == LayerMask.NameToLayer("Enemy"))
        {
            var damage = GameManager.Instance.Player.AbilitySystem.GetValue(AttributeType.Strength);
            _damageEffect.amount = damage;
            var enemyASC = hitInfo.collider.gameObject.GetComponent<Enemy>().blackboard.abilitySystem;
            enemyASC.ApplyEffect(_damageEffect);
            enemyASC.ApplyEffect(_resistanceEffect);
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
