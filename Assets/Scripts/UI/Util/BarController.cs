using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System;
using TMPro;
using System.Collections;
using Cysharp.Threading.Tasks;
namespace Moon
{
    public class BarController : MonoBehaviour
    {
        [SerializeField] Image fillImage;
        [SerializeField] Image delayFillImage;
        [SerializeField] CanvasGroup frameActiveCanvasGroup;
        [SerializeField] TextMeshProUGUI _valueText;
        Image _backgorundImage;
        RectTransform _rectTransform;
        //HP바에 체력감소를 띄워주기 위한 변수
        [SerializeField] public RectTransform damageTextRectTransform;
        [SerializeField] bool isPunch = false;


        //체력최대값과 실제 값을 변화시키는 과정을 보여주기 위함
        float _currentMaxValue = 0;
        float _currentValue = 0;
        float _currentValueTarget = 0;
        float _currentMaxValueTarget = 0;

        //canvasGroup flag
        float _canvasGroupTargetAlpha = 0;

        bool _changeValueFlag = false;

        void Awake()
        {
            _backgorundImage = GetComponent<Image>();
            _rectTransform = GetComponent<RectTransform>();
        }        

        public void SetFillAmount(float amount, bool smoothly)
        {   
            if(_rectTransform == null)
            {
                _rectTransform = GetComponent<RectTransform>();
            }

            if(smoothly)
            {
                fillImage.DOFillAmount(amount, 0.5f).SetEase(Ease.OutSine);
            }
            else
            {
                fillImage.fillAmount = amount;
            }

            SetFillAmountWithDelay(amount, 1f);

            if(frameActiveCanvasGroup != null)
            {
                if(amount >= 1)
                {
                    frameActiveCanvasGroup.gameObject.SetActive(true);
                    frameActiveCanvasGroup.alpha = 0;
                    _canvasGroupTargetAlpha = 1;
                }
                else
                {
                    frameActiveCanvasGroup.alpha = 0;
                    frameActiveCanvasGroup.gameObject.SetActive(false);
                }
            }

            if(isPunch && smoothly)
            {
                DOTween.Kill(_rectTransform);
                _rectTransform.localScale = Vector3.one;
                _rectTransform.DOPunchScale(Vector3.one * 0.1f, 0.5f, 1, 0.5f).OnComplete(() =>
                {
                    _rectTransform.localScale = Vector3.one;
                });
            }
        }

        //체력이 설정 된 후 1초 후에 체력바가 사라짐
        public void SetFillAmountWithDelay(float amount, float delayTime)
        {
            if (delayFillImage != null)
            {
                delayFillImage.DOFillAmount(amount, 0.5f).SetDelay(delayTime);
            }
        }

        void SetValueText(string value)
        {
            if (_valueText != null)
            {
                _valueText.text = value.ToString();
            }
        }

        public void SetValue(float value, float maxValue)
        {
            _currentValueTarget = value;
            _currentMaxValueTarget = maxValue;

            try
            {
                if (_currentValueTarget != _currentValue)
                {
                    _ = UpdateValue(_currentValueTarget);    
                }

                if (_currentMaxValueTarget != _currentMaxValue)
                {
                    _ = UpdateMaxValue(_currentMaxValueTarget);
                }
            }
            catch
            {
                _currentValue = _currentValueTarget;
                _currentMaxValue = _currentMaxValueTarget;
            }
        }

        private async UniTask UpdateValue(float newValue, float duration = 0.5f)
        {
            float startValue = _currentValue;
            float elapsedTime = 0f;

            while (elapsedTime < duration)
            {
                await UniTask.Yield(PlayerLoopTiming.Update);
                elapsedTime += Time.deltaTime;
                _currentValue = (int)Mathf.Lerp(startValue, newValue, elapsedTime / duration);
                _changeValueFlag = true;
            }

            _currentValue = (int)newValue;
            _changeValueFlag = true;
        }

        private async UniTask UpdateMaxValue(float newValue, float duration = 0.3f)
        {
            float startValue = _currentMaxValue;
            float elapsedTime = 0f;

            while (elapsedTime < duration)
            {
                await UniTask.Yield(PlayerLoopTiming.Update);
                elapsedTime += Time.deltaTime;
                _currentMaxValue = (int)Mathf.Lerp(startValue, newValue, elapsedTime / duration);
                _changeValueFlag = true;
            }

            _currentMaxValue = (int)newValue;
            _changeValueFlag = true;
        }
        


        void Update()
        {
            //Blink frameActive
            if(frameActiveCanvasGroup != null)
            {
                if(frameActiveCanvasGroup.gameObject.activeSelf)
                {
                    if(_canvasGroupTargetAlpha == 1)
                    {
                        frameActiveCanvasGroup.alpha = Mathf.MoveTowards(frameActiveCanvasGroup.alpha, 1, Time.deltaTime);
                        if(frameActiveCanvasGroup.alpha >= 0.95f)
                        {
                            _canvasGroupTargetAlpha = 0;
                        }
                    }
                    else
                    {
                        frameActiveCanvasGroup.alpha = Mathf.MoveTowards(frameActiveCanvasGroup.alpha, 0, Time.deltaTime);
                        if(frameActiveCanvasGroup.alpha <= 0.05f)
                        {
                            _canvasGroupTargetAlpha = 1;
                        }
                    }
                }
            }

            if (_changeValueFlag)
            {
                _changeValueFlag = false;
                SetValueText($"[{Mathf.RoundToInt(_currentValue)}/{Mathf.RoundToInt(_currentMaxValue)}]");
            }
        }
    }
}