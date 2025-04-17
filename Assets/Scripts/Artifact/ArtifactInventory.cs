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
    
    public void N_Apply(ArtifactDataSO[] artifactList)
    {
        foreach (var artifact in artifactList)
        {
            if(artifact != null)
                artifact.N_ApplyTo(abilitySystem);
        }
    }

    public void S_Apply(ArtifactDataSO[] artifactList)
    {
        foreach (var artifact in artifactList)
        {
            if (artifact != null)
                artifact.S_ApplyTo(abilitySystem);
        }
    }
    
    public void N_Remove(ArtifactDataSO[] artifactList)
    {
        foreach (var artifact in artifactList)
        {
            if (artifact != null)
                artifact.N_RemoveTo(abilitySystem);
        }
    }
    
    public void S_Remove(ArtifactDataSO[] artifactList)
    {
        foreach (var artifact in artifactList)
        {
            if (artifact != null)
                artifact.S_RemoveTo(abilitySystem);
        }
    }

    public void SetLeftArtifact(ArtifactDataSO artifact, int index)
    {
        Left_ArtifactGas[index] = artifact;
        if (_magneticController.magneticType == MagneticType.N)
        {
            artifact.N_ApplyTo(abilitySystem);
        }
        else
        {
            artifact.S_ApplyTo(abilitySystem);
        }
    }

    public void SetRightArtifact(ArtifactDataSO artifact, int index)
    {
        Right_ArtifactGas[index] = artifact;
        if (_magneticController.magneticType == MagneticType.N)
        {
            artifact.S_ApplyTo(abilitySystem);
        }
        else
        {
            artifact.N_ApplyTo(abilitySystem);
        }
    }

    
    public void ConvertArtifact()
    {
        if(_magneticController.magneticType == MagneticType.S)
        {
            N_Remove(Left_ArtifactGas);
            S_Remove(Right_ArtifactGas);
            N_Apply(Right_ArtifactGas);
            S_Apply(Left_ArtifactGas);
        }
        else
        {
            N_Remove(Right_ArtifactGas);
            S_Remove(Left_ArtifactGas);
            N_Apply(Left_ArtifactGas);
            S_Apply(Right_ArtifactGas);
        }
    }
    
}
