using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackEffect : MonoBehaviour, IObserver<HitInfo>
{
    public Action<HitInfo> OnHit;
    [SerializeField] private float _startOffset;
    [SerializeField] private float _duration;
    [SerializeField] private float _lifeTime;

    // private AbilitySystem _abilitySystem;
    private HitDetector _hitDetector;
    private float _startTime;
    private float _endTime;
    private float _destroyTime;

    private float _damage;
    private float _impulse;
    
    void Awake()
    {
        _hitDetector = gameObject.GetComponent<HitDetector>();
        _hitDetector.Subscribe(this);
        _startTime = Time.time + _startOffset;
        _endTime = _startTime + _duration;
        _destroyTime = Time.time + _lifeTime;
    }
    

    void Update()
    {
        if (Time.time > _startTime)
        {
            _startTime = float.PositiveInfinity;
            _hitDetector.StartDetection();
        }

        if (Time.time > _endTime)
        {
            _endTime = float.PositiveInfinity;
            _hitDetector.StopDetection();
        }

        if (Time.time > _destroyTime)
        {
            Destroy(gameObject);
        }
    }
    
    public void OnNext(HitInfo hitInfo)
    {
        GameplayEffect damageEffect = new GameplayEffect(EffectType.Instant, AttributeType.Damage, _damage);
        GameplayEffect impulseEffect = new GameplayEffect(EffectType.Instant, AttributeType.Impulse, _impulse);
        damageEffect.extraData.sourceTransform = transform;
        impulseEffect.extraData.sourceTransform = transform;
        hitInfo.collider.gameObject.GetComponent<AbilitySystem>().ApplyEffect(damageEffect);
        hitInfo.collider.gameObject.GetComponent<AbilitySystem>().ApplyEffect(impulseEffect);
    }

    public void SetAttackDamageImpulse(float damage, float impulse)
    {
        _damage = damage;
        _impulse = impulse;
    }

    public void OnError(Exception error)
    {
        Debug.LogError(error);
    }

    public void OnCompleted()
    {
    }
}
