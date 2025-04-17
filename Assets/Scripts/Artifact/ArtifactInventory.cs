using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class ArtifactInventory : MonoBehaviour
{
    public ArtifactDataSO[] Left_ArtifactGas = new ArtifactDataSO[15];
    public ArtifactDataSO[] Right_ArtifactGas = new ArtifactDataSO[15];

    public AbilitySystem abilitySystem;
    private MagneticController _magneticController;
    
    void Start()
    {
        abilitySystem = GetComponent<AbilitySystem>();
        _magneticController = GetComponent<MagneticController>();
    }
    
    private void N_ApplyAll(ArtifactDataSO[] artifactList)
    {
        foreach (var artifact in artifactList)
        {
            if(artifact != null)
                artifact.N_ApplyTo(abilitySystem);
        }
    }

    private  void S_ApplyAll(ArtifactDataSO[] artifactList)
    {
        foreach (var artifact in artifactList)
        {
            if (artifact != null)
                artifact.S_ApplyTo(abilitySystem);
        }
    }
    
    private  void N_RemoveAll(ArtifactDataSO[] artifactList)
    {
        foreach (var artifact in artifactList)
        {
            if (artifact != null)
                artifact.N_RemoveTo(abilitySystem);
        }
    }
    
    private  void S_RemoveAll(ArtifactDataSO[] artifactList)
    {
        foreach (var artifact in artifactList)
        {
            if (artifact != null)
                artifact.S_RemoveTo(abilitySystem);
        }
    }

    public void SetLeftArtifact(int index, ArtifactDataSO artifact)
    {
        // 현재 위치의 아티팩트가 위치하면 극에 맞게 기존 특성 지우기
        if (Left_ArtifactGas[index] != null)
        {
            if (_magneticController.magneticType == MagneticType.N)
            {
                Left_ArtifactGas[index].N_RemoveTo(abilitySystem);
            }
            else
            {
                Left_ArtifactGas[index].S_RemoveTo(abilitySystem);

            }   
        }
        
        Left_ArtifactGas[index] = artifact;
        // 업데이트 된 아티팩트가 있으면 극에 맞게 적용
        if (artifact != null)
        {
            if (_magneticController.magneticType == MagneticType.N)
            {
                Left_ArtifactGas[index].N_ApplyTo(abilitySystem);
            }
            else
            {
                Left_ArtifactGas[index].S_ApplyTo(abilitySystem);
            }
        }
    }

    public void SetRightArtifact(int index, ArtifactDataSO artifact)
    {
        // 현재 위치의 아티팩트가 위치하면 극에 맞게 기존 특성 지우기
        if (Right_ArtifactGas[index] != null)
        {
            if (_magneticController.magneticType == MagneticType.N)
            {
                Right_ArtifactGas[index].S_RemoveTo(abilitySystem);
            }
            else
            {
                Right_ArtifactGas[index].N_RemoveTo(abilitySystem);
            }   
        }
        Right_ArtifactGas[index] = artifact;
        // 업데이트 된 아티팩트가 있으면 극에 맞게 적용
        if (artifact != null)
        {
            if (_magneticController.magneticType == MagneticType.N)
            {
                Right_ArtifactGas[index].S_ApplyTo(abilitySystem);
            }
            else
            {
                Right_ArtifactGas[index].N_ApplyTo(abilitySystem);
            }   
        }
    }

    
    // 극성 변환시 전체 아티팩트의 효과 업데이트
    public void ConvertArtifact()
    {
        if(_magneticController.magneticType == MagneticType.S)
        {
            N_RemoveAll(Left_ArtifactGas);
            S_RemoveAll(Right_ArtifactGas);
            N_ApplyAll(Right_ArtifactGas);
            S_ApplyAll(Left_ArtifactGas);
        }
        else
        {
            N_RemoveAll(Right_ArtifactGas);
            S_RemoveAll(Left_ArtifactGas);
            N_ApplyAll(Left_ArtifactGas);
            S_ApplyAll(Right_ArtifactGas);
        }
    }
    
}
