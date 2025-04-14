// ComboStep.cs
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ComboStep
{
    [Header("이 스텝을 구분할 이름(디버그용)")]
    public string stepName;             

    [Header("재생할 애니메이션 이름(또는 클립)")]
    public string animationName;         

    [Header("다음 콤보 입력을 받기 시작하는 타이밍")]
    public float comboInputStartTime;    

    [Header("다음 콤보 입력을 받을 수 있는 마지막 타이밍")]
    public float comboInputEndTime;      

    [Header("분기 정보: 어떤 입력(강/약)을 하면 다음 단계로 이동하는가?")]
    public List<ComboBranch> nextBranches;

    [Header("공격 판정(대미지 등) 필요시 추가할 데이터들")]
    public float damage;
    public float attackRange;
    // 필요하다면 이펙트, 사운드, 히트 리액션 등 추가
}