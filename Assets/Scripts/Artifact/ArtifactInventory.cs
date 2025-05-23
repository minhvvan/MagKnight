using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using hvvan;
using Managers;
using UnityEngine;
using UnityEngine.Serialization;

public class ArtifactInventory : MonoBehaviour
{
    public ArtifactDataSO[] Left_ArtifactGas = new ArtifactDataSO[9];
    public ArtifactDataSO[] Right_ArtifactGas = new ArtifactDataSO[9];

    public AbilitySystem abilitySystem;
    private MagneticController _magneticController;
    
    private ArtifactDataMappingSO _artifactDataMappingSO;
    
    void Start()
    {
        abilitySystem = GameManager.Instance.Player.AbilitySystem;
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
            if(artifact != null)
                artifact.S_ApplyTo(abilitySystem);
        }
    }
    
    private  void N_RemoveAll(ArtifactDataSO[] artifactList)
    {
        foreach (var artifact in artifactList)
        {
            if(artifact != null)
                artifact.N_RemoveTo(abilitySystem);
        }
    }
    
    private  void S_RemoveAll(ArtifactDataSO[] artifactList)
    {
        foreach (var artifact in artifactList)
        {
            if(artifact != null)
                artifact.S_RemoveTo(abilitySystem);
        }
    }
    
    
    public async UniTask SetLeftArtifact(int index, ArtifactDataSO artifact, bool isSave = true)
    {
        // 바뀐 아티팩트가 기존에 있던게 아니라면
        if (artifact != null && !Left_ArtifactGas.Contains(artifact))
        {
            if (_magneticController.magneticType == MagneticType.N)
            {
                artifact.N_ApplyTo(abilitySystem);
            }
            else
            {
                artifact.S_ApplyTo(abilitySystem);
            }
        }
        
        await UniTask.DelayFrame(1);
        
        var currentArtifact = Left_ArtifactGas[index];
        Left_ArtifactGas[index] = artifact;
        
        await UniTask.DelayFrame(1);
        
        if (currentArtifact != null && !Left_ArtifactGas.Contains(currentArtifact))
        {
            if (_magneticController.magneticType == MagneticType.N)
            {
                currentArtifact.N_RemoveTo(abilitySystem);
            }
            else
            {
                currentArtifact.S_RemoveTo(abilitySystem);
            }
        }
        
        var currentRunData = GameManager.Instance.CurrentRunData;
        if (artifact)
        {
            currentRunData.leftArtifacts[index] = artifact.itemID;
            if(!currentRunData.artifactsId.Contains(artifact.itemID))
            {
                currentRunData.artifactsId.Add(artifact.itemID);
                ItemManager.Instance.RemoveArtifactList(artifact);
            }
        }
        else
        {
            currentRunData.leftArtifacts.Remove(index);
        }
        
        //데이터 저장
        if (isSave) await GameManager.Instance.SaveData(Constants.CurrentRun);
    }

    public async UniTask SetRightArtifact(int index, ArtifactDataSO artifact, bool isSave = true)
    {
        if (artifact != null && !Right_ArtifactGas.Contains(artifact))
        {
            if (_magneticController.magneticType == MagneticType.N)
            {
                artifact.S_ApplyTo(abilitySystem);
            }
            else
            {
                artifact.N_ApplyTo(abilitySystem);
            }
        }
        
        await UniTask.DelayFrame(1);
        
        var currentArtifact = Right_ArtifactGas[index];
        Right_ArtifactGas[index] = artifact;
        
        await UniTask.DelayFrame(1);
        
        if (currentArtifact != null && !Right_ArtifactGas.Contains(currentArtifact))
        {
            if (_magneticController.magneticType == MagneticType.N)
            {
                currentArtifact.S_RemoveTo(abilitySystem);
            }
            else
            {
                currentArtifact.N_RemoveTo(abilitySystem);
            }   
        }
        
        var currentRunData = GameManager.Instance.CurrentRunData;
        if (artifact)
        {
            currentRunData.rightArtifacts[index] = artifact.itemID;
            if(!currentRunData.artifactsId.Contains(artifact.itemID))
            {
                currentRunData.artifactsId.Add(artifact.itemID);
                ItemManager.Instance.RemoveArtifactList(artifact);
            }
        }
        else
        {
            currentRunData.rightArtifacts.Remove(index);
        }
        
        //데이터 저장
        if (isSave) await GameManager.Instance.SaveData(Constants.CurrentRun);
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

    public async UniTaskVoid SetLeftArtifact(int index, int artifactID)
    {
        if (!_artifactDataMappingSO)
        {
            _artifactDataMappingSO = await DataManager.Instance.LoadScriptableObjectAsync<ArtifactDataMappingSO>(Addresses.Data.Artifact.ArtifactMappingData);
        }

        SetLeftArtifact(index, _artifactDataMappingSO.artifacts[artifactID], false).Forget();
    }
    
    public async UniTaskVoid SetRightArtifact(int index, int artifactID)
    {
        if (!_artifactDataMappingSO)
        {
            _artifactDataMappingSO = await DataManager.Instance.LoadScriptableObjectAsync<ArtifactDataMappingSO>(Addresses.Data.Artifact.ArtifactMappingData);
        }

        SetRightArtifact(index, _artifactDataMappingSO.artifacts[artifactID], false).Forget();
    }
}
