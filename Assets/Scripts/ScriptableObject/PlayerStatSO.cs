
// 매핑을 위한 ScriptableObject

using System.Collections.Generic;
using AYellowpaper.SerializedCollections;
using UnityEngine;

[CreateAssetMenu(fileName = "PlayerStatSO", menuName = "SO/PlayerStat")]
public class PlayerStatSO : ScriptableObject
{
    public PlayerStat Stat = new PlayerStat();
}