using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using hvvan;
using UnityEngine;

public class MagneticFilter : MonoBehaviour
{
    [SerializeField] private float duration;
    [SerializeField] private float maxRange;
    
    private Material _material;
    private Tween _magneticFilterTween;
    private static readonly int SplitValue = Shader.PropertyToID("_SplitValue");

    private void Awake()
    {
        _material = GetComponent<Renderer>().material;
        GameManager.Instance.OnMagneticPressed += OnMagneticPressed;
        GameManager.Instance.OnMagneticReleased += OnMagneticReleased;
        _material.SetFloat(SplitValue, 0);
    }

    private void OnMagneticPressed()
    {
        if (_magneticFilterTween != null && _magneticFilterTween.IsActive())
        {
            _magneticFilterTween.Kill();
        }
        
        _magneticFilterTween = DOTween.To(() => 0, value =>
            {
                _material.SetFloat(SplitValue, value);

            }, maxRange, duration)
            .SetEase(Ease.OutQuad);
            
        VFXManager.Instance.TriggerVFX(VFXType.MAGNET_AIM_SHOCKWAVE, transform);
    }

    private void OnMagneticReleased()
    {
        if (_magneticFilterTween != null && _magneticFilterTween.IsActive())
        {
            _magneticFilterTween.Kill();
        }
        
        _magneticFilterTween = DOTween.To(() => maxRange, value =>
            {
                _material.SetFloat(SplitValue, value);

            }, 0f, duration)
            .SetEase(Ease.OutQuad);
    }
}
