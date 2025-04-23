using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "MagneticSetup", menuName = "SO/Magnetic/MagneticSetup")]
public class MagneticSetupSO : ScriptableObject
{
    [Header("Scan Max Unit")]
    public int maxInCount; //탐지 개체 최대 수
    
    [Header("Movement")]
    public float dragValue; //가속 후 감속값
    public float structSpeed; //구조물 대상 적용 속도
    public float nonStructSpeed; //비구조물 대상 적용 속도
    
    
    [Header("Magnetic & Scan Value")]
    public float minDistance;//대상 오브젝트가 일정 거리 이내로 다가올 시 자기력 상호작용을 종료할 최소거리.
    public float outBoundDistance;//자기력이 작용하는 최대 거리
    public float hangAdjustValue;//Vector만 전달 시 y축 보정

    [Header("CounterPress")] 
    public float counterPressRange;
    public float counterPressPower;
    public void OnValidate()
    {
        hangAdjustValue = outBoundDistance/10f+0.25f; //1.2~2f
    }
}
