
using System;
using UnityEngine;

public class Katana: BaseWeapon
{
    public override void AttackStart()
    {
        base.AttackStart();
        
        //TODO: FX
    }

    public override void AttackEnd()
    {
        base.AttackEnd();

    }

    public override void OnNext(HitInfo hitInfo)
    {
        base.OnNext(hitInfo);
        
        // float finalDamage = -1f;
        // float resistanceDecrease = -2f;
        // Enemy enemy = hitInfo.hit.collider.gameObject.GetComponent<Enemy>();
        // GameplayEffect damageEffect = new GameplayEffect(EffectType.Instant, AttributeType.HP, finalDamage);
        // GameplayEffect resistanceEffect = new GameplayEffect(EffectType.Instant, AttributeType.RES, resistanceDecrease);
        // enemy.blackboard.abilitySystem.ApplyEffect(damageEffect);
        // enemy.blackboard.abilitySystem.ApplyEffect(resistanceEffect);
    }

    public override void OnError(Exception error)
    {
        Debug.LogError(error);
    }

    public override void OnCompleted()
    {
    }
    
    public override void ChangePolarity()
    {
        //TODO: 극성 스위칭 효과
        // 2초간 대쉬 쿨타임 없음
    }
}
