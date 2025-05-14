using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Cinemachine;
using Cysharp.Threading.Tasks;
using hvvan;
using Moon;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UIElements;
using Random = UnityEngine.Random;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(AbilitySystem))]
[RequireComponent(typeof(EnemyBlackboard))]
public class Enemy : MagneticObject, IObserver<HitInfo>
{
    // components
    public Animator Anim { get; private set; }
    public NavMeshAgent Agent { get; private set; }
    public Collider MainCollider { get; private set; }
    public Rigidbody Rb { get; private set; }
    public AbilitySystem EnemyAbilitySystem { get; private set; }
    public HitDetector HitHandler { get; private set; } // Melee type enemy만 enemy한테 붙어있음
    public EnemyBlackboard blackboard;

    public HpBarController hpBarController;

    private AnimatorStateInfo _currentAnimStateInfo;

    public Action<Enemy> OnDead;
    
    private RaycastHit[] _hits = new RaycastHit[1];
    private Collider[] _colliders = new Collider[1];
    private RaycastHit[] _hitsBelow = new RaycastHit[5];

    public bool isDashing;
    public bool isFalling;

    private float _gravity = 9.8f;
    private float _maxFallSpeed = 10f;
    private float _verticalSpeed = 0f;
    private float _yOffset = 0.05f;
    
    // stateMachine
    private StateMachine _stateMachine;
    
    [NonSerialized] public EnemyStateSpawn spawnState;
    [NonSerialized] public EnemyStateAI aiState;
    [NonSerialized] public EnemyStateAction actionState;
    [NonSerialized] public EnemyStateStagger staggerState;
    [NonSerialized] public EnemyStateDead deadState;
    
    //enemy scrap value
    [NonSerialized] public int scrapMinValue = 10;
    [NonSerialized] public int scrapMaxValue = 40;

    void Awake()
    {
        base.Awake();
        
        Initialize();
        InitializeMagnetic();
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        _stateMachine.ChangeState(spawnState);
        
        // 시작 y축 위치 보정
        if (Physics.Raycast(transform.position + Vector3.up * 2f, Vector3.down, out RaycastHit hit, 4f))
        {
            Vector3 newPos = hit.point;
            newPos.y += _yOffset;
            Agent.Warp(newPos);
        }
    }
    
    private void Initialize()
    {
        Anim = GetComponent<Animator>();
        Agent = GetComponent<NavMeshAgent>();
        MainCollider = GetComponent<Collider>();
        Rb = GetComponent<Rigidbody>();
        EnemyAbilitySystem = GetComponent<AbilitySystem>();
        Effector = GetComponent<Effector>();

        Agent.updatePosition = false;
        Agent.updateRotation = false;
        
        // hitbox Controller
        HitDetector hitHandler;
        if (TryGetComponent<HitDetector>(out hitHandler))
        {
            HitHandler = hitHandler;
            HitHandler.Subscribe(this);
        }
        
        InitializeState();
    }

    private void InitializeState()
    {
        _stateMachine = new StateMachine();

        spawnState = new EnemyStateSpawn(this);
        aiState = new EnemyStateAI(this);
        actionState = new EnemyStateAction(this);
        staggerState = new EnemyStateStagger(this);
        deadState = new EnemyStateDead(this);
    }

    void Update()
    {
        _stateMachine.Update();
    }

    public void SetState(BaseState<Enemy> newState)
    {
        _stateMachine.ChangeState(newState);
    }
    
    public bool IsCurrentAnimFinished(string animName)
    {
        AnimatorStateInfo info = Anim.GetCurrentAnimatorStateInfo(0);
        return info.IsName(animName) && info.normalizedTime >= 1f;
    }

    private void FixedUpdate()
    { 
        _currentAnimStateInfo = Anim.GetCurrentAnimatorStateInfo(0);
        var enemies = Physics.OverlapSphere(transform.position, 0.5f, 1 << LayerMask.NameToLayer("Enemy"));

        ApplySoftCollision(enemies);

        int a = Physics.SphereCastNonAlloc(transform.position + Vector3.up * 1f, 1f, Vector3.down, _hitsBelow, 1f, 
            LayerMask.GetMask("Environment")); // 주변에 environment 감지 안될 시 중력 적용
        if (a <= 0)
        {
            Agent.ResetPath();
            isFalling = true;
            ApplyGravity();
        }
    }
    
    private void OnAnimatorMove()
    {
        if (isFalling)
        {
            return;
        }
        
        if (_currentAnimStateInfo.IsName("Trace"))
        {   
            Vector3 velocity = Agent.desiredVelocity.normalized * Time.deltaTime * 
                               blackboard.abilitySystem.GetValue(AttributeType.MoveSpeed);
            transform.position += velocity;
            Agent.nextPosition = transform.position;
        }

        if (isDashing) return;
        Vector3 dir = Agent.desiredVelocity;
        if (dir.sqrMagnitude > 0.1f)
        {
            Quaternion targetRot = Quaternion.LookRotation(dir);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * 10f);
        }
    }

    public void OnDeath()
    {
        if(blackboard.isDead) return;

        blackboard.isDead = true;
        SetState(deadState);
        OnDead?.Invoke(this);
        
        if (blackboard.aiType == EnemyAIType.Boss)
        {
            AudioManager.Instance.PlaySFX(AudioBase.SFX.Enemy.Dead.PowerDown1);
        }
        else
        {
            AudioManager.Instance.PlaySFX(AudioBase.SFX.Enemy.Dead.SignalLost);
        }
        
        //죽으면 일정 확률로 힐팩 생성
        if(ItemManager.Instance.CheckProbability(ItemCategory.HealthPack, ItemRarity.Common))
        {
            ItemManager.Instance.CreateItem(ItemCategory.HealthPack, ItemRarity.Common, 
                MainCollider.bounds.center + Vector3.up, Quaternion.identity);
        }
        
        //죽으면 scrap 흭득
        if (GameManager.Instance.CurrentRunData is { } currentRunData)// is { } => 왼쪽 데이터가 null이 아니라면.
        {
            VFXManager.Instance.TriggerVFX(VFXType.GET_SCRAP,transform.position,Quaternion.identity);
            
            var dropScrap = Random.Range(scrapMinValue, scrapMaxValue);
            GameManager.Instance.CurrentRunData.scrap += dropScrap;
            UIManager.Instance.inGameUIController.currencyUIController.UpdateScrap();
        }
    }

    public void OnStagger()
    {
        if (blackboard.isDead) return;
        SetState(staggerState);
    }

    public void OnHit(ExtraData extraData)
    {
        // 붉은색으로 변환, enemy 위치 보정
        
        if(blackboard.isDead) return;
        
        Effector.OnHit(1f);
        
        if (blackboard.onHitCancellation != null)
        {
            blackboard.onHitCancellation.Cancel();
            blackboard.onHitCancellation.Dispose();
        }
        blackboard.onHitCancellation = new CancellationTokenSource();
        
        OnHitHelper(extraData).Forget();
    }

    private async UniTask OnHitHelper(ExtraData extraData)
    {
        if (blackboard.aiType == EnemyAIType.Boss) return; // 보스 넉백 없음
        
        CancellationToken token = blackboard.onHitCancellation.Token;
        
        blackboard.enemyRenderer.material.SetTexture("_BaseColor", blackboard.onHitTexture);
        
        // Enemy 넉백 관련은 이 함수를 사용
        float desiredDistance = extraData.weaponRange;
        float enemyPositionCorrection = 0.5f;
        float pullSpeed = 20f;
        float moveTime = 0.2f;
        float duration = 0f;
        
        Vector3 forward = extraData.sourceTransform.forward;
        Vector3 right = extraData.sourceTransform.right;
        Vector3 targetPos = transform.position + forward;
        if ((targetPos - extraData.sourceTransform.position).magnitude > desiredDistance)
        {
            Vector3 dir = transform.position - extraData.sourceTransform.position;
            float rightSize = Vector3.Dot(dir, right);
            if (Mathf.Abs(rightSize) > desiredDistance)
            {
                targetPos = extraData.sourceTransform.position + dir.normalized * desiredDistance;
            }
            else
            {
                float delta = Mathf.Sqrt(desiredDistance * desiredDistance - rightSize * rightSize) 
                              - Vector3.Dot(dir, forward);
                targetPos = transform.position + forward * delta;
            }
        }
        
        var end = transform.position + new Vector3(0, 1, 0);
        
        var didHit = Physics.OverlapCapsuleNonAlloc(
            transform.position + new Vector3(0, .7f, 0), 
            end, 
            .5f,
            _colliders, 
            (1 << LayerMask.NameToLayer("Environment")));

        if (didHit > 0)
        {
            targetPos = transform.position;
        }
        else
        {
            var knockBackDir = targetPos - transform.position;
            var distance = knockBackDir.magnitude;
            knockBackDir.y = 0;
            knockBackDir.Normalize();

            didHit = Physics.CapsuleCastNonAlloc(
                transform.position + new Vector3(0, .7f, 0), 
                end, 
                .3f, 
                knockBackDir,
                _hits,
                distance,
                (1 << LayerMask.NameToLayer("Environment"))
            );
        
            if (didHit > 0)
            {
                targetPos = _hits[0].point - knockBackDir * .1f;
            }
        }
        targetPos.y = transform.position.y;
        try
        {
            while (duration < moveTime)
            {
                Vector3 newPos = Vector3.Lerp(transform.position, targetPos, duration / moveTime);
                transform.position = newPos;
                Agent.nextPosition = newPos;
                duration += Time.deltaTime;
                await UniTask.Yield(token);
            }
        }
        catch (OperationCanceledException){}
        finally
        {
            blackboard.enemyRenderer.material.SetTexture("_BaseColor", blackboard.baseColorTexture);
        }
    }
    
    public void OnDamaged(ExtraData extraData)
    {
        if(blackboard.isDead) return;
        
        // 체력바 감소
        hpBarController.SetHP(blackboard.abilitySystem.GetValue(AttributeType.HP)/blackboard.abilitySystem.GetValue(AttributeType.MaxHP));
        
        DAMAGEType damageType = extraData.isCritical ? DAMAGEType.CRITICAL : extraData.damageType;

        // 피격 효과
        VFXManager.Instance.TriggerDamageNumber(transform.position, extraData.finalAmount, damageType, transform);

        //콤보시스템 카운팅 추가
        if(damageType == DAMAGEType.NORMAL || damageType == DAMAGEType.CRITICAL)
        {
            UIManager.Instance.inGameUIController.AddCombo();
        }

        if(extraData.isCritical)
        {
            CinemachineImpulseController.GenerateImpulse();
            Time.timeScale = 0.1f;
            UniTask.Delay(TimeSpan.FromMilliseconds(100f), DelayType.UnscaledDeltaTime).ContinueWith(() =>
            {
                Time.timeScale = 1;
            });

            if (extraData.hitInfo != null && extraData.hitInfo.hit.point != Vector3.zero)
            {
                VFXManager.Instance.TriggerVFX(VFXType.HIT_CRITICAL, extraData.hitInfo.hit.point, Quaternion.identity, Vector3.one * 0.5f);
                AudioManager.Instance.PlaySFX(AudioBase.SFX.Player.Attack.Critical[0]);
            }
        }
        else
        {
            if(extraData.hitInfo != null && extraData.hitInfo.hit.point != Vector3.zero) VFXManager.Instance.TriggerVFX(VFXType.HIT_NORMAL, extraData.hitInfo.hit.point, Quaternion.identity, Vector3.one * 0.5f);
            AudioManager.Instance.PlaySFX(AudioBase.SFX.Player.Attack.Hit[0]);
        }
        
        
    }

    public virtual void OnNext(HitInfo hitInfo)
    {
        GiveDamageEffect(hitInfo);
    }

    public virtual void GiveDamageEffect(HitInfo hitInfo)
    {
        float damage = blackboard.abilitySystem.GetValue(AttributeType.Strength);
        GameplayEffect damageEffect = new GameplayEffect(EffectType.Instant, AttributeType.Damage, damage);
        GameplayEffect impulseEffect = new GameplayEffect(EffectType.Instant, AttributeType.Impulse, 30);
        damageEffect.extraData.sourceTransform = transform;
        impulseEffect.extraData.sourceTransform = transform;

        if (hitInfo.collider.gameObject.TryGetComponent(out AbilitySystem abilitySystem))
        {
            abilitySystem.ApplyEffect(damageEffect);
            abilitySystem.ApplyEffect(impulseEffect);
        }
    }

    public void OnError(Exception error)
    {
        Debug.LogError(error);
    }

    public void OnCompleted()
    {
    }
    
    public virtual void OnPhaseChange(int phase){}
    
    public void MeleeAttackStart(int throwing = 0)
    {
        HitHandler.StartDetection();
    }

    public void MeleeAttackEnd()
    {
        HitHandler.StopDetection();
    }

    public void CreateAttackEffect(GameObject effectPrefab)
    {
        GameObject effect = Instantiate(effectPrefab, transform.position, Quaternion.identity);
        AttackEffect attackEffect = effect.GetComponent<AttackEffect>();
        float damage = blackboard.abilitySystem.GetValue(AttributeType.Strength);
        float impulse = 10000;
        attackEffect.SetAttackDamageImpulse(damage, impulse);
        
        AudioManager.Instance.PlaySFX(AudioBase.SFX.Boss.Boom.Impact);
    }

    public void CreateAttackEffectAtTarget(GameObject effectPrefab)
    {
        GameObject effect = Instantiate(effectPrefab, blackboard.target.transform.position, Quaternion.identity);
        effect.GetComponent<AttackEffect>().SetAttackDamageImpulse(blackboard.abilitySystem.GetValue(AttributeType.Strength), 30);
    }

    
    //특정 대상에게 극성 상관없이 정해진 동작만을 수행하게도 가능.
    public override async UniTask OnMagneticInteract(MagneticObject target)
    {
        //ex. Enemy에겐 사용 시 무조건 돌진한다.
        //await magnetApproach.Execute(this, target);
        await magnetDashAttackAction.Execute(this, target);
    }

    void ApplySoftCollision(Collider[] colliders)
    {
        Vector3 correction = Vector3.zero;
        foreach (var other in colliders)
        {
            if (other.gameObject == gameObject) continue;
        
            Vector3 diff = transform.position - other.transform.position;
            float dist = diff.magnitude;
            float minDist = 0.5f; // 적당한 간격
            
            if (dist < 0.001f) // 완벽하게 겹칠 경우에는 diff가 Vector3.zero가 되므로 예외처리
                correction += new Vector3(1, 0, 0);
            
            else if (dist < minDist && dist > 0.001f)
            {
                float pushStrength = (minDist - dist) / minDist;
                correction += diff.normalized * pushStrength;
            }
        }
        
        Vector3 newPos = transform.position + correction * Time.deltaTime * 2f;
        if(isFalling)
            newPos.y = transform.position.y;
        else
            newPos.y = Agent.nextPosition.y;
        
        transform.position = newPos;
        Agent.nextPosition = newPos;
    }
    #region debugging
    private void OnDrawGizmos()
    {
        // Gizmos.color = Color.red;
        // Gizmos.DrawSphere(transform.position, blackboard.attackRange);
        //
        // // Agent 목적지 시각화
        // if (Agent != null && Agent.hasPath)
        // {
        //     Gizmos.color = Color.green;
        //     Gizmos.DrawSphere(Agent.destination, 0.5f);
        //     Gizmos.DrawLine(Agent.destination, Agent.destination);
        // }
        // Gizmos.color = Color.blue;
        // Gizmos.DrawRay(transform.position + Vector3.up * 0.5f, transform.forward * blackboard.attackRange);
    }
    
    #endregion
    
    public bool IsAvailableTarget()
    {
        GameObject target = blackboard.target;
        if (target == null) return false;
        if (target.TryGetComponent(out PlayerController player))
        {
            return !player.IsInvisible;
        }
        return false;
    }
    
    
    #region animator
    public void SetAnimBool(string paramName, bool value)
    {
        if (Anim != null && Anim.gameObject != null)
            Anim.SetBool(paramName, value);
    }

    public void SetAnimTrigger(string paramName)
    {
        if (Anim != null && Anim.gameObject != null)
            Anim.SetTrigger(paramName);
    }

    public void SetAnimFloat(string paramName, float value)
    {
        if (Anim != null && Anim.gameObject != null)
            Anim.SetFloat(paramName, value);
    }
    #endregion

    private void ApplyGravity()
    {
        _verticalSpeed += _gravity * Time.fixedDeltaTime;
        _verticalSpeed = Mathf.Clamp(_verticalSpeed, 0f, _maxFallSpeed);
        Vector3 dir = Vector3.down * (_verticalSpeed * Time.fixedDeltaTime);
        // 아래 방향으로 이동
        Agent.nextPosition += dir;
        transform.position += dir;
        // Agent.nextPosition = transform.position;
        
        // 땅에 닿았는지 다시 체크해서 멈추기
        if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, 1f))
        {
            Vector3 destination = new Vector3(transform.position.x, hit.point.y, transform.position.z);
            destination.y += _yOffset;
            Agent.Warp(destination);
            transform.position = destination;
            _verticalSpeed = 0f;
            isFalling = false;
        }
    }
    
    void PlayMeleeAttack()
    {
        var EnemyMeleeAttackSfxRandomClip = AudioManager.Instance.GetRandomClip(AudioBase.SFX.Enemy.Attack.HurtL);
        AudioManager.Instance.PlaySFX(EnemyMeleeAttackSfxRandomClip);
    }
    
    void PlayRangedAttack()
    {
        AudioManager.Instance.PlaySFX(AudioBase.SFX.Enemy.Attack.Cannon.ShotP1);
    }
    
}
