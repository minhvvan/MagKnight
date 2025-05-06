using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public abstract class ArtifactDataSO : ScriptableObject
{
   [Header("Artifact Info")]
   public Sprite icon;
   public string itemName;
   public string description;
   public int scrapValue;
   public int itemID;
   public ItemRarity rarity;
   
   public virtual void N_ApplyTo(AbilitySystem target) {}
   
   public virtual void S_ApplyTo(AbilitySystem target) {}

   public virtual void N_RemoveTo(AbilitySystem target) {}
   public virtual void S_RemoveTo(AbilitySystem target) {}
   
}
