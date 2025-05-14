using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Threading;
using Cysharp.Threading.Tasks;
using hvvan;
using Jun;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

public class Boss : Enemy
{
    public BossBlackboard bossBlackboard => (BossBlackboard)blackboard;
    
    public PatternController patternController;
    
    private GameObject _bomb;
    private CancellationTokenSource _buffCancellationToken;
    private CancellationTokenSource _missileCancellationToken;
    
    public override void OnPhaseChange(int phase)
    {
        if (bossBlackboard.aiType == EnemyAIType.Boss)
        {
            SetAnimTrigger("PhaseChange");
            bossBlackboard.phase = phase;
            patternController.PhaseChange(phase);
            if (phase == 2)
            {
                GameplayEffect gameplayEffect = new GameplayEffect(EffectType.Instant, AttributeType.MoveSpeed, 2);
                bossBlackboard.abilitySystem.ApplyEffect(gameplayEffect);
                SetAnimFloat("phase", phase);
                bossBlackboard.enemyRenderer.material.SetColor("_EmissiveTint", Color.red);
                SetAnimFloat("AttackSpeed", 1.5f);
            }
            else if (phase == 3)
            {
                GameplayEffect gameplayEffect = new GameplayEffect(EffectType.Instant, AttributeType.MoveSpeed, 3);
                bossBlackboard.abilitySystem.ApplyEffect(gameplayEffect);
                SetAnimFloat("phase", phase);
                blackboard.enemyRenderer.material.SetColor("_EmissiveTint", Color.red);
                blackboard.enemyRenderer.material.SetFloat("_Intensity", 1);
                SetAnimFloat("AttackSpeed", 2f);
            }
            
            AudioManager.Instance.PlaySFX(AudioBase.SFX.Boss.Growl.Growl1);
        }
    }

    public override void OnNext(HitInfo hitInfo)
    {
        GiveDamageEffect(hitInfo);
    }
    
    public override void GiveDamageEffect(HitInfo hitInfo)
    {
        float damage = blackboard.abilitySystem.GetValue(AttributeType.Strength) * patternController.GetCuurrentPatternDamage();
        float impulse = 30 * patternController.GetCurrentPatternImpulse();
        GameplayEffect damageEffect = new GameplayEffect(EffectType.Instant, AttributeType.Damage, damage);
        GameplayEffect impulseEffect = new GameplayEffect(EffectType.Instant, AttributeType.Impulse, impulse);
        damageEffect.extraData.sourceTransform = transform;
        impulseEffect.extraData.sourceTransform = transform;

        if (hitInfo.collider.gameObject.TryGetComponent(out AbilitySystem abilitySystem))
        {
            abilitySystem.ApplyEffect(damageEffect);
            abilitySystem.ApplyEffect(impulseEffect);
        }
    }
    
    #region patternAnimationEvent
    public void PatternAttackStart(int patternIndex)
    {
        patternController.AttackStart(patternIndex);

        switch (patternIndex)
        {
            case 0:
                AudioManager.Instance.PlaySFX(AudioBase.SFX.Boss.Attack.Impact);
                break;
            case 1:
                AudioManager.Instance.PlaySFX(AudioBase.SFX.Boss.Boom.Impact);
                break;
        }
    }

    public void PatternAttackEnd(int patternIndex)
    {
        patternController.AttackEnd(patternIndex);
    }
    
    public void SpawnBomb(GameObject prefab)
    {
        _bomb = Instantiate(prefab, bossBlackboard.leftHandTransform.position, bossBlackboard.leftHandTransform.rotation, bossBlackboard.leftHandTransform);
    }

    public void ActivateBomb()
    {
        _bomb.transform.SetParent(null);
        NavMeshAgent bombAgent = _bomb.GetComponent<NavMeshAgent>();
        bombAgent.enabled = true;
        _bomb.GetComponent<Enemy>().enabled = true;
        Vector3 pos = _bomb.transform.position;
        pos.y = 0.05f;
        _bomb.transform.position = pos;
        bombAgent.nextPosition = pos;
    }

    public void MagneticBarrier()
    {
        SwitchMagneticType();
        Vector3 barrierPos = new Vector3(0,1.8f,0);
        if (magneticType == MagneticType.N)
        {
            bossBlackboard.abilitySystem.SetTag("BarrierN");
            GameObject vfxObject = VFXManager.Instance.TriggerVFX(VFXType.MAGNETIC_SHIELD_N, transform, barrierPos, Quaternion.identity, returnAutomatically:false);
            CheckBuffEndTime("BarrierN", VFXType.MAGNETIC_SHIELD_N, vfxObject).Forget();
        }
        else if (magneticType == MagneticType.S)
        {
            bossBlackboard.abilitySystem.SetTag("BarrierS");
            GameObject vfxObject = VFXManager.Instance.TriggerVFX(VFXType.MAGNETIC_SHIELD_S, transform, barrierPos, Quaternion.identity, returnAutomatically:false);
            CheckBuffEndTime("BarrierS", VFXType.MAGNETIC_SHIELD_S, vfxObject).Forget();
        }
        
        AudioManager.Instance.PlaySFX(AudioBase.SFX.Boss.Shield.Shield1);
    }

    private async UniTask CheckBuffEndTime(string tag, VFXType type, GameObject vfxObject)
    {
        _buffCancellationToken = new CancellationTokenSource();
        float remainingTime = 10f;

        try
        {
            while (remainingTime > 0f)
            {
                await UniTask.Yield(_buffCancellationToken.Token);
                remainingTime -= Time.deltaTime;
            }
            VFXManager.Instance.ReturnVFX(type, vfxObject);
            bossBlackboard.abilitySystem.DeleteTag(tag);
        }
        catch(OperationCanceledException){}
    }

    public void MagneticPulling()
    {
        VFXManager.Instance.TriggerVFX(VFXType.MAGNETIC_PULLING, transform.position, Quaternion.identity, returnAutomatically:true);
        if (GameManager.Instance.Player.AbilitySystem.HasTag("SuperArmor") ||
            GameManager.Instance.Player.AbilitySystem.HasTag("Invincibility"))
        {
            return;
        }
        MagneticController targetMC = bossBlackboard.target.GetComponent<MagneticController>();
        gameObject.GetComponent<EnemyMagnetActionController>().StartMagneticPull(targetMC);
        SetAnimTrigger("PullSucceed");
        
        AudioManager.Instance.PlaySFX(AudioBase.SFX.Player.Magnetic.MagneticForce);
    }

    public void Missile(GameObject missileEffect)
    {
        float duration = 1.5f;
        
        AudioManager.Instance.PlaySFX(AudioBase.SFX.Enemy.Attack.ShortVulcan.BurstShot);
        
        _missileCancellationToken = new CancellationTokenSource();
        GameObject effect = Instantiate(missileEffect, blackboard.target.transform.position, Quaternion.identity);
        AttackEffect attackEffect = effect.GetComponent<AttackEffect>();
        float damage = blackboard.abilitySystem.GetValue(AttributeType.Strength) *
                       patternController.GetCuurrentPatternDamage();
        float impulse = 30 * patternController.GetCurrentPatternImpulse();
        attackEffect.SetAttackDamageImpulse(damage, impulse);
        
        ParticleSystem warningParticleSystem = effect.GetComponent<ParticleSystem>();
        var main = warningParticleSystem.main;
        main.startLifetime = 6f;

        
        MissileHelper(effect, duration).Forget();
    }

    public async UniTask MissileHelper(GameObject effect, float duration)
    {
        GameObject fallingMissileEffect = effect.transform.GetChild(0).gameObject;
        try
        {
            while (duration > 0)
            {
                await UniTask.Yield(_missileCancellationToken.Token);
                duration -= Time.deltaTime;
            }

            fallingMissileEffect.SetActive(true);
        }
        catch (OperationCanceledException){ }
    }
    #endregion

    void OnDisable()
    {
        UIManager.Instance.inGameUIController.UnbindBossAttributeChanges();
    }

    protected void OnDestroy()
    {
        base.OnDestroy();
        _buffCancellationToken?.Cancel();
        _buffCancellationToken?.Dispose();
        _missileCancellationToken?.Cancel();
        _missileCancellationToken?.Dispose();
    }
}
