using System.Collections;
using System.Collections.Generic;
using AYellowpaper.SerializedCollections;
using hvvan;
using UnityEngine;

public class FieldSkill : Skill
{
    new void Start()
    {
        base.Start();
      
        Destroy(gameObject, 5f);
    }
    
    public override void OnNext(HitInfo hitInfo)
    {
        if (hitInfo.collider.gameObject.layer == LayerMask.NameToLayer("Enemy"))
        {
            GameplayEffect damageEffect = _damageEffect.DeepCopy();
            GameplayEffect resistanceEffect = _resistanceEffect.DeepCopy();
            
            (damageEffect.amount, damageEffect.extraData.isCritical) = GameManager.Instance.Player.GetAttackDamage(0.5f);
            damageEffect.extraData.hitInfo = hitInfo;
            
            var enemyASC = hitInfo.collider.gameObject.GetComponent<Enemy>().blackboard.abilitySystem;
            enemyASC.ApplyEffect(damageEffect);
            enemyASC.ApplyEffect(resistanceEffect);
        }
    }
}
