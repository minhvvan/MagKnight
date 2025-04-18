using System;

public static class SceneTransitionEvent
{
    public static event Action<string, bool> OnSceneTransitionComplete;

    public static void TriggerSceneTransitionComplete(string sceneName, bool isSetNewName)
    {
        OnSceneTransitionComplete?.Invoke(sceneName, isSetNewName);
    }
}