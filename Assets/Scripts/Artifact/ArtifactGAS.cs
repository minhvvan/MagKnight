using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Artifact/Artifact GAS")]
public class ArtifactGAS : ScriptableObject
{
   public List<Effect> N_ArtifactEffect;
   public List<Effect> S_ArtifactEffect;
   
   
   public void N_ApplyTo(AbilitySystem target)
   {
      foreach (var instance in N_ArtifactEffect)
      {
         target.ApplyEffect(instance);
      }
   }
   
   public void S_ApplyTo(AbilitySystem target)
   {
      foreach (var instance in S_ArtifactEffect)
      {
         target.ApplyEffect(instance);
      }
   }
}
