using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using hvvan;
using Moon;
using UnityEngine;

public class MagnetPlate : MagneticObject, IObserver<HitInfo>
{
    protected HitDetector HitDetector;
    public Action<HitInfo> OnHit;
    public bool isHold = false;
    
    private AbilitySystem _abilitySystem;
    private GameplayEffect _damageEffect;
    private GameplayEffect _resistanceEffect;
    
    
    protected override void Awake()
    {
        base.Awake();

        InitializeMagnetic();
        
        HitDetector = GetComponent<HitDetector>();
        HitDetector.Subscribe(this);

        OnHit = OnHitAction;
        
        _damageEffect = new GameplayEffect(EffectType.Instant, AttributeType.Damage, 0)
        {
            extraData = new ExtraData(){ sourceTransform = transform }
        };
        _resistanceEffect = new GameplayEffect(EffectType.Instant, AttributeType.ResistanceDamage, 10)
        {
            extraData = new ExtraData(){ sourceTransform = transform }
        };
    }

    private void FixedUpdate()
    {
        CheckBoxCollision();
    }
    
    public override async UniTask OnMagneticInteract(MagneticObject target)
    {
        await magnetPlatePullAction.Execute(this, target);
        if (target.TryGetComponent(out PlayerController player))
        {
            _abilitySystem = player.AbilitySystem;
        }
        isHold = true;
    }
    
    //디텍터 켜고 끄기.
    public void OnHitDetect(bool isOn)
    {
        if(isOn) HitDetector.StartDetection(0);
        else HitDetector.StopDetection();
    }
    
    private void OnHitAction(HitInfo hitInfo)
    {
        if (hitInfo.collider.gameObject.layer == LayerMask.NameToLayer("Enemy"))
        {
            Enemy enemy = hitInfo.collider.gameObject.GetComponent<Enemy>();
            _abilitySystem.TriggerEvent(TriggerEventType.OnHit, enemy.blackboard.abilitySystem);
            _abilitySystem.TriggerEvent(TriggerEventType.OnHit, _abilitySystem);
            
            GameplayEffect damageEffect = _damageEffect.DeepCopy();
            GameplayEffect resistanceEffect = _resistanceEffect.DeepCopy();
            
            (damageEffect.amount, damageEffect.extraData.isCritical) = GameManager.Instance.Player.GetAttackDamage();
            damageEffect.extraData.hitInfo = hitInfo;

            enemy.blackboard.abilitySystem.ApplyEffect(damageEffect);
            enemy.blackboard.abilitySystem.ApplyEffect(resistanceEffect);
        }
    }

    public void OnNext(HitInfo hitInfo)
    {
        OnHit?.Invoke(hitInfo);
    }

    public void OnError(Exception error)
    {
        
    }

    public void OnCompleted()
    {
        
    }
    
    public void CheckBoxCollision(bool isPlayer = false)
    {
        var playerLayer = LayerMask.NameToLayer("Player");
        var magneticLayer = LayerMask.NameToLayer("Magnetic");
        var enemyLayer = LayerMask.NameToLayer("Enemy");
        var groundLayer = LayerMask.NameToLayer("Ground");
        var environmentLayer = LayerMask.NameToLayer("Environment");

        var obj = gameObject;
        var col = obj.GetComponent<BoxCollider>();
        var center = col.bounds.center;
        var size = col.bounds.size;
        var halfExtents = size / 2f;

        var hitColliders = new Collider[20];
        var hitCount = Physics.OverlapBoxNonAlloc(center, halfExtents, hitColliders, Quaternion.identity,
            isPlayer ? playerLayer : 
                (1 << magneticLayer) | (1 << enemyLayer) |
                (1 << groundLayer) | (1 << environmentLayer));

        //감지한 대상 기반으로 보정, 충돌 찾기 수행.
        for (int i = 0; i < hitCount; i++)
        {
            var hitCol = hitColliders[i];

            //본인 제외
            if (hitCol == col) continue;

            //ComputePenetration
            if (Physics.ComputePenetration(col, obj.transform.position, obj.transform.rotation,
                    hitCol, hitCol.transform.position, hitCol.transform.rotation,
                    out Vector3 direction, out float distance))
            {
                //겹침(침투) 보정 //겹친만큼 반대방향으로 이동시킴.
                obj.transform.position += direction * distance;
            }
        }
    }
}
