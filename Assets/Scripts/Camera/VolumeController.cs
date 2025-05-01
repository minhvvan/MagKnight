using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using DG.Tweening;

public static class VolumeController
{
    private static MotionBlur _motionBlur;
    private static ColorAdjustments _colorAdjustments;

    private static bool _initialized = false;

    public static void MotionBlurPlay(float intensity = 0.7f, float duration = 0.5f)
    {
        if (!_initialized || _motionBlur == null)
        {
            Initialize();
        }

        if (_motionBlur == null) return;

        _motionBlur.intensity.value = 0f;
        _motionBlur.active = true;

        DOTween.To(() => _motionBlur.intensity.value, x => _motionBlur.intensity.value = x, intensity, duration)
        .OnComplete(() => {
            if(intensity <= 0f)
            {
                _motionBlur.active = false;
            }
        });
    }

    public static void SetSaturation(float startSaturation,  float targetSaturation, float duration, Ease ease = Ease.InQuint)
    {
        if (!_initialized || _colorAdjustments == null)
        {
            Initialize();
        }

        if (_colorAdjustments == null) return;

        DOTween.Kill(_colorAdjustments.saturation);
        
        _colorAdjustments.saturation.value = startSaturation;
        
        
        DOTween.To(() => _colorAdjustments.saturation.value, x => _colorAdjustments.saturation.value = x, targetSaturation, duration).SetEase(ease).SetUpdate(true);
    }




    private static void Initialize()
    {
        var volume = GameObject.FindObjectOfType<Volume>();
        if (volume == null || volume.profile == null)
        {
            Debug.LogWarning("[MotionBlurController] Global Volume이나 Profile이 존재하지 않습니다.");
            return;
        }

        if (!volume.profile.TryGet(out _motionBlur))
        {
            Debug.LogWarning("[MotionBlurController] Volume Profile에 MotionBlur가 존재하지 않습니다.");
        }

        if (!volume.profile.TryGet(out _colorAdjustments))
        {
            Debug.LogWarning("[MotionBlurController] Volume Profile에 ColorAdjustments가 존재하지 않습니다.");
        }


        _initialized = true;
    }
}