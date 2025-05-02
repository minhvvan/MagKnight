
using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Moon;
using UnityEngine;
using UnityEngine.Serialization;

public class HitInfo
{
    public RaycastHit hit;
    public Collider collider;
    public Vector3 previousPoint;
    public Vector3 currentPoint;
    public float time;

    public HitInfo(RaycastHit h, Vector3 prev, Vector3 current)
    {
        hit = h;
        collider = h.collider;
        previousPoint = prev;
        currentPoint = current;
        time = Time.fixedTime;
    }

    public HitInfo(Collider c, Vector3 prev, Vector3 current)
    {
        collider = c;
        previousPoint = prev;
        currentPoint = current;
        time = Time.fixedTime;
    }
}

[Serializable]
public class Hitbox
{
    // 단일 Hitbox
    public Transform referenceTransform; // 기준점
    public Vector3 offset;
    public float radius;
}

[Serializable]
public class HitboxesGroup
{
    // 함께 활성화되는 Hitbox들을 묶어놓음
    [FormerlySerializedAs("groupId")] public int groupId;
    public string groupName;
    [FormerlySerializedAs("isAlwaysActive")] public bool isAlwaysActive; // 공격 type이 projectile인지
    [FormerlySerializedAs("isContinuous")] public bool isContinuous; // 공격이 틱데미지 형태인지
    [FormerlySerializedAs("isSingle")] public bool isSingle; // 단일 공격인지
    [HideInInspector] public bool isActive;
    [FormerlySerializedAs("hitboxes")] public Hitbox[] hitboxes;
    public float hitInterval;
}

public class HitDetector: MonoBehaviour, IObservable<HitInfo>
{
    [SerializeField] private HitboxesGroup[] _hitboxesGroups;
    [SerializeField] private LayerMask _layerMask;
    
    private Dictionary<int, Vector3[]> _previousPointsGroups = new Dictionary<int, Vector3[]>();
    private Dictionary<int, Dictionary<Collider, float>> _hitCollidersGroups = new Dictionary<int, Dictionary<Collider, float>>(); // 다음으로 히트될 수 있는 시간을 저장
    private RaycastHit[] _hitResults = new RaycastHit[10];
    private Collider[] _colResults = new Collider[10];
    private List<HitInfo> _debugHits = new List<HitInfo>();
    private List<IObserver<HitInfo>> _observers = new List<IObserver<HitInfo>>();

    private Color[] gizmoColors = {Color.red, Color.green, Color.blue, Color.yellow, Color.magenta, Color.white, Color.cyan};
    
    void Awake()
    {
        // 상시 활성화되어있는 hitbox는 StartDetection과 상관없이 작동
        foreach (var group in _hitboxesGroups)
        {
            _previousPointsGroups[group.groupId] = new Vector3[group.hitboxes.Length];
            for (int i = 0; i < group.hitboxes.Length; i++)
            {
                Hitbox hitbox = group.hitboxes[i];
                _previousPointsGroups[group.groupId][i] = hitbox.referenceTransform.TransformPoint(hitbox.offset);
            }
            
            if (group.isAlwaysActive)
            {
                group.isActive = true;
                _hitCollidersGroups[group.groupId] = new Dictionary<Collider, float>();
            }

        }
    }
    
    public void StartDetection(int groupId = default)
    {
        var group = _hitboxesGroups.FirstOrDefault(g => g.groupId == groupId);
        if (group == null)
        {
            Debug.LogError($"Pattern {groupId} not found");
            return;
        }
        
        group.isActive = true;
        _hitCollidersGroups[group.groupId] = new Dictionary<Collider, float>();

        for (int i = 0; i < group.hitboxes.Length; i++)
        {
            Hitbox hitbox = group.hitboxes[i];
            _previousPointsGroups[group.groupId][i] = hitbox.referenceTransform.TransformPoint(hitbox.offset);
        }
    }

    public void StopDetection(int groupId = default)
    {
        var group = _hitboxesGroups.FirstOrDefault(g => g.groupId == groupId);
        if (group != null) group.isActive = false;
        _hitCollidersGroups.Remove(groupId);
    }

    private void FixedUpdate()
    {
        foreach (var group in _hitboxesGroups)
        {
            if (!group.isActive) continue;
            int groupId = group.groupId;
            
            for (int i = 0; i < group.hitboxes.Length; i++)
            {
                if (!group.isActive) continue;
                
                Hitbox hitbox = group.hitboxes[i];
                Vector3 currentPos = hitbox.referenceTransform.TransformPoint(hitbox.offset);
                Vector3 previousPos = _previousPointsGroups[groupId][i];
                Vector3 direction = currentPos - previousPos;
                float distance = direction.magnitude;
                
                if (distance > 0)
                {
                    direction.Normalize();
                    if (group.isSingle) // 단일 공격
                    {
                        int hitCount = Physics.SphereCastNonAlloc(previousPos, hitbox.radius, direction, _hitResults, distance, _layerMask);
                        if (hitCount > 0)
                        {
                            if (_hitResults[0].point == Vector3.zero)
                            {
                                _hitResults[0].point = _hitResults[0].collider.ClosestPoint(previousPos);
                            }
                            HandleHit(_hitResults[0], previousPos, currentPos); // 가장 먼저 충돌한 collider에 대해서만 처리
                            group.isActive = false; // 같은 그룹의 나머지 히트박스들도 전부 비활성화
                            _hitCollidersGroups.Remove(groupId);
                        }
                    }
                    else
                    {
                        int hitCount = Physics.SphereCastNonAlloc(previousPos, hitbox.radius, direction, _hitResults, distance, _layerMask);
                        for (int k = 0; k < hitCount; k++)
                        {
                            RaycastHit hit = _hitResults[k];
                            if (hit.point == Vector3.zero)
                            {
                                hit.point = hit.collider.ClosestPoint(previousPos);
                            }

                            if (!_hitCollidersGroups[groupId].ContainsKey(hit.collider) || _hitCollidersGroups[groupId][hit.collider] < Time.time)
                            {
                                if (group.isContinuous)
                                {
                                    _hitCollidersGroups[group.groupId][hit.collider] = Time.time + group.hitInterval;
                                }
                                else
                                {
                                    _hitCollidersGroups[group.groupId][hit.collider] = float.PositiveInfinity;
                                }
                                HandleHit(hit, previousPos, currentPos);
                            }
                        }
                    }
                    _previousPointsGroups[group.groupId][i] = currentPos;
                }
                
                else // 히트박스가 정지해 있는 경우
                {
                    if (group.isSingle) // 단일 공격
                    {
                        int hitCount = Physics.OverlapSphereNonAlloc(currentPos, hitbox.radius, _colResults, _layerMask);
                        if (hitCount > 0)
                        {
                            HandleHit(_colResults[0], previousPos, currentPos); // 가장 먼저 충돌한 collider에 대해서만 처리
                            group.isActive = false; // 같은 그룹의 나머지 히트박스들도 전부 비활성화
                            _hitCollidersGroups.Remove(groupId);
                        }
                    }
                    else
                    {
                        int hitCount = Physics.OverlapSphereNonAlloc(currentPos, hitbox.radius, _colResults, _layerMask);
                        for (int k = 0; k < hitCount; k++)
                        {
                            Collider hit = _colResults[k];

                            if (!_hitCollidersGroups[groupId].ContainsKey(hit.GetComponent<Collider>()) || _hitCollidersGroups[groupId][hit.GetComponent<Collider>()] < Time.time)
                            {
                                if (group.isContinuous)
                                {
                                    _hitCollidersGroups[group.groupId][hit.GetComponent<Collider>()] = Time.time + group.hitInterval;
                                }
                                else
                                {
                                    _hitCollidersGroups[group.groupId][hit.GetComponent<Collider>()] = float.PositiveInfinity;
                                }
                                HandleHit(hit, previousPos, currentPos);
                            }
                        }
                    }
                }
            }
        }
    }

    private void HandleHit(RaycastHit hit, Vector3 prev, Vector3 current)
    {
        HitInfo hitInfo = new HitInfo(hit, prev, current);
        //_debugHits.Add(hitInfo);
        
        Notify(hitInfo);
        
        //Debug.Shaker
        HitEffect();
        
        
        //*Temp Debug
        // hit.collider.GetComponentsInChildren<MeshRenderer>().ForEach(mr => mr.material.color = Color.red);
    }

    private void HandleHit(Collider col, Vector3 prev, Vector3 current)
    {
        HitInfo hitInfo = new HitInfo(col, prev, current);
        //_debugHits.Add(hitInfo);
        
        Notify(hitInfo);
        
        //Debug.Shaker
        HitEffect();
        
        //*Temp Debug
        // hit.collider.GetComponentsInChildren<MeshRenderer>().ForEach(mr => mr.material.color = Color.red);
    }

    void HitEffect(){
        //CameraShake.Shake(0.05f, 0.2f);
        CinemachineImpulseController.GenerateImpulse();

        //Critical Hit Effect
        Time.timeScale = 0.1f;
        UniTask.Delay(TimeSpan.FromMilliseconds(100f), DelayType.UnscaledDeltaTime).ContinueWith(() =>
        {
            Time.timeScale = 1;
        });
    }

    #if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (_hitboxesGroups == null || _hitboxesGroups.Length == 0) return;

        
        foreach (var group in _hitboxesGroups)
        {
            Gizmos.color = gizmoColors[group.groupId];
            foreach (var hitbox in group.hitboxes)
            {
                Vector3 worldPoint = hitbox.referenceTransform.TransformPoint(hitbox.offset);
                Gizmos.DrawSphere(worldPoint, hitbox.radius);
            }
        }
        
        // // 저장된 히트 정보 시각화
        // foreach (var hitInfo in _debugHits)
        // {
        //     // 히트 포인트 (빨간색)
        //     Gizmos.color = Color.red;
        //     Gizmos.DrawSphere(hitInfo.hit.point, 0.1f);
        //
        //     // 이전 위치 (노란색)
        //     Gizmos.color = Color.yellow;
        //     Gizmos.DrawSphere(hitInfo.previousPoint, 0.08f);
        //
        //     // 현재 위치 (녹색)
        //     Gizmos.color = Color.green;
        //     Gizmos.DrawSphere(hitInfo.currentPoint, 0.08f);
        //
        //     // 선으로 연결
        //     Gizmos.color = Color.white;
        //     Gizmos.DrawLine(hitInfo.previousPoint, hitInfo.hit.point);
        //     Gizmos.DrawLine(hitInfo.hit.point, hitInfo.currentPoint);
        //
        //     UnityEditor.Handles.Label(hitInfo.hit.point + Vector3.up * 0.2f, $"Hit at {hitInfo.time:F1}s");
        // }
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
