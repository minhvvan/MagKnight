using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "VFXData", menuName = "SO/FX/VFXData")]
public class VFXDataSO : ScriptableObject
{
    public VFXType vfxType;
    public GameObject vfxPrefab;
    public int poolSize;
    public float duration;
    public Vector3 size;
}
