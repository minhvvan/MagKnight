using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class ArtifactButtonTest : MonoBehaviour
{
    public AbilitySystem abilitySystem;
    [FormerlySerializedAs("artifactGas")] public ArtifactDataSO artifactDataSo;

    public void ApplyN()
    {
        artifactDataSo.N_ApplyTo(abilitySystem);
    }

    public void ApplyS()
    {
        artifactDataSo.S_ApplyTo(abilitySystem);
    }
}
