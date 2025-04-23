using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AYellowpaper.SerializedCollections;
using UnityEngine;
using VFolders.Libs;

public class PatternController : MonoBehaviour
{
    public List<PatternDataSO> patterns = new List<PatternDataSO>();
    private HashSet<PatternDataSO> _activePatterns = new HashSet<PatternDataSO>();
    private int currentPhase;

    private PatternDataSO _currentPattern;
    
    private PatternContext patternContext;

    private Dictionary<string, int> patternsDictionary = new Dictionary<string, int>()
    {
        { "SwingAttack", 1 },
        { "Kick", 2}
    };

    void Awake()
    {
        currentPhase = 1;
        PhaseChange(currentPhase);
    }
    
    public PatternDataSO GetAvailablePattern(Transform executorTransform, Transform targetTransform)
    {
        foreach (PatternDataSO pattern in _activePatterns)
        {
            if (pattern.CanUse(executorTransform, targetTransform))
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
                _activePatterns.Add(pattern);
            }
        }
    }

    public void ExecutePattern(Animator anim)
    {
        _currentPattern.Execute(anim);
    }

    public void AttackStart()
    {
        int patternId = patternsDictionary[_currentPattern.patternName];
        gameObject.GetComponent<HitDetector>().StartDetection(patternId);
    }

    public void AttackEnd()
    {
        gameObject.GetComponent<HitDetector>().StopDetection();
        _currentPattern.OnExecute();
        _currentPattern = null;
    }
}
