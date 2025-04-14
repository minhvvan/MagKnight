using AYellowpaper.SerializedCollections;
using UnityEngine;

[CreateAssetMenu(menuName = "SO/Weapon", fileName = "WeaponPrefabs")]
public class WeaponPrefabSO: ScriptableObject
{
    public SerializedDictionary<WeaponType, GameObject> weapons;
}
