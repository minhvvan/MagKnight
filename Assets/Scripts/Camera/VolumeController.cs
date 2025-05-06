using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using DG.Tweening;

public static class VolumeController
{
    private static MotionBlur _motionBlur;
    private static ColorAdjustments _colorAdjustments;
    private static ChromaticAberration _chromaticAberration;

    private static bool _initialized = false;

    private static Tween _chromaticAberrationTween;
    private static Tween _saturationTween;
    private static Tween _motionBlurTween;

    public static void MotionBlurPlay(float intensity = 0.7f, float duration = 0.5f)
    {
        if (!_initialized || _motionBlur == null)
        {
            Initialize();
            if (_motionBlur == null)
            {
                Debug.LogWarning("MotionBlur not available.");
                return;
            }
        }

        _motionBlurTween?.Kill();

        _motionBlur.intensity.value = 0f;
        _motionBlur.active = true;

        _motionBlurTween = DOTween.To(() => _motionBlur.intensity.value, 
                                      x => _motionBlur.intensity.value = x, 
                                      intensity, duration)
            .SetUpdate(true);
    }

    public static void SetSaturation(float startSaturation, float targetSaturation, float duration, Ease ease = Ease.InQuint)
    {
        if (!_initialized || _colorAdjustments == null)
        {
            Initialize();
            if (_colorAdjustments == null)
            {
                Debug.LogWarning("ColorAdjustments not available.");
                return;
            }
        }

        _saturationTween?.Kill();

        _colorAdjustments.saturation.value = startSaturation;

        _saturationTween = DOTween.To(() => _colorAdjustments.saturation.value,
                                      x => _colorAdjustments.saturation.value = x,
                                      targetSaturation, duration)
            .SetEase(ease)
            .SetUpdate(true);
    }

    public static void SetChromaticAberration(float startValue, float targetValue, float duration, Ease ease = Ease.InQuint)
    {
        if (!_initialized || _chromaticAberration == null)
        {
            Initialize();
            if (_chromaticAberration == null)
            {
                Debug.LogWarning("ChromaticAberration not available.");
                return;
            }
        }

        _chromaticAberrationTween?.Kill();

        _chromaticAberration.intensity.value = startValue;

        _chromaticAberrationTween = DOTween.To(() => _chromaticAberration.intensity.value,
                                                x => _chromaticAberration.intensity.value = x,
                                                targetValue, duration)
            .SetEase(ease)
            .SetUpdate(true);
    }


    public static void Initialize()
    {
        var volume = GameObject.FindObjectOfType<Volume>();
        if (volume == null || volume.profile == null)
        {
            Debug.LogWarning("Global Volume이나 Profile이 존재하지 않습니다.");
            return;
        }

        if (!volume.profile.TryGet(out _motionBlur))
        {
            Debug.LogWarning("Volume Profile에 MotionBlur가 존재하지 않습니다.");
        }

        if (!volume.profile.TryGet(out _colorAdjustments))
        {
            Debug.LogWarning("Volume Profile에 ColorAdjustments가 존재하지 않습니다.");
        }

        if (!volume.profile.TryGet(out _chromaticAberration))
        {
            Debug.LogWarning("Volume Profile에 ChromaticAberration이 존재하지 않습니다.");
        }


        _initialized = true;
    }
}