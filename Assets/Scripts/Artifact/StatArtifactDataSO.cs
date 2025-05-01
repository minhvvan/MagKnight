using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "SO/Artifact/StatArtifactDataSO")]
public class StatArtifactDataSO : ArtifactDataSO
{
    [SerializeField] List<GameplayEffect> N_ArtifactEffect;
    [SerializeField] List<GameplayEffect> S_ArtifactEffect;
   
    public override void N_ApplyTo(AbilitySystem target)
    {
        foreach (var instance in N_ArtifactEffect)
        {
            target.ApplyEffect(instance);
        }
    }
   
    public override void S_ApplyTo(AbilitySystem target)
    {
        foreach (var instance in S_ArtifactEffect)
        {
            target.ApplyEffect(instance);
        }
    }

    public override void N_RemoveTo(AbilitySystem target)
    {
        foreach (var instance in N_ArtifactEffect)
        {
            target.RemoveEffect(instance);
        }
    }
    public override void S_RemoveTo(AbilitySystem target)
    {
        foreach (var instance in S_ArtifactEffect)
        {
            target.RemoveEffect(instance);
        }
    }
}
