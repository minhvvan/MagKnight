using UnityEngine;
using UnityEngine.UI;

public class BlinkImage : MonoBehaviour
{
    Image _image;
    [SerializeField] float _blinkDuration = 0.5f;

    float _timer;
    float _currentAlpha = 0f;
    float _targetAlpha = 1f;

    private void Awake()
    {
        _image = GetComponent<Image>();
    }

    void Update()
    {
        _timer += Time.deltaTime;

        if (_timer >= _blinkDuration)
        {
            _timer = 0f;
            _currentAlpha = _image.color.a;
            _targetAlpha = _currentAlpha == 1f ? 0f : 1f;
        }

        float alpha = Mathf.Lerp(_currentAlpha, _targetAlpha, _timer / _blinkDuration);
        Color color = _image.color;
        color.a = alpha;
        _image.color = color;
    }
}