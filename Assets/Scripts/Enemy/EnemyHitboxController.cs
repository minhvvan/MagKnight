using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class EnemyHitboxController : MonoBehaviour, IObservable<GameObject>
{
    [Serializable]
    public class HitboxZone
    {
        public Transform referenceTransform;
        public Vector3 offset;
        public float radius;
    }
    
    [SerializeField] private HitboxZone[] _triggerZones;
    
    private List<IObserver<GameObject>> _observers = new List<IObserver<GameObject>>();

    // 충돌 처리
    private Vector3[] _previousPositions;
    private HashSet<Collider> _hitColliders;
    private Ray _ray = new Ray();
    private RaycastHit[] _hits = new RaycastHit[10];
    private bool _isAttacking = false;

    private void Start()
    {
        _previousPositions = new Vector3[_triggerZones.Length];
        _hitColliders = new HashSet<Collider>();
    }

    public void AttackStart()
    {
        _isAttacking = true;
        _hitColliders.Clear();

        for (int i = 0; i < _triggerZones.Length; i++)
        {
            _previousPositions[i] = _triggerZones[i].referenceTransform.position + _triggerZones[i].offset;
        }
    }
    
    public void AttackEnd()
    {
        _isAttacking = false;
    }
    
    private void FixedUpdate()
    {
        if (_isAttacking)
        {
            for (int i = 0; i < _triggerZones.Length; i++)
            {
                var worldPosition = _triggerZones[i].referenceTransform.position + _triggerZones[i].offset;
                var direction = worldPosition - _previousPositions[i];
                _ray.origin = _previousPositions[i];
                _ray.direction = direction;
                
                var hitCount = Physics.SphereCastNonAlloc(_ray, 
                    _triggerZones[i].radius, _hits, 
                    direction.magnitude, LayerMask.GetMask("Player"),
                    QueryTriggerInteraction.UseGlobal);
                for (int j = 0; j < hitCount; j++)
                {
                    var hit = _hits[j];
                    if (!_hitColliders.Contains(hit.collider))
                    {
                        // Time.timeScale = 0f;
                        // StartCoroutine(ResumeTimeScale());
                        
                        _hitColliders.Add(hit.collider);
                        Notify(hit.collider.gameObject);
                    }
                }
                _previousPositions[i] = worldPosition;
            }
        }
    }

    public void Subscribe(IObserver<GameObject> observer)
    {
        if (!_observers.Contains(observer))
        {
            _observers.Add(observer);
        }
    }

    public void Unsubscribe(IObserver<GameObject> observer)
    {
        if (_observers.Contains(observer))
        {
            _observers.Remove(observer);
        }
    }

    public void Notify(GameObject value)
    {
        foreach (var observer in _observers)
        {
            observer.OnNext(value);
        }
    }
    
    
#if UNITY_EDITOR
    
    private void OnDrawGizmos()
    {
        if (_isAttacking)
        {
            for (int i = 0; i < _triggerZones.Length; i++)
            {
                var worldPosition = _triggerZones[i].referenceTransform.position + _triggerZones[i].offset;
                var direction = worldPosition - _previousPositions[i];
                
                Gizmos.color = Color.green;
                Gizmos.DrawWireSphere(worldPosition, _triggerZones[i].radius);
                
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(worldPosition + direction, _triggerZones[i].radius);
            }
        }
        else
        {
            foreach (var triggerZone in _triggerZones)
            {
                var worldPosition = triggerZone.referenceTransform.position + triggerZone.offset;
                Gizmos.color = Color.green;
                Gizmos.DrawSphere(worldPosition, triggerZone.radius);
            }   
        }
    }
    
#endif
}
