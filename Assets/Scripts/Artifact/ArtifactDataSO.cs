using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public abstract class ArtifactDataSO : ScriptableObject
{
   public Sprite icon;
   public string itemName;
   
   public virtual void N_ApplyTo(AbilitySystem target) {}
   
   public virtual void S_ApplyTo(AbilitySystem target) {}

   public virtual void N_RemoveTo(AbilitySystem target) {}
   public virtual void S_RemoveTo(AbilitySystem target) {}
   
}
