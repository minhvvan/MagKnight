using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class ArtifactButtonTest : MonoBehaviour
{
    public AbilitySystem abilitySystem;
    public ArtifactDataSO artifactDataSo;
    
    public GameplayEffect damageEffect;
    
    public void GetDamage()
    {
        abilitySystem.ApplyEffect(damageEffect);
    }
    
    public void ApplyN()
    {
        artifactDataSo.N_ApplyTo(abilitySystem);
    }

    public void ApplyS()
    {
        artifactDataSo.S_ApplyTo(abilitySystem);
    }
}
