using UnityEngine;

public static class GameObjectExtensions
{
    public static void Destroy(this GameObject gameObject)
    {
        if (gameObject != null)
        {
            Object.Destroy(gameObject);
        }
    }
}