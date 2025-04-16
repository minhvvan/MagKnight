using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArtifactInventory : MonoBehaviour
{
    public List<ArtifactGAS> N_ArtifactGas = new List<ArtifactGAS>();
    public List<ArtifactGAS> S_ArtifactGas = new List<ArtifactGAS>();

    public AbilitySystem abilitySystem;

    void Start()
    {
        N_Apply();
        S_Apply();
    }
    
    public void N_Apply()
    {
        foreach (var artifact in N_ArtifactGas)
        {
            artifact.N_ApplyTo(abilitySystem);
        }
    }

    public void S_Apply()
    {
        foreach (var artifact in S_ArtifactGas)
        {
            artifact.S_ApplyTo(abilitySystem);
        }
    }
    
    public void N_Remove()
    {
        foreach (var artifact in N_ArtifactGas)
        {
            artifact.N_RemoveTo(abilitySystem);
        }
    }
    
    public void S_Remove()
    {
        foreach (var artifact in S_ArtifactGas)
        {
            artifact.N_RemoveTo(abilitySystem);
        }
    }

    public void TempArtifact()
    {
        (N_ArtifactGas, S_ArtifactGas) = (S_ArtifactGas, N_ArtifactGas);
    }

    public void ConvertArtifact()
    {
        N_Remove();
        S_Remove();
        TempArtifact();
        N_Apply();
        S_Apply();
    }
    
}
