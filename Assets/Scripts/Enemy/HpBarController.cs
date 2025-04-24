using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class HpBarController : MonoBehaviour
{
    [SerializeField] private Image hpGauge;
    [SerializeField] private bool isWorldCanvas;
    private Camera _mainCamera;

    void Awake()
    {
        _mainCamera = Camera.main;
    }
    
    private void Update()
    {
        if (isWorldCanvas)
        {
            var cameraTransform = Camera.main.transform;
            transform.rotation = cameraTransform.rotation;
        }
    }

    public void SetHP(float hp)
    {
        hpGauge.fillAmount = hp;
    }
}
