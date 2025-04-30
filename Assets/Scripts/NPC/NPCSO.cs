using System;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
[CreateAssetMenu(fileName = "NPCData", menuName = "SO/NPC/NPCData")]
public class NPCSO : ScriptableObject
{
    public string npcName;
    public string npcDescription;
    public List<DialogueDataSO> dialogueData = new  List<DialogueDataSO>();
    
    //역할을 나는게 좋아보임
    public enum NPCType{
        None,
        Merchant,
        Healer,
        Blacksmith,
    }

    public NPCType npcType;
}