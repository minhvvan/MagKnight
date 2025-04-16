using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Moon
{
    public class ScreenFader : Singleton<ScreenFader>
    {
        public enum FadeType
        {
            CommonFade, Loading, Intro,
        }

        public static bool IsFading
        {
            get { return Instance._isFading; }
        }

        [SerializeField] public CanvasGroup commonFaderCanvasGroup;
        [SerializeField] public CanvasGroup loadingCanvasGroup;
        [SerializeField] public float fadeDuration = 1f;

        protected bool _isFading;
        const int k_MaxSortingLayer = 32767;

        protected IEnumerator Fade(float finalAlpha, CanvasGroup canvasGroup)
        {
            _isFading = true;
            canvasGroup.blocksRaycasts = true;
            float fadeSpeed = Mathf.Abs(canvasGroup.alpha - finalAlpha) / fadeDuration;
            while (!Mathf.Approximately(canvasGroup.alpha, finalAlpha))
            {
                canvasGroup.alpha = Mathf.MoveTowards(canvasGroup.alpha, finalAlpha,
                    fadeSpeed * Time.deltaTime);
                yield return null;
            }
            canvasGroup.alpha = finalAlpha;
            _isFading = false;
            canvasGroup.blocksRaycasts = false;
        }

        public static void SetAlpha(float alpha)
        {
            Instance.commonFaderCanvasGroup.alpha = alpha;
        }

        
        public static IEnumerator FadeSceneIn()
        {
            CanvasGroup canvasGroup;
            if (Instance.commonFaderCanvasGroup.alpha > 0.1f)
                canvasGroup = Instance.commonFaderCanvasGroup;
            else
                canvasGroup = Instance.loadingCanvasGroup;

            yield return Instance.StartCoroutine(Instance.Fade(0f, canvasGroup));

            canvasGroup.gameObject.SetActive(false);
        }

        public static IEnumerator FadeSceneOut(FadeType fadeType = FadeType.CommonFade)
        {
            CanvasGroup canvasGroup;
            switch (fadeType)
            {
                case FadeType.CommonFade:
                    canvasGroup = Instance.commonFaderCanvasGroup;
                    break;
                default:
                    canvasGroup = Instance.loadingCanvasGroup;
                    break;
            }

            canvasGroup.gameObject.SetActive(true);

            yield return Instance.StartCoroutine(Instance.Fade(1f, canvasGroup));
        }
    }
}