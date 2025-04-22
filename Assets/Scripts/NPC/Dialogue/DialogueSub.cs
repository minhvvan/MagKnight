using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class DialogueLine
{
    public string speakerName;
    [TextArea] public string text;

    public List<DialogueOption> options; // 선택지
}

[System.Serializable]
public class DialogueOption
{
    public string text; // 버튼에 표시될 문장
    public int nextLineIndex;
}

