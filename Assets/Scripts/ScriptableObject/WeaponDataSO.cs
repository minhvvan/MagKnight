using AYellowpaper.SerializedCollections;
using UnityEngine;

[CreateAssetMenu(menuName = "SO/Weapon/WeaponData", fileName = "WeaponData")]
public class WeaponDataSO: ScriptableObject
{
    public GameObject prefab;
    
    public float range = 1f;
}
