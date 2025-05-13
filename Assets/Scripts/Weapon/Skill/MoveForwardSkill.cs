using System;
using System.Collections;
using System.Collections.Generic;
using hvvan;
using UnityEngine;

public class MoveForwardSkill : Skill
{
    private float moveSpeed = 15;

    new void Start()
    {
        base.Start();
        Destroy(gameObject, 5f);
    }

    void FixedUpdate()
    {
        transform.Translate(Vector3.forward * (moveSpeed * Time.deltaTime));
    }
    
    public override void OnNext(HitInfo hitInfo)
    {
        if (hitInfo.collider.gameObject.layer == LayerMask.NameToLayer("Enemy"))
        {
            GameplayEffect damageEffect = _damageEffect.DeepCopy();
            GameplayEffect resistanceEffect = _resistanceEffect.DeepCopy();
            
            (damageEffect.amount, damageEffect.extraData.isCritical) = GameManager.Instance.Player.GetAttackDamage(3f);
            damageEffect.extraData.hitInfo = hitInfo;

            var enemyASC = hitInfo.collider.gameObject.GetComponent<Enemy>().blackboard.abilitySystem;
            enemyASC.ApplyEffect(damageEffect);
            enemyASC.ApplyEffect(resistanceEffect);
        }
    }
}
