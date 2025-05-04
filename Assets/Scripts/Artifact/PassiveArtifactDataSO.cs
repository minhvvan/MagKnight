using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "SO/Artifact/PassiveArtifactDataSO")]
public class PassiveArtifactDataSO : ArtifactDataSO
{
    public List<PassiveEffectData> N_passiveArtifacts = new List<PassiveEffectData>();
    public List<PassiveEffectData> S_passiveArtifacts = new List<PassiveEffectData>();
    
    public override void N_ApplyTo(AbilitySystem target)
    {
        foreach (var instance in N_passiveArtifacts)
        {
            target.RegisterPassiveEffect(instance);
        }
    }
   
    public override void S_ApplyTo(AbilitySystem target)
    {
        foreach (var instance in S_passiveArtifacts)
        {
            target.RegisterPassiveEffect(instance);
        }
    }

    public override void N_RemoveTo(AbilitySystem target)
    {
        foreach (var instance in N_passiveArtifacts)
        {
            target.RemovePassiveEffect(instance);
        }
    }
    public override void S_RemoveTo(AbilitySystem target)
    {
        foreach (var instance in S_passiveArtifacts)
        {
            target.RemovePassiveEffect(instance);
        }
    }
}
