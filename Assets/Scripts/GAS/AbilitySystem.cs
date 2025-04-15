using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AbilitySystem : MonoBehaviour
{
    public AttributeSet Attributes = new AttributeSet();

    public void ApplyEffect(Effect effect)
    {
        effect.Apply(this);
    }
}

