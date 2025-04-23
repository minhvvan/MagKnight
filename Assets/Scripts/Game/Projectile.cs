using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;

public enum TrajectoryType
{
    None,
    Straight,
    Homing,
    Parabolic,
    Max
}

[RequireComponent(typeof(HitDetector))]
public class Projectile : MonoBehaviour, IObserver<HitInfo>
{
    [SerializeField] private float _projectileSpeed;
    [SerializeField] private TrajectoryType _trajectoryType;
    [SerializeField] private float _lifeTime;
    
    private AbilitySystem _shooterAbilitySystem; // 발사하는 주체의 ability system
    private Collider _targetCollider;
    private HitDetector _hitDetector;

    private bool _hasTarget;

    private CancellationTokenSource _cancellation;

    public void Initialize(AbilitySystem shooterAbilitySystem, Collider targetCollider, float speed)
    {
        _shooterAbilitySystem = shooterAbilitySystem;
        _targetCollider = targetCollider;
        _projectileSpeed = speed == 0 ? _projectileSpeed : speed; // runtime중 스피드 조절

        _hasTarget = (targetCollider != null); // target 여부에 따라 trajectory 계산이 달라짐
        
        _cancellation = new CancellationTokenSource();
        
        _hitDetector = GetComponent<HitDetector>();
        _hitDetector.Subscribe(this);

        switch (_trajectoryType)
        {
            case TrajectoryType.Straight:
                MoveStraight().Forget();
                break;
        }
    }

    public void OnDestroy()
    {
        _cancellation.Cancel();
        _cancellation.Dispose();
    }

    public void OnNext(HitInfo hitInfo)
    {
        GameObject colliderObject = hitInfo.hit.collider.gameObject;

        // 히트한 object의 layer에 따라 다르게 처리
        switch (LayerMask.LayerToName(colliderObject.layer))
        {
            case "Player":
                float damage = - _shooterAbilitySystem.GetValue(AttributeType.Strength);
                GameplayEffect damageEffectToPlayer = new GameplayEffect(EffectType.Instant, AttributeType.HP, damage);
                colliderObject.GetComponent<AbilitySystem>().ApplyEffect(damageEffectToPlayer);
                Destroy(gameObject);
                break;
            case "Enemy":
                GameplayEffect damageEffectToEnemy = new GameplayEffect(EffectType.Instant, AttributeType.HP, -10);
                colliderObject.GetComponent<EnemyBlackboard>().abilitySystem.ApplyEffect(damageEffectToEnemy);
                Destroy(gameObject);
                break;
            case "Environment":
                Destroy(gameObject);
                break;
        }
    }

    public void OnError(Exception error)
    {
        Debug.LogError(error);
    }

    public void OnCompleted()
    {
    }
    
    
    private async UniTask MoveStraight()
    {
        if (_hasTarget)
        {
            float elapsedTime = 0;
            Vector3 targetPos = _targetCollider.bounds.center;
            Vector3 startPos = transform.position;
            Vector3 dir = Vector3.Normalize(targetPos - startPos);
            while (elapsedTime < _lifeTime)
            {
                transform.position += dir * (Time.deltaTime * _projectileSpeed);
            
                await UniTask.Yield(cancellationToken:_cancellation.Token);
                elapsedTime += Time.deltaTime;
            }
            Destroy(gameObject);
        }
    }
}
