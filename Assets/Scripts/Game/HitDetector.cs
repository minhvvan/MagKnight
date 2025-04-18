
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using VFolders.Libs;

public enum HitType
{
    None,
    Melee,
    Projectile,
    Max
}

public class HitInfo
{
    public RaycastHit hit;
    public Vector3 previousPoint;
    public Vector3 currentPoint;
    public float time;

    public HitInfo(RaycastHit h, Vector3 prev, Vector3 current)
    {
        hit = h;
        previousPoint = prev;
        currentPoint = current;
        time = Time.fixedTime;
    }
}

public class HitDetector: MonoBehaviour, IObservable<HitInfo>
{
    [Serializable]
    public class HitboxZone
    {
        public Transform referenceTransform; // 기준점
        public Vector3 offset;
        public float radius;
    }
    
    [SerializeField] private HitboxZone[] _hitboxZones;
    [SerializeField] private LayerMask _layerMask;
    [SerializeField] private HitType _hitType;
    
    private bool _isDetecting = false;
    private List<Vector3> _previousPoints = new List<Vector3>();
    private HashSet<Collider> _hitColliders = new HashSet<Collider>();
    private RaycastHit[] _hitResults = new RaycastHit[10];
    private List<HitInfo> _debugHits = new List<HitInfo>();
    private List<IObserver<HitInfo>> _observers = new List<IObserver<HitInfo>>();
    
    public void StartDetection()
    {
        _isDetecting = true;
        _hitColliders.Clear();
        
        _previousPoints.Clear();
        _hitboxZones.ForEach(hitboxZone => _previousPoints.Add(hitboxZone.referenceTransform.TransformPoint(hitboxZone.offset)));
    }

    public void StopDetection()
    {
        _isDetecting = false;
    }

    private void FixedUpdate()
    {
        // projectile은 항상 공격모션과 별개로 히트판정이 항상 존재하기 때문에 isDetecting에 여부와 관계없이 히트 여부를 확인한다.
        if(!_isDetecting && _hitType == HitType.Melee) return;

        for (var i = 0; i < _previousPoints.Count; i++)
        {
            Vector3 currentPos = _hitboxZones[i].referenceTransform.TransformPoint(_hitboxZones[i].offset);
            Vector3 previousPos = _previousPoints[i];
            Vector3 direction = currentPos - previousPos;
            float distance = direction.magnitude;
            
            if (distance > 0)
            {
                direction.Normalize();
            
                int hitCount = Physics.SphereCastNonAlloc(previousPos, _hitboxZones[i].radius, direction, _hitResults, distance, _layerMask);
                for (int j = 0; j < hitCount; j++)
                {
                    RaycastHit hit = _hitResults[j];

                    if(!_hitColliders.Contains(hit.collider))
                    {
                        _hitColliders.Add(hit.collider);
                        HandleHit(hit, previousPos, currentPos);
                    }
                }
            }
        }
    
        _previousPoints.Clear();
        _hitboxZones.ForEach(hitboxZone => _previousPoints.Add(hitboxZone.referenceTransform.TransformPoint(hitboxZone.offset)));
    }

    private void HandleHit(RaycastHit hit, Vector3 prev, Vector3 current)
    {
        HitInfo hitInfo = new HitInfo(hit, prev, current);
        _debugHits.Add(hitInfo);
        
        Notify(hitInfo);
        
        //*Temp Debug
        hit.collider.GetComponentsInChildren<MeshRenderer>().ForEach(mr => mr.material.color = Color.red);
    }

    #if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (_hitboxZones.Any())
        {
            Gizmos.color = Color.blue;
            foreach (var hitboxZone in _hitboxZones)
            {
                Vector3 worldPoint = hitboxZone.referenceTransform.TransformPoint(hitboxZone.offset);
                Gizmos.DrawSphere(worldPoint, hitboxZone.radius);
            }
        }

        // 저장된 히트 정보 시각화
        foreach (var hitInfo in _debugHits)
        {
            // 히트 포인트 (빨간색)
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(hitInfo.hit.point, 0.1f);
        
            // 이전 위치 (노란색)
            Gizmos.color = Color.yellow;
            Gizmos.DrawSphere(hitInfo.previousPoint, 0.08f);
        
            // 현재 위치 (녹색)
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(hitInfo.currentPoint, 0.08f);
        
            // 선으로 연결
            Gizmos.color = Color.white;
            Gizmos.DrawLine(hitInfo.previousPoint, hitInfo.hit.point);
            Gizmos.DrawLine(hitInfo.hit.point, hitInfo.currentPoint);
        
            UnityEditor.Handles.Label(hitInfo.hit.point + Vector3.up * 0.2f, $"Hit at {hitInfo.time:F1}s");
        }
    }
    #endif
    public void Subscribe(IObserver<HitInfo> observer)
    {
        if (_observers.Contains(observer)) return;
        _observers.Add(observer);
    }

    public void Unsubscribe(IObserver<HitInfo> observer)
    {
        if (!_observers.Contains(observer)) return;
        _observers.Remove(observer);
    }

    public void Notify(HitInfo hitInfo)
    {
        _observers.ForEach(observer => observer.OnNext(hitInfo));
    }
}
