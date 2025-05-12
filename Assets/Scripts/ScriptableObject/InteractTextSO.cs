using System;
using AYellowpaper.SerializedCollections;
using UnityEngine;

[CreateAssetMenu(menuName = "SO/InteractText", fileName = "InteractTextSO")]
public class InteractTextSO: ScriptableObject
{
    public SerializedDictionary<InteractType, string> text = new SerializedDictionary<InteractType, string>();
}