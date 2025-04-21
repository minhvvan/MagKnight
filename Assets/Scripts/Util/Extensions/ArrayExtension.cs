using System;
using UnityEngine;

public static class ArrayExtensions
{
    public static void ForEach<T>(this T[] array, Action<T> action)
    {
        if (array == null) return;
        
        foreach (T item in array)
        {
            action(item);
        }
    }
}