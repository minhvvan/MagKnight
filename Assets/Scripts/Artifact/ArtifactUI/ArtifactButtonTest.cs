using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArtifactButtonTest : MonoBehaviour
{
    public AbilitySystem abilitySystem;
    public ArtifactGAS artifactGas;

    public void ApplyN()
    {
        artifactGas.N_ApplyTo(abilitySystem);
    }

    public void ApplyS()
    {
        artifactGas.S_ApplyTo(abilitySystem);
    }
}
