using TMPro;
using UnityEngine;
using DG.Tweening;
using Cysharp.Threading.Tasks;


public class FadeText : MonoBehaviour
{
    TextMeshProUGUI _text;

    void Awake()
    {
        _text = GetComponent<TextMeshProUGUI>();
    }

    public void SetTextAndShowFadeInAndOut(string text, bool isSetNewName)
    {
        if(isSetNewName)
        {
            _text.text = text;
        }
        _text.color = new Color(_text.color.r, _text.color.g, _text.color.b, 0);
        FadeInAndOut(0.5f).Forget();
    }

    async UniTaskVoid FadeInAndOut(float duration)
    {
        await _text.DOFade(1, duration).SetEase(Ease.InOutSine).AsyncWaitForCompletion();
        await UniTask.Delay(1000);
        await _text.DOFade(0, duration).SetEase(Ease.InOutSine).AsyncWaitForCompletion();
    }
}