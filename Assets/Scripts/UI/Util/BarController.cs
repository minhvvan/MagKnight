using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System;
namespace Moon
{
    public class BarController : MonoBehaviour
    {
        [SerializeField] Image fillImage;
        [SerializeField] Image delayFillImage;
        [SerializeField] CanvasGroup frameActiveCanvasGroup;
        Image _backgorundImage;
        RectTransform _rectTransform;
        //HP바에 체력감소를 띄워주기 위한 변수
        [SerializeField] public RectTransform damageTextRectTransform;
        [SerializeField] bool isPunch = false;


        //canvasGroup flag
        float _canvasGroupTargetAlpha = 0;

        void Awake()
        {
            _backgorundImage = GetComponent<Image>();
            _rectTransform = GetComponent<RectTransform>();
        }
        
        public void SetFillAmount(float amount, bool smoothly)
        {   
            if(_rectTransform == null) return;

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
        }
    }
}