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
            var damage = GameManager.Instance.Player.GetAttackDamage(0.5f);
            _damageEffect.amount = damage;
            var enemyASC = hitInfo.collider.gameObject.GetComponent<Enemy>().blackboard.abilitySystem;
            enemyASC.ApplyEffect(_damageEffect);
            enemyASC.ApplyEffect(_resistanceEffect);
        }
    }
}
