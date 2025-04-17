using System;
using System.Collections;
using System.Collections.Generic;
using AYellowpaper.SerializedCollections;
using UnityEngine;

[System.Serializable]
public class AttributeSet
{
    // 직접 접근은 피하기
    // 무조건 Ability System 통해서 접근
    
    public SerializedDictionary<AttributeType, Attribute> attributeDictionary = new SerializedDictionary<AttributeType, Attribute>();

    public void AddAttribute(AttributeType type , float value, Action<float> onPreModify = null, Action onPostModify = null)
    {
        Attribute instance = new Attribute{Value = value, OnPreModify = onPreModify, OnPostModify = onPostModify};
        attributeDictionary.Add(type, instance);
    }
    
    public float GetValue(AttributeType type)
    {
        if (attributeDictionary.ContainsKey(type))
            return attributeDictionary[type].Value;
        
        Debug.LogError($"{type} not found");
        return 0;
    }
    
    public void Modify(AttributeType type, float amount)
    {
        if (attributeDictionary.ContainsKey(type))
            attributeDictionary[type].Modify(amount);
    }
    
    public void Set(AttributeType type, float value)
    {
        if (attributeDictionary.ContainsKey(type))
            attributeDictionary[type].Set(value);
    }

    public void AddPreModify(AttributeType type, Action<float> onPreModify)
    {
        if(attributeDictionary.ContainsKey(type))
            attributeDictionary[type].OnPreModify += onPreModify;
        else Debug.LogError($"{type} is not added to this attribute set");
    }
    
    public void AddPostModify(AttributeType type, Action onPostModify)
    {
        if(attributeDictionary.ContainsKey(type))
            attributeDictionary[type].OnPostModify += onPostModify;
        else Debug.LogError($"{type} is not added to this attribute set");
    }
}

