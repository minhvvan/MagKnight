
using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class GateIndicatorUI : MonoBehaviour
{
    [SerializeField] private Image image;

    [SerializeField] private float moveDistance = 30f; // 위아래로 움직일 거리
    [SerializeField] private float moveDuration = 1f; // 한 방향으로 움직이는 시간
    [SerializeField] private Ease easeType = Ease.InOutQuad; // 이징 타입

    private Vector3 startPos;
    
    private void Start()
    {
        if (image == null)
            image = GetComponent<Image>();

        startPos = image.rectTransform.localPosition;
    
        image.rectTransform.DOLocalMoveY(startPos.y + moveDistance, moveDuration)
            .SetEase(easeType)
            .SetLoops(-1, LoopType.Yoyo); // Yoyo는 왕복 움직임
    }
}
