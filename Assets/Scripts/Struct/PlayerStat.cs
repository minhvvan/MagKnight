using System;

[Serializable]
public class PlayerStat
{
    public AttributePair MaxHP;
    public AttributePair HP;
    public AttributePair Strength;
    public AttributePair Intelligence;
    public AttributePair CriticalRate;
    public AttributePair Defense;
    public AttributePair CriticalDamage;
    public AttributePair Damage;
    public AttributePair MoveSpeed;
    public AttributePair Impluse;
    public AttributePair ImpulseThreshold;
}

[Serializable]
public struct AttributePair
{
    public AttributeType Key;
    public int Value;
}