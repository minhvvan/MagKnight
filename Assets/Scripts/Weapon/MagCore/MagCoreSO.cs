using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "MagCoreSO", menuName = "SO/MagCore/MagCoreSO")]
public class MagCoreSO : ScriptableObject
{
    public Sprite icon;
    public string itemName;
    public string description;
    public WeaponType weaponType;
    public PartsType partsType;
    public int scrapValue;
}
