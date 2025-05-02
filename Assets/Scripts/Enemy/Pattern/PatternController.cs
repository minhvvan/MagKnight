using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using UnityEngine;

public class PatternController : MonoBehaviour
{
    public List<PatternDataSO> patterns = new List<PatternDataSO>();
    private Dictionary<PatternDataSO, float> _activePatterns = new Dictionary<PatternDataSO, float>();
    private int currentPhase;

    private PatternDataSO _currentPattern;
    
    private PatternContext patternContext;

    void Awake()
    {
        currentPhase = 1;
        PhaseChange(currentPhase);
    }
    
    public PatternDataSO GetAvailablePattern(Transform executorTransform, Transform targetTransform)
    {
        foreach (PatternDataSO pattern in _activePatterns.Keys)
        {
            if (_activePatterns[pattern] < Time.time &&
                pattern.CanUse(executorTransform, targetTransform))
            {
                if(_currentPattern == null) { _currentPattern = pattern; }
                else
                {
                    _currentPattern = pattern.priority > _currentPattern.priority ? pattern : _currentPattern;
                }
            }
        }
        
        return _currentPattern;
    }

    public void PhaseChange(int phase)
    {
        foreach (PatternDataSO pattern in patterns)
        {
            if (pattern.phase <= phase)
            {
                _activePatterns[pattern] = Time.time;
            }
        }
    }

    public void ExecutePattern(Animator anim)
    {
        _currentPattern.Execute(anim);
        _activePatterns[_currentPattern] = Time.time + _currentPattern.cooldown;
        _currentPattern = null;
    }

    public void AttackStart(int patternId)
    {
        // animation event
        gameObject.GetComponent<HitDetector>().StartDetection(patternId);
    }

    public void AttackEnd(int patternId)
    {
        // animation event
        gameObject.GetComponent<HitDetector>().StopDetection(patternId);
    }
    
    
    
    #region debugging

    void OnDrawGizmos()
    {
        float radius = 0.5f;
        float range = 2f;
        
        Vector3 origin = transform.position + Vector3.up * 0.5f;
        Vector3 end = origin + transform.forward * range;
        
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(origin, radius);
        Gizmos.DrawWireSphere(end, radius);
        
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(origin + transform.right * radius, end + transform.right * radius);
        Gizmos.DrawLine(origin - transform.right * radius, end - transform.right * radius);
        Gizmos.DrawLine(origin + transform.up * radius, end + transform.up * radius);
        Gizmos.DrawLine(origin - transform.up * radius, end - transform.up * radius);
    }
    #endregion
}
