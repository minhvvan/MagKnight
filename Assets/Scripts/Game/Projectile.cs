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

public class ProjectileLaunchData
{
    // 궤적을 계산할 때 사용할 값을 선택하세요
    public Vector3? InitialDirection { get; private set; } // 초기 방향
    public Vector3? TargetPosition { get; private set; } // 목표지점의 position
    public Collider TargetCollider { get; private set; } // 타겟의 collider
    public ProjectileLaunchData(Vector3 direction)
    {
        InitialDirection = direction.normalized;
    }

    public ProjectileLaunchData(Vector3 targetPosition, bool isTargetPoint)
    {
        TargetPosition = targetPosition;
    }

    public ProjectileLaunchData(Collider targetCollider)
    {
        TargetCollider = targetCollider;
        TargetPosition = targetCollider.bounds.center;
    }
}

[RequireComponent(typeof(HitDetector))]
public class Projectile : MonoBehaviour, IObserver<HitInfo>
{
    public Action<HitInfo> OnHit;
    
    [SerializeField] private float _projectileSpeed;
    [SerializeField] private TrajectoryType _trajectoryType;
    [SerializeField] private float _lifeTime;
    [SerializeField] private ParticleSystem projectileParticleSystem;
    
    // private AbilitySystem _shooterAbilitySystem; // 발사하는 주체의 ability system
    private Transform _targetTransform;
    private HitDetector _hitDetector;

    private Vector3 _velocity;
    private float _gravity = -1f;
    private float _elapsedTime = 0f;
    

    public void Initialize(ProjectileLaunchData projectileLaunchData)
    {
        _hitDetector = GetComponent<HitDetector>();
        _hitDetector.Subscribe(this);
        
        _velocity = CalculateInitialVelocity(projectileLaunchData);
    }
    
    public void FixedUpdate()
    {
        switch (_trajectoryType)
        {
            case TrajectoryType.Straight:
                MoveStraight();
                break;
            case TrajectoryType.Parabolic:
                MoveParabolic();
                break;
            case TrajectoryType.Homing:
                // MoveHoming();
                break;
        }
        
        _elapsedTime += Time.deltaTime;
        if (_elapsedTime >= _lifeTime)
        {
            Destroy(gameObject);
        }
    }
    
    public void OnDestroy()
    {
    }
    
    private void MoveStraight()
    {
        transform.position += _velocity * Time.deltaTime;
    }

    private void MoveParabolic()
    {
        _velocity += Vector3.up * (_gravity * Time.deltaTime);
        transform.position += _velocity * Time.deltaTime;

        // 화살이 이동 방향을 바라보도록 회전
        if (_velocity.sqrMagnitude > 0.01f)
            transform.rotation = Quaternion.LookRotation(_velocity);
    }

    private void MoveHoming()
    {
    }

    private Vector3 CalculateInitialVelocity(ProjectileLaunchData launchData)
    {
        // 목적지 position을 가지고 있는 경우
        if (launchData.TargetPosition.HasValue)
        {
            Vector3 targetPosition = launchData.TargetPosition.Value;
        
            switch (_trajectoryType)
            {
                case TrajectoryType.Straight:
                    return (targetPosition - transform.position).normalized * _projectileSpeed;
                case TrajectoryType.Parabolic:
                    if (CalculateParabolicVelocity(transform.position, targetPosition, out Vector3 velocity, false)) 
                        return velocity;
                    else // 현재 총알속도로는 물리적으로 목적지에 도달이 불가능한 경우
                    {
                        if(launchData.InitialDirection.HasValue) // direction이 있으면 그 방향으로 발사
                            return launchData.InitialDirection.Value.normalized * _projectileSpeed;
                        else // direction이 없으면 45도 각도로 발사
                            return ((targetPosition - transform.position).normalized + Vector3.up).normalized * _projectileSpeed;
                    }
            }
        }
        if (launchData.InitialDirection.HasValue) // 목적지 없이 direction만 있는 경우
            return launchData.InitialDirection.Value.normalized * _projectileSpeed;

        Debug.Log("No launch data found");
        return Vector3.zero;
    }

    private bool CalculateParabolicVelocity(Vector3 start, Vector3 end, out Vector3 launchVelocity, bool preferHighArc)
    {
        Vector3 delta = end - start;
        Vector3 deltaXZ = new Vector3(delta.x, 0f, delta.z);
        float distance = deltaXZ.magnitude;
        float heightDifference = delta.y;

        float speedSquared = _projectileSpeed * _projectileSpeed;
        float g = Mathf.Abs(_gravity);

        float discriminant = speedSquared * speedSquared - g * (g * distance * distance + 2 * heightDifference * speedSquared);

        if (discriminant < 0)
        {
            // 현재 화살 속도로는 목표지점까지 도달 불가능
            launchVelocity = Vector3.zero;
            return false;
        }

        float sqrt = Mathf.Sqrt(discriminant);
        float lowAngle = Mathf.Atan((speedSquared - sqrt) / (g * distance));
        float highAngle = Mathf.Atan((speedSquared + sqrt) / (g * distance));
        float angle = preferHighArc ? highAngle : lowAngle;

        Vector3 dir = deltaXZ.normalized;
        launchVelocity = Quaternion.AngleAxis(Mathf.Rad2Deg * angle, Vector3.Cross(dir, Vector3.up)) * dir * _projectileSpeed;
        return true;
    }

    public void SetProjectileVFXColor(Color color)
    {
        if (!projectileParticleSystem) return;
    
        // 파티클 시스템의 렌더러를 통해 머티리얼에 접근
        ParticleSystemRenderer particleRenderer = projectileParticleSystem.GetComponent<ParticleSystemRenderer>();
    
        if (particleRenderer && particleRenderer.material)
        {
            particleRenderer.material.SetColor("_BaseColor", color);
        }
    }
    
    public void OnNext(HitInfo hitInfo)
    {
        OnHit?.Invoke(hitInfo);
        Destroy(gameObject);
    }
    
    public void OnError(Exception error)
    {
        Debug.LogError(error);
    }

    public void OnCompleted()
    {
    }

}