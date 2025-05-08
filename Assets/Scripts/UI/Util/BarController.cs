using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
namespace Moon
{
    public class BarController : MonoBehaviour
    {
        [SerializeField] Image fillImage;
        [SerializeField] Image frameActiveImage;
        Image _backgorundImage;

        void Awake()
        {
            _backgorundImage = GetComponent<Image>();
        }
        
        public void SetFillAmount(float amount, bool smoothly)
        {   
            if(smoothly)
            {
                fillImage.DOFillAmount(amount, 0.5f).SetEase(Ease.OutSine);
            }
            else
            {
                fillImage.fillAmount = amount;
            }

            if(frameActiveImage != null)
            {
                if(amount >= 1)
                {
                    frameActiveImage.gameObject.SetActive(true);
                }
                else
                {
                    frameActiveImage.gameObject.SetActive(false);
                }
            }
        }
    }
}