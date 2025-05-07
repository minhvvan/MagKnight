using System;
using System.Collections.Generic;
using System.Threading;
using AYellowpaper.SerializedCollections;
using DG.Tweening;
using UnityEditor;
using UnityEngine;
using Highlighters;
using UnityEngine.Serialization;
using Highlighter = Highlighters.Highlighter;


public enum VisualEffectType
{
    None,
    Phase,
    Dissolve,
    Max
}

public enum HighlightType
{
    None,
    Hit,
    Magnetic,
    Max
}


public class Effector : MonoBehaviour
{
    [SerializeField] private SerializedDictionary<VisualEffectType, Material> _visualEffects = new SerializedDictionary<VisualEffectType, Material>();
    [SerializeField] private SerializedDictionary<HighlightType, Highlighter> _highlighters = new SerializedDictionary<HighlightType, Highlighter>();
    [SerializeField] private SerializedDictionary<MagneticType, Color> colors = new SerializedDictionary<MagneticType, Color>();
    [SerializeField] private GameObject switchPolarityRenderer;
    
    private static readonly int SplitValue = Shader.PropertyToID("_SplitValue");
    private static readonly int ProgressValue = Shader.PropertyToID("_Progress");
    private static readonly int GlowColor = Shader.PropertyToID("_GlowColor");

    private Renderer _renderer;

    private CancellationToken _cts;
    private Tween _hitTween;
    private Tween _changePolarityTween;
    
    private void Awake()
    {
        _renderer = GetComponentInChildren<Renderer>();
    }

    public void Dissolve(float duration = 2f, Action onComplete = null)
    {
        if (!_visualEffects.ContainsKey(VisualEffectType.Dissolve)) return;
        _renderer.material = _visualEffects[VisualEffectType.Dissolve];
        
        _renderer.material.DOFloat(0, SplitValue, duration).SetEase(Ease.InOutCubic).OnComplete(() =>
        {
            onComplete?.Invoke();
        });
    }
    
    public void Phase(float duration = 2f, Action onComplete = null)
    {
        if (!_visualEffects.ContainsKey(VisualEffectType.Phase)) return;
        _renderer.material = _visualEffects[VisualEffectType.Phase];

        _renderer.material.DOFloat(2, SplitValue, duration).SetEase(Ease.InOutCubic).OnComplete(() =>
        {
            onComplete?.Invoke();
        });
    }

    public void SwitchPolarity(MagneticType magneticType, float duration = 2f, Action onComplete = null)
    {
        if (!switchPolarityRenderer) return;
        
        switchPolarityRenderer.SetActive(true);

        var renderer = switchPolarityRenderer.GetComponent<Renderer>();

        var materials = new List<Material>();
        renderer.GetMaterials(materials);

        foreach (var material in materials)
        {
            material.SetColor(GlowColor, colors[magneticType]);
        }

        float progress = 0;
        
        _changePolarityTween = DOTween.To(() => progress, value => {
                progress = value;
                foreach (var material in materials)
                {
                    material.SetFloat(ProgressValue, progress);
                }
            }, 1f, duration)
            .SetEase(Ease.OutQuad)
            .OnComplete(() => {
                onComplete?.Invoke();
                foreach (var material in materials)
                {
                    material.SetFloat(ProgressValue, 0);
                }
            });
    }

    public void OnHit(float duration, Action onComplete = null)
    {
        if (!_highlighters.ContainsKey(HighlightType.Hit)) return;
        
        // 기존 트윈이 실행 중이면 취소
        if (_hitTween != null && _hitTween.IsActive())
        {
            _hitTween.Kill();
        }
        
        _highlighters[HighlightType.Hit].enabled = true;

        Color currentColor = _highlighters[HighlightType.Hit].Settings.OverlayFront.Color;
        float startAlpha = 1;

        _highlighters[HighlightType.Hit].Settings.OverlayFront.Color += new Color(0, 0, 0, 1);

        _hitTween = DOTween.To(() => startAlpha, value => {
                _highlighters[HighlightType.Hit].Settings.OverlayFront.Color = new Color(
                    currentColor.r, 
                    currentColor.g, 
                    currentColor.b, 
                    value
                );
                
            }, 0f, duration)
            .SetEase(Ease.OutQuad)
            .OnComplete(() => {
                onComplete?.Invoke();
                _highlighters[HighlightType.Hit].enabled = false;

            });
    }
}
