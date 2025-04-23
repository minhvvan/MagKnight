using UnityEngine;
using System.Collections.Generic;


[CreateAssetMenu(menuName = "SO/NPC/Dialogue/DialogueData")]
public class DialogueDataSO : ScriptableObject
{
    public List<DialogueLine> lines;
}

