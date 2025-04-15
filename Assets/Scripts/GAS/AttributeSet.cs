using System;
using System.Collections;
using System.Collections.Generic;
using AYellowpaper.SerializedCollections;
using UnityEngine;

[System.Serializable]
public class AttributeSet
{
    public SerializedDictionary<AttributeType, Attribute> attributeDictionary = new SerializedDictionary<AttributeType, Attribute>();

    public void AddAttribute(AttributeType type , float value, Action onModified = null)
    {
        Attribute instance = new Attribute{Value = value, OnModified = onModified};
        attributeDictionary.Add(type, instance);
    }
    public float GetValue(AttributeType type)
    {
        if (attributeDictionary.ContainsKey(type))
            return attributeDictionary[type].Value;
        
        return -404;
    }

    public void Modify(AttributeType type, float value)
    {
        if (attributeDictionary.ContainsKey(type))
            attributeDictionary[type].Modify(value);
    }

    public void Set(AttributeType type, float value)
    {
        if (attributeDictionary.ContainsKey(type))
            attributeDictionary[type].Set(value);
    }
}

