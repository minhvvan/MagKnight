using System;
using System.Collections;
using System.Collections.Generic;
using hvvan;
using UnityEngine;

public class TornadoSkill : Skill
{
    [SerializeField] private float moveSpeed;

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
            var damage = GameManager.Instance.Player.GetAttackDamage(3f);
            _damageEffect.amount = damage;
            var enemyASC = hitInfo.collider.gameObject.GetComponent<Enemy>().blackboard.abilitySystem;
            enemyASC.ApplyEffect(_damageEffect);
            enemyASC.ApplyEffect(_resistanceEffect);
        }
    }
}
