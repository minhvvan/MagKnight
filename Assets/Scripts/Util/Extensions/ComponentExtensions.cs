using UnityEngine;

public static class ComponentExtensions
{
    public static T GetInterfaceInParent<T>(this Component component) where T : class
    {
        MonoBehaviour[] components = component.GetComponentsInParent<MonoBehaviour>();
        foreach (MonoBehaviour mb in components)
        {
            if (mb is T interfaceInstance)
            {
                return interfaceInstance;
            }
        }

        return null;
    }
}