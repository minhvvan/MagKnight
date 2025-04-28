using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "EnemyStatSO", menuName = "SO/EnemyStat")]
public class EnemyStatSO : ScriptableObject
{
    public EnemyStat Stat = new EnemyStat();
}
