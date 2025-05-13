using System;
using System.Collections.Generic;
using UnityEngine;

public static class ListExtensions
{
    public static bool AddUnique<T>(this List<T> list, T newItem)
    {
        if (list == null) return false;
        if(list.Contains(newItem)) return false;
        
        list.Add(newItem);
        return true;
    }
}