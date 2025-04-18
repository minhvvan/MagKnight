using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;

public class Projectile : MonoBehaviour, IObserver<GameObject>
{
    private EnemyBlackboard _enemyBlackboard;
    
    private CancellationTokenSource _cancellation;
    
    public EnemyHitDetector HitHandler { get; private set; } // Melee type enemy만 enemy한테 붙어있음

    public void Initialize(EnemyBlackboard blackboard)
    {
        _enemyBlackboard = blackboard;
        _cancellation = new CancellationTokenSource();
        
        EnemyHitDetector hitHandler;
        if (TryGetComponent<EnemyHitDetector>(out hitHandler))
        {
            HitHandler = hitHandler;
            HitHandler.Subscribe(this);
        }
        
        OnShoot();
    }

    private void OnShoot()
    {
        MoveGradually().Forget();
    }

    private async UniTask MoveGradually()
    {        
        Vector3 targetPos = _enemyBlackboard.target.transform.position;
        Vector3 startPos = transform.position;
        Vector3 dir = Vector3.Normalize(targetPos - startPos);
        while (true)
        {
            transform.position += dir * (Time.deltaTime * _enemyBlackboard.projectileSpeed);
            
            await UniTask.Yield(cancellationToken:_cancellation.Token);
        }
    }

    // void OnTriggerEnter(Collider other)
    // {
    //     if (other.gameObject.layer == LayerMask.NameToLayer("Player") ||
    //         other.gameObject.layer == LayerMask.NameToLayer("Environment"))
    //     {
    //         Destroy(gameObject);
    //     }
    // }

    public void OnDestroy()
    {
        _cancellation.Cancel();
    }

    public void OnNext(GameObject value)
    {
        float damage = -_enemyBlackboard.abilitySystem.GetValue(AttributeType.ATK);
        GameplayEffect damageEffect = new GameplayEffect(EffectType.Static, AttributeType.HP, damage);
        value.GetComponent<CharacterBlackBoardPro>().GetAbilitySystem().ApplyEffect(damageEffect);
        
        Destroy(gameObject);
    }

    public void OnError(Exception error)
    {
        throw new NotImplementedException();
    }

    public void OnCompleted()
    {
        throw new NotImplementedException();
    }
}
