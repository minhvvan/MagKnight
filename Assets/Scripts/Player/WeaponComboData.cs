// WeaponComboData.cs
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "WeaponComboData", menuName = "ScriptableObjects/Weapon Combo Data")]
public class WeaponComboData : ScriptableObject
{
    [Header("이 무기의 콤보 스텝 정보")]
    public List<ComboStep> comboSteps;

    /// <summary>
    /// 특정 스텝에서 받은 AttackType 입력에 따라 다음 스텝 인덱스를 찾는 함수
    /// </summary>
    public int GetNextStepIndex(int currentStepIndex, AttackType input)
    {
        // 리스트 범위 벗어나는지 체크
        if (currentStepIndex < 0 || currentStepIndex >= comboSteps.Count)
        {
            Debug.LogWarning($"유효하지 않은 스텝 인덱스: {currentStepIndex}");
            return -1; // 콤보 종료 등의 처리를 위해 -1 반환
        }

        // 현재 스텝의 분기 정보를 확인
        ComboStep currentStep = comboSteps[currentStepIndex];
        foreach (var branch in currentStep.nextBranches)
        {
            if (branch.inputType == input)
            {
                // 해당 입력에 맞는 다음 스텝 인덱스
                return branch.nextStepIndex;
            }
        }

        // 해당 스텝에서 처리하지 않는 입력이라면, 콤보가 끊기거나 -1
        return -1;
    }
}