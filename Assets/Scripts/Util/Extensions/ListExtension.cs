using System;
using System.Collections.Generic;
using UnityEngine;

public static class ListExtensions
{
    public static void AddUnique<T>(this List<T> list, T newItem)
    {
        if (list == null) return;
        if(list.Contains(newItem)) return;
        
        list.Add(newItem);
    }
}