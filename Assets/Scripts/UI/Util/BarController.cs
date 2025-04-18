using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Cysharp.Threading.Tasks;

namespace Moon
{
    public class BarController : MonoBehaviour
    {
        [SerializeField] Image fillImage;
        Image _backgorundImage;

        void Awake()
        {
            _backgorundImage = GetComponent<Image>();
            _ = Test();
        }

        //Test Code Update full -> empty repeatedly
        async UniTask Test()
        {
            while (true)
            {
                SetFillAmount(1, true);
                await UniTask.Delay(1000, cancellationToken: this.GetCancellationTokenOnDestroy());
                SetFillAmount(0, true);
                await UniTask.Delay(1000, cancellationToken: this.GetCancellationTokenOnDestroy());
            }
        }

        
        public void SetFillAmount(float amount, bool smoothly)
        {
            var fullWidth = _backgorundImage.rectTransform.rect.width;
            var fillWidth = fullWidth * amount;
            
            
            if(smoothly)
            {
                fillImage.rectTransform.DOSizeDelta(new Vector2(fillWidth, _backgorundImage.rectTransform.rect.height), 0.5f).SetEase(Ease.InOutSine);
            }
            else
            {
                fillImage.rectTransform.sizeDelta = new Vector2(fillWidth, _backgorundImage.rectTransform.rect.height);
            }
        }
    }
}