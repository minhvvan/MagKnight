using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class ArtifactInventory : MonoBehaviour
{
    public ArtifactGAS[] Left_ArtifactGas = new ArtifactGAS[15];
    public ArtifactGAS[] Right_ArtifactGas = new ArtifactGAS[15];

    public AbilitySystem abilitySystem;

    private bool isN = true;
    
    void Start()
    {
        abilitySystem = GetComponent<AbilitySystem>();
    }
    
    public void N_Apply(ArtifactGAS[] artifactList)
    {
        foreach (var artifact in artifactList)
        {
            if(artifact != null)
                artifact.N_ApplyTo(abilitySystem);
        }
    }

    public void S_Apply(ArtifactGAS[] artifactList)
    {
        foreach (var artifact in artifactList)
        {
            if (artifact != null)
                artifact.S_ApplyTo(abilitySystem);
        }
    }
    
    public void N_Remove(ArtifactGAS[] artifactList)
    {
        foreach (var artifact in artifactList)
        {
            if (artifact != null)
                artifact.N_RemoveTo(abilitySystem);
        }
    }
    
    public void S_Remove(ArtifactGAS[] artifactList)
    {
        foreach (var artifact in artifactList)
        {
            if (artifact != null)
                artifact.S_RemoveTo(abilitySystem);
        }
    }

    public void ConvertArtifact()
    {
        if(isN)
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
        isN = !isN;
    }
    
}
