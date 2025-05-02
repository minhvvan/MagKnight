using System;
using System.Collections;
using System.Collections.Generic;
using AYellowpaper.SerializedCollections;
using DG.Tweening;
using UnityEngine;

public enum VisualEffectType
{
    None,
    Phase,
    Dissolve,
    Max
}

public class Effector : MonoBehaviour
{
    [SerializeField] private SerializedDictionary<VisualEffectType, Material> _visualEffects = new SerializedDictionary<VisualEffectType, Material>();
    
    private static readonly int SplitValue = Shader.PropertyToID("_SplitValue");
    private Renderer _renderer;
    
    private void Awake()
    {
        _renderer = GetComponentInChildren<Renderer>();
    }

    public void Dissolve(float duration = 2f, Action onComplete = null)
    {
        if (!_visualEffects[VisualEffectType.Dissolve]) return;
        _renderer.material = _visualEffects[VisualEffectType.Dissolve];
        
        _renderer.material.DOFloat(0, SplitValue, duration).SetEase(Ease.InOutCubic).OnComplete(() =>
        {
            onComplete?.Invoke();
        });
    }
    
    public void Phase(float duration = 2f, Action onComplete = null)
    {
        if (!_visualEffects[VisualEffectType.Phase]) return;
        _renderer.material = _visualEffects[VisualEffectType.Phase];

        _renderer.material.DOFloat(2, SplitValue, duration).SetEase(Ease.InOutCubic).OnComplete(() =>
        {
            onComplete?.Invoke();
        });
    }
}
