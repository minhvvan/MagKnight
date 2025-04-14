// ComboBranch.cs
using UnityEngine;

[System.Serializable]
public struct ComboBranch
{
    public AttackType inputType;  // 예: 약(Weak)인지 강(Strong)인지
    public int nextStepIndex;     // 만약 약이면 nextStepIndex 번 스텝으로 이동
}