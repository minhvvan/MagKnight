using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

public class Boss : Enemy
{
    public BossBlackboard bossBlackboard => (BossBlackboard)blackboard;
    
    public PatternController patternController;
    
    private GameObject _bomb;
    private CancellationTokenSource buffCancellationToken;
    
    public override void OnPhaseChange(int phase)
    {
        if (bossBlackboard.aiType == EnemyAIType.Boss)
        {
            Anim.SetTrigger("PhaseChange");
            bossBlackboard.phase = phase;
            patternController.PhaseChange(phase);
            if (phase == 2)
            {
                // blackboard.abilitySystem.
                GameplayEffect gameplayEffect = new GameplayEffect(EffectType.Instant, AttributeType.MoveSpeed, 2);
                bossBlackboard.abilitySystem.ApplyEffect(gameplayEffect);
                Anim.SetFloat("phase", phase);
                bossBlackboard.enemyRenderer.material.SetColor("_EmissiveTint", Color.red);
                Anim.SetFloat("AttackSpeed", 1.5f);
            }
            else if (phase == 3)
            {
                GameplayEffect gameplayEffect = new GameplayEffect(EffectType.Instant, AttributeType.MoveSpeed, 3);
                bossBlackboard.abilitySystem.ApplyEffect(gameplayEffect);
                Anim.SetFloat("phase", phase);
                blackboard.enemyRenderer.material.SetColor("_EmissiveTint", Color.red);
                blackboard.enemyRenderer.material.SetFloat("_Intensity", 1);
                Anim.SetFloat("AttackSpeed", 2f);
            }
        }
    }

    public void PatternAttackStart(int patternIndex)
    {
        patternController.AttackStart(patternIndex);
        if (patternIndex == 0)
        {
            // Quaternion rotation = Quaternion.LookRotation(casterPos - targetPos);
            VFXManager.Instance.TriggerVFX(VFXType.DASH_TRAIL_BLUE, bossBlackboard.rightHandTransform.position, Quaternion.identity, size: new Vector3(3,3,3));
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
        Vector3 pos = transform.position;
        pos.y = 0;
        transform.position = pos;
        bombAgent.nextPosition = pos;
    }

    public void MagneticBarrier()
    {
        SwitchMagneticType();
        if (magneticType == MagneticType.N)
        {
            bossBlackboard.abilitySystem.SetTag("BarrierN");
            // VFXManager.Instance.TriggerVFX(VFXType.MAGNET_SHIELD, transform.position)
            CheckBuffEndTime("BarrierN");
            Debug.Log("N activated");
        }
        else if (magneticType == MagneticType.S)
        {
            bossBlackboard.abilitySystem.SetTag("BarrierS");
            CheckBuffEndTime("BarrierS");
            Debug.Log("S activated");
        }
    }

    private async UniTask CheckBuffEndTime(string tag)
    {
        buffCancellationToken = new CancellationTokenSource();
        float remainingTime = 20f;

        try
        {
            while (remainingTime > 0f)
            {
                await UniTask.Yield(buffCancellationToken.Token);
                remainingTime -= Time.deltaTime;
            }
            bossBlackboard.abilitySystem.DeleteTag(tag);
        }
        catch(OperationCanceledException){}
    }

    public void MagneticPulling()
    {
        MagneticController targetMC = bossBlackboard.target.GetComponent<MagneticController>();
        IMagneticInteractCommand magnetPullAction = MagneticInteractFactory.GetInteract<MagnetDashAttackAction>();
        magnetPullAction.Execute(this, targetMC);
    }
}
