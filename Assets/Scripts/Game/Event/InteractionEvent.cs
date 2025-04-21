using System;

public static class InteractionEvent
{
    public static event Action OnDialogueStart;
    public static event Action OnDialogueEnd;

    public static void DialogueStart()
    {
        OnDialogueStart?.Invoke();
    }

    public static void DialogueEnd()
    {
        OnDialogueEnd?.Invoke();
    }
}