using TMPro;
using UnityEngine;
using DG.Tweening;


public class BlinkText : MonoBehaviour
{
    TextMeshProUGUI _text;

    void Start()
    {
        _text = GetComponent<TextMeshProUGUI>();
        if (_text == null)
        {
            Debug.LogError("TextMeshProUGUI component not found on this GameObject.");
            return;
        }

        _text.DOFade(0, 0.5f)
            .SetLoops(-1, LoopType.Yoyo)
            .SetEase(Ease.InOutSine)
            .OnKill(() => _text.color = new Color(_text.color.r, _text.color.g, _text.color.b, 1));
    }
}