using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "HealthPack", menuName = "SO/Item/HealthPack")]
public class HealthPackSO : ScriptableObject
{
    public Sprite icon;
    public string itemName;
    public string description;
    public float healValue;
    public int scrapValue;
}
