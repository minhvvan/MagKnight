using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "MagneticObjectInfo", menuName = "SO/Magnetic/MagneticObjectInfo")]
public class MagneticObjectSO : ScriptableObject
{
    public string magneticObjectName; // 적용될 오브젝트 명
    public MagneticType magneticType; //오브젝트의 극 (N,S)
    public bool isStructure; //움직이지 않는 구조물인가?
    public float objectMass; //오브젝트 중량
}
