using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Artifact/ArtifactDataSO")]
public class ArtifactDataSO : ScriptableObject
{
   public Sprite icon;
   public string name;
   
   public List<GameplayEffect> N_ArtifactEffect;
   public List<GameplayEffect> S_ArtifactEffect;
   
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

   public void N_RemoveTo(AbilitySystem target)
   {
      foreach (var instance in N_ArtifactEffect)
      {
         target.RemoveEffect(instance);
      }
   }
   public void S_RemoveTo(AbilitySystem target)
   {
      foreach (var instance in S_ArtifactEffect)
      {
         target.RemoveEffect(instance);
      }
   }
   
}
