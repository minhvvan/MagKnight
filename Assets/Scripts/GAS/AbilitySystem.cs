using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AbilitySystem : MonoBehaviour
{
    public AttributeSet Attributes = new AttributeSet();

    public void ApplyEffect(GameplayEffect gameplayEffect)
    {
        gameplayEffect.Apply(this);
    }

    public void RemoveEffect(GameplayEffect gameplayEffect)
    {
        gameplayEffect.Remove(this);
    }
}

