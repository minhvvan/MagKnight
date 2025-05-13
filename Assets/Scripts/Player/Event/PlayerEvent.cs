using System;

public static class PlayerEvent
{
    // public static event Action<string, bool> OnSceneTransitionComplete;

    // public static void TriggerSceneTransitionComplete(string sceneName, bool isSetNewName)
    // {
    //     OnSceneTransitionComplete?.Invoke(sceneName, isSetNewName);
    // }
    

    //극성변환 시 발생
    public static event Action<MagneticType> OnPolarityChange;

    public static void TriggerPolarityChange(MagneticType magneticType)
    {
        OnPolarityChange?.Invoke(magneticType);
    }
}