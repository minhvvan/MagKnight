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
    [SerializeField] private AbilitySystem _shooterAbilitySystem; // 발사하는 주체의 ability system
    [SerializeField] private float _projectileSpeed;
    [SerializeField] private TrajectoryType _trajectoryType;
    
    private Transform targetTransform;

    private float lifeTime = 5f;
    private CancellationTokenSource _cancellation;

    private HitDetector _hitDetector;


    public void Initialize(Transform targetTransform)
    {
        this.targetTransform = targetTransform;
        
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
                GameplayEffect damageEffectToPlayer = new GameplayEffect(EffectType.Static, AttributeType.HP, 10);
                colliderObject.GetComponent<CharacterBlackBoardPro>().GetAbilitySystem().ApplyEffect(damageEffectToPlayer);
                Destroy(gameObject);
                break;
            case "Enemy":
                float damage = - _shooterAbilitySystem.GetValue(AttributeType.ATK);
                GameplayEffect damageEffectToEnemy = new GameplayEffect(EffectType.Static, AttributeType.HP, damage);
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
        float elapsedTime = 0;
        Vector3 targetPos = targetTransform.position;
        Vector3 startPos = transform.position;
        Vector3 dir = Vector3.Normalize(targetPos - startPos);
        while (elapsedTime < lifeTime)
        {
            transform.position += dir * (Time.deltaTime * _projectileSpeed);
            
            await UniTask.Yield(cancellationToken:_cancellation.Token);
            elapsedTime += Time.deltaTime;
        }
        Destroy(gameObject);
    }
}
