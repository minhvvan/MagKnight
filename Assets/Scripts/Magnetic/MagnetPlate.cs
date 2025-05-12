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
}
