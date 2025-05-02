using System;
using System.Threading;
using AYellowpaper.SerializedCollections;
using DG.Tweening;
using UnityEditor;
using UnityEngine;
using Highlighters;
using Highlighter = Highlighters.Highlighter;


public enum VisualEffectType
{
    None,
    Phase,
    Dissolve,
    Max
}

[RequireComponent(typeof(Highlighters.Highlighter))]
public class Effector : MonoBehaviour
{
    [SerializeField] private SerializedDictionary<VisualEffectType, Material> _visualEffects = new SerializedDictionary<VisualEffectType, Material>();
    
    [Header("Hit Highlighter")]
    [SerializeField] private HighlighterSettings _hitHighlightSettings = new HighlighterSettings();
    
    [Header("Magnetic Highlighter")]
    [SerializeField] private HighlighterSettings _maneticHighlightSettings = new HighlighterSettings();
    
    private static readonly int SplitValue = Shader.PropertyToID("_SplitValue");
    private Renderer _renderer;
    private Highlighters.Highlighter _highlighter;

    private CancellationToken _cts;
    private Tween _tween;
    
    private void Awake()
    {
        _renderer = GetComponentInChildren<Renderer>();
        _highlighter = GetComponent<Highlighters.Highlighter>();
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

    public void OnHit(float duration, Action onComplete = null)
    {
        // 기존 트윈이 실행 중이면 취소
        if (_tween != null && _tween.IsActive())
        {
            _tween.Kill();
        }
    
        _highlighter.Settings = _hitHighlightSettings;
    
        Color currentColor = _highlighter.Settings.OverlayFront.Color;
        float startAlpha = 1;

        _highlighter.Settings.OverlayFront.Color += new Color(0, 0, 0, 1);
    
        _tween = DOTween.To(() => startAlpha, value => {
                _highlighter.Settings.OverlayFront.Color = new Color(
                    currentColor.r, 
                    currentColor.g, 
                    currentColor.b, 
                    value
                );

                Highlighter.HighlightersNeedReset();
            }, 0f, duration)
            .SetEase(Ease.OutQuad)
            .OnComplete(() => {
                onComplete?.Invoke();
            });
    }
}
