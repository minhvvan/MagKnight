using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public enum EffectType
{
    Static,
    Buff,
    Debuff
}

[System.Serializable]
public class GameplayEffect
{
    public EffectType effectType;
    public AttributeType attributeType;
    public float value;
    public float duration;

    public GameplayEffect(EffectType effectType, AttributeType attributeType, float value, float duration = 0f)
    {
        this.effectType = effectType;
        this.attributeType = attributeType;
        this.value = value;
        this.duration = duration;
    }

    public virtual void Apply(AbilitySystem system)
    {
        system.Attributes.Modify(attributeType, value);
        switch (effectType)
        {
            case EffectType.Static:
                break;
            case EffectType.Buff:
                system.StartCoroutine(RemoveAfterDuration(system));
                break;
            case EffectType.Debuff:
                system.StartCoroutine(RemoveAfterDuration(system));
                break;
        }
    }

    public virtual void Remove(AbilitySystem system)
    {
        system.Attributes.Modify(attributeType, -value);
    }
    
    private IEnumerator RemoveAfterDuration(AbilitySystem system)
    {
        yield return new WaitForSeconds(duration);
        system.Attributes.Modify(attributeType, -value);
    }
}
