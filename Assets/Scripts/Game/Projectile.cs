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
    private Transform _targetTransform;
    private HitDetector _hitDetector;

    private bool _hasTarget;

    private CancellationTokenSource _cancellation;

    public void Initialize(AbilitySystem shooterAbilitySystem, Transform targetTransform, float speed)
    {
        _shooterAbilitySystem = shooterAbilitySystem;
        _targetTransform = targetTransform;
        _projectileSpeed = speed == 0 ? _projectileSpeed : speed; // runtime중 스피드 조절

        _hasTarget = (targetTransform != null); 
        
        _cancellation = new CancellationTokenSource();
        
        _hitDetector = GetComponent<HitDetector>();
        _hitDetector.Subscribe(this);
    }

    public void OnDestroy()
    {
        _cancellation.Cancel();
        _cancellation.Dispose();
    }

    public void OnNext(HitInfo hitInfo)
    {
        GameObject colliderObject = hitInfo.collider.gameObject;

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


    public void Update()
    {
        switch (_trajectoryType)
        {
            case TrajectoryType.Straight:
                MoveStraight().Forget();
                break;
            case TrajectoryType.Parabolic:
                CalculateParabolicVelocity(transform.position, _targetTransform.position, 1f, 1f, out Vector3 direction,
                    true);
                MoveParabolic().Forget();
                break;
            case TrajectoryType.Homing:
                // MoveHoming().Forget();
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
            Vector3 targetPos = _targetTransform.position;
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

    private async UniTask MoveParabolic()
    {
        Vector3 _velocity = Vector3.zero;
        float _gravity = -1f;
        float _elapsed = 0;

        
        _velocity += Vector3.up * _gravity * Time.deltaTime;
        transform.position += _velocity * Time.deltaTime;

        // 화살이 이동 방향을 바라보도록 회전
        if (_velocity.sqrMagnitude > 0.01f)
            transform.rotation = Quaternion.LookRotation(_velocity);

        // 수명 종료
        _elapsed += Time.deltaTime;
        if (_elapsed > _lifeTime)
            Destroy(gameObject);
    }

    private async UniTask MoveHoming()
    {
    // if (_hasTarget)
    // {
    //     float elapsedTime = 0;
    //     Vector3 targetPos = _targetCollider.bounds.center;
    //     Vector
    //     Vector3 dir = 
    // }
    // Vector3 dir = _target.position - _bullet.transform.position;
    // //float distThisFrame = speed * Time.deltaTime;
    // Vector3 newDirection = Vector3.RotateTowards(_bullet.transform.forward, dir, 
    //     Time.deltaTime * _bullet.bulletData.turnSpeed, 0.0f);
    // Debug.DrawRay(_bullet.transform.position, newDirection, Color.red);
    //
    // //transform.Translate(dir.normalized * distThisFrame, Space.World);
    // //transform.LookAt(target);
    //
    // _bullet.transform.Translate(Vector3.forward * Time.deltaTime * 20f);
    // _bullet.transform.rotation = Quaternion.LookRotation(newDirection);
    }

    private bool CalculateParabolicVelocity(Vector3 start, Vector3 end, float speed, float gravity, out Vector3 launchVelocity, bool preferHighArc)
    {
        Vector3 delta = end - start;
        Vector3 deltaXZ = new Vector3(delta.x, 0f, delta.z);
        float distance = deltaXZ.magnitude;
        float heightDifference = delta.y;

        float speedSquared = speed * speed;
        float g = Mathf.Abs(gravity);

        float discriminant = speedSquared * speedSquared - g * (g * distance * distance + 2 * heightDifference * speedSquared);

        if (discriminant < 0)
        {
            // 도달 불가능한 속도
            launchVelocity = Vector3.zero;
            return false;
        }

        float sqrt = Mathf.Sqrt(discriminant);
        float lowAngle = Mathf.Atan((speedSquared - sqrt) / (g * distance));
        float highAngle = Mathf.Atan((speedSquared + sqrt) / (g * distance));
        float angle = preferHighArc ? highAngle : lowAngle;

        Vector3 dir = deltaXZ.normalized;
        launchVelocity = Quaternion.AngleAxis(Mathf.Rad2Deg * angle, Vector3.Cross(dir, Vector3.up)) * dir * speed;
        return true;
    }
}
