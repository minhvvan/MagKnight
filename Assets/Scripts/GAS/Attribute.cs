using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public enum AttributeType
{
    MaxHP,
    HP,
    STR,
    INT,
    DEF,
    LUK,
    CRT,
    DMG,
    SPD,
    BAS
}

[System.Serializable]
public class Attribute
{
    public float Value;
    public Action OnModified;

    public void Modify(float amount)
    {
        Value += amount;
        OnModified?.Invoke();
    }

    public void Set(float value)
    {
        Value = value;
        OnModified?.Invoke();
    }
    
}


