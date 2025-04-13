
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using VFolders.Libs;

public class HitInfo
{
    public int objectId;
    public Vector3 hitPoint;
    public Vector3 previousPoint;
    public Vector3 currentPoint;
    public float time;

    public HitInfo(int id, Vector3 hit, Vector3 prev, Vector3 current)
    {
        objectId = id;
        hitPoint = hit;
        previousPoint = prev;
        currentPoint = current;
        time = Time.time;
    }
}

public class OverlapDetector: MonoBehaviour
{
    [SerializeField] private List<Vector3> hitPoints;
    [SerializeField] LayerMask layerMask;
    [SerializeField] float detectionRadius;
    
    private bool _IsDetecting = false;
    private List<Vector3> _previousPoints = new List<Vector3>();
    private RaycastHit[] _hitResults = new RaycastHit[10];
    private List<HitInfo> _hits = new List<HitInfo>();

    private void Start()
    {
        StartDetection();
    }

    public void StartDetection()
    {
        _IsDetecting = true;
    }

    public void StopDetection()
    {
        _IsDetecting = false;
        _hits.Clear();
    }

    private void FixedUpdate()
    {
        if(!_IsDetecting) return;

        for (var i = 0; i < _previousPoints.Count; i++)
        {
            Vector3 currentPos = ConvertWorldTransform(hitPoints[i]);
            Vector3 previousPos = _previousPoints[i];
            Vector3 direction = currentPos - previousPos;
            float distance = direction.magnitude;
            
            if (distance > 0)
            {
                direction.Normalize();
            
                int hitCount = Physics.SphereCastNonAlloc(previousPos, detectionRadius, direction, _hitResults, distance, layerMask);
                
                for (int j = 0; j < hitCount; j++)
                {
                    RaycastHit hit = _hitResults[j];
                    if (hit.point != Vector3.zero)
                    {
                        HandleHit(hit, previousPos, currentPos);
                    }
                }
            }
        }
    
        _previousPoints.Clear();
        hitPoints.ForEach(point => _previousPoints.Add(ConvertWorldTransform(point)));
    }

    private void HandleHit(RaycastHit hit, Vector3 prev, Vector3 current)
    {
        if (_hits.Any(hitted => hitted.objectId == hit.colliderInstanceID)) return;
        
        _hits.Add(new HitInfo(hit.colliderInstanceID, hit.point, prev, current));
        
        //TODO: hit 처리 + 필요시 전달
        hit.collider.GetComponentsInChildren<MeshRenderer>().ForEach(mr => mr.material.color = Color.red);
    }

    private Vector3 ConvertWorldTransform(Vector3 point)
    {
        return transform.TransformPoint(point);
    }

    #if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (hitPoints is { Count: > 0 })
        {
            Gizmos.color = Color.blue;
            foreach (var point in hitPoints)
            {
                Vector3 worldPoint = ConvertWorldTransform(point);
                Gizmos.DrawSphere(worldPoint, detectionRadius);
            }
        }

        // 저장된 히트 정보 시각화
        foreach (var hit in _hits)
        {
            // 히트 포인트 (빨간색)
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(hit.hitPoint, 0.1f);
        
            // 이전 위치 (노란색)
            Gizmos.color = Color.yellow;
            Gizmos.DrawSphere(hit.previousPoint, 0.08f);
        
            // 현재 위치 (녹색)
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(hit.currentPoint, 0.08f);
        
            // 선으로 연결
            Gizmos.color = Color.white;
            Gizmos.DrawLine(hit.previousPoint, hit.hitPoint);
            Gizmos.DrawLine(hit.hitPoint, hit.currentPoint);
        
            UnityEditor.Handles.Label(hit.hitPoint + Vector3.up * 0.2f, $"Hit at {hit.time:F1}s");
        }
    }
    #endif
}
