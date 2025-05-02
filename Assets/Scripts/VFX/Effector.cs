using System;
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
    
    private static readonly int SplitValue = Shader.PropertyToID("_SplitValue");
    private Renderer _renderer;

    private CancellationToken _cts;
    private Tween _hitTween;
    
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

    public void OnHit(float duration, Action onComplete = null)
    {
        if(!_highlighters[HighlightType.Hit]) return;
        
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

    public void OnMagneticPressed(MagneticType magneticType)
    {
        if (!_highlighters.ContainsKey(HighlightType.Magnetic)) return;
        
        var highlighter = _highlighters[HighlightType.Magnetic];
        
        //color setting
        highlighter.Settings.OverlayFront.Color = colors[magneticType];
        highlighter.Settings.OverlayBack.Color = colors[magneticType];
        highlighter.Settings.MeshOutlineFront.Color = colors[magneticType];
        highlighter.Settings.MeshOutlineBack.Color = colors[magneticType];
        highlighter.Settings.OuterGlowColorFront = colors[magneticType];
        highlighter.Settings.OuterGlowColorBack = colors[magneticType];
        
        _highlighters[HighlightType.Magnetic].enabled = true;
    }

    public void OnMagneticReleased()
    {
        if (!_highlighters.ContainsKey(HighlightType.Magnetic)) return;
        _highlighters[HighlightType.Magnetic].enabled = false;
    }
}
