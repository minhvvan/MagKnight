using AYellowpaper.SerializedCollections;
using UnityEngine;

[CreateAssetMenu(menuName = "SO/Weapon/Dictionary", fileName = "WeaponDictionary")]
public class WeaponDataDictSO: ScriptableObject
{
    public SerializedDictionary<WeaponType, WeaponDataSO> weapons;
}
