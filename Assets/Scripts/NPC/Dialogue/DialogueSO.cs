using UnityEngine;
using System.Collections.Generic;


[CreateAssetMenu(menuName = "SO/NPC/Dialogue/DialogueData")]
public class DialogueDataSO : ScriptableObject
{
    public string npcName;
    public List<DialogueLine> lines;
}