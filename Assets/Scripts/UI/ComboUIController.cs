using DamageNumbersPro;
using UnityEngine;

public class ComboUIController : MonoBehaviour
{
    [SerializeField] private DamageNumber _damageNumberPrefab;
    [SerializeField] private float _comboDuration = 1f;

    RectTransform _rectTransform;

    int _comboCount;
    float _comboTimer;
    private void Awake()
    {
        _comboCount = 0;
        _comboTimer = 0f;
        _rectTransform = GetComponent<RectTransform>();
    }
    private void Update()
    {
        if (_comboCount > 0)
        {
            _comboTimer += Time.deltaTime;
            if (_comboTimer >= _comboDuration)
            {
                ResetCombo();
            }
        }
    }

    public void AddCombo()
    {
        _comboCount++;
        _comboTimer = 0f;

        // Create a damage number
        DamageNumber damageNumber = Instantiate(_damageNumberPrefab, _rectTransform);
        damageNumber.SpawnGUI(_rectTransform, Vector2.zero, $"{_comboCount}");

    }

    private void ResetCombo()
    {
        _comboCount = 0;
        _comboTimer = 0f;
    }
}
