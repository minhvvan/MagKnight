using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using AYellowpaper.SerializedCollections;
using Cysharp.Threading.Tasks;
using Managers;
using UnityEngine;
using Random = UnityEngine.Random;

public class AbilitySystem : MonoBehaviour
{
    [SerializeReference, SubclassPicker] private AttributeSet Attributes;
    
    // 기존 GameplayEffect는 가지면서 기존 GameplayEffect에 영향을 주지않기 위해 Hash를 가져와서 사용
    // 저장되는 GameplayEffect는 실제 적용된 Gameplay의 Instance
    // Remove를 요청할 때는 요청자는 단순히 나의 GE를 삭제해달라고 요청해주면 된다.
    [SerializeField] SerializedDictionary<int, GameplayEffect> _activatedEffects = new SerializedDictionary<int, GameplayEffect>();

    // Passive를 담고 있는 Dicitionary
    [SerializeField] SerializedDictionary<int, PassiveEffectData> _registeredPassiveEffects = new SerializedDictionary<int, PassiveEffectData>();

    // 버프/디버프 갱신 용 Dictionary
    private Dictionary<int, CancellationTokenSource> _durationEffectTokens = new();
    
    // PlayerStat -> Attribute
    public void InitializeFromPlayerStat(PlayerStat playerStat = null)
    {
        if (playerStat == null)
        {
            Debug.LogError("PlayerStat is not assigned!");
            return;
        }

        //이전 Attribute 삭제
        Attributes.ClearAllAttributes();
        
        // PlayerStat의 모든 필드를 순회
        foreach (var field in typeof(PlayerStat).GetFields())
        {
            // 필드 값을 AttributePair로 가져옴
            AttributePair attributePair = (AttributePair)field.GetValue(playerStat);
            
            // AbilitySystem에 값 추가
            AddAttribute(attributePair.Key, attributePair.Value);
        }
    }

    public void InitializeFromEnemyStat(EnemyStat enemyStat = null)
    {
        if (enemyStat == null)
        {
            Debug.LogError("EnemyStat is not assigned!");
            return;
        }
        
        Attributes.ClearAllAttributes();
        foreach (var field in typeof(EnemyStat).GetFields())
        {
            // 필드 값을 AttributePair로 가져옴
            AttributePair attributePair = (AttributePair)field.GetValue(enemyStat);
            
            // AbilitySystem에 값 추가
            AddAttribute(attributePair.Key, attributePair.Value);
        }
    }
    
    #region Attribute
    
    public void AddAttribute(AttributeType type, float value)
    {
        Attributes.AddAttribute(type, value);
    }

    // Attribute의 CurrentValue를 가져옴
    public float GetValue(AttributeType type)
    {
        return Attributes.GetValue(type);
    }
    
    #endregion
    
    #region Effect
    
    // effect 개념으로 관리하고 싶은 것은 ApplyEffect를 한다.
    // e.g. 아티팩트 효과, 버프 디버프 등
    public void ApplyEffect(GameplayEffect gameplayEffect)
    {
        // instance로 만드는 이유 : gameplayEffect를 직접적으로 수정하지 않도록
        // AttributeSet 안에 PreAttributeChange에서 수정 위험 요소 있음
        var instanceGE = gameplayEffect.DeepCopy();
        var hash = gameplayEffect.GetHashCode();
        
        if(gameplayEffect.effectType == EffectType.Instant)
            Attributes.Modify(instanceGE);
        
        if(gameplayEffect.effectType == EffectType.Duration)
        {
            if (gameplayEffect.period > 0f)
            {
                ApplyPeriodicEffect(instanceGE);
            }
            else
            {
                if (_activatedEffects.TryGetValue(hash, out var effect))
                {
                    if (effect.currentStack < effect.maxStack)
                    {
                        Attributes.Modify(instanceGE);
                        effect.currentStack++;
                        effect.amount += gameplayEffect.amount;
                    }
                }
                // 처음 적용
                else
                {
                    Attributes.Modify(instanceGE);
                    instanceGE.currentStack = 1;
                }
                //Remove 항상 기존 gameplayEffect걸로 -> Hash때문
                RemoveAfterDuration(gameplayEffect);
            }
        }
        
        // Infinite는 항상 저장
        if (instanceGE.tracking || gameplayEffect.effectType == EffectType.Infinite)
        {
            if(gameplayEffect.effectType == EffectType.Infinite)
                Attributes.Modify(instanceGE);
            _activatedEffects.TryAdd(hash, instanceGE);
        }
    }
    
    public void RemoveEffect(GameplayEffect gameplayEffect)
    {
        if (gameplayEffect.tracking || gameplayEffect.effectType == EffectType.Infinite)
        {
            if (_activatedEffects.TryGetValue(gameplayEffect.GetHashCode(), out var effect))
            {
                effect.amount = -effect.amount; 
                Attributes.Modify(effect);
                _activatedEffects.Remove(gameplayEffect.GetHashCode());
            }
            else // tracking 중인데 _activatedEffect가 없을수도
            {
                Debug.Log("해당 이펙트는 이미 제거되었습니다");
            }
        }
        else
        {
            var instanceGE = gameplayEffect.DeepCopy();
            instanceGE.amount = -instanceGE.amount;
            Attributes.Modify(instanceGE);
        }
    }
    
    //Duration에서 사용됨
    private async UniTask RemoveAfterDuration(GameplayEffect gameplayEffect)
    {
        // await UniTask.WaitForSeconds(gameplayEffect.duration);
        // RemoveEffect(gameplayEffect);
        
        var id = gameplayEffect.GetHashCode();

        // 기존 타이머가 있다면 취소
        if (_durationEffectTokens.TryGetValue(id, out var oldToken))
        {
            oldToken.Cancel();
            _durationEffectTokens.Remove(id);
        }

        // 새로운 타이머 시작
        var cts = new CancellationTokenSource();
        _durationEffectTokens[id] = cts;

        try
        {
            await UniTask.Delay(TimeSpan.FromSeconds(gameplayEffect.duration), cancellationToken: cts.Token);
            RemoveEffect(gameplayEffect);
            _durationEffectTokens.Remove(id);
        }
        catch (OperationCanceledException)
        {
            // 취소된 경우 아무것도 하지 않음
        }
    }

    public T GetAttributeSet<T>() where T : AttributeSet
    {
        return Attributes as T;
    }

    public bool TryGetAttributeSet<T>(out T outVar) where T : AttributeSet
    {
        outVar = GetAttributeSet<T>();
        return outVar != null;
    }
    
    private async UniTaskVoid ApplyPeriodicEffect(GameplayEffect gameplayEffect)
    {
        if(_activatedEffects.ContainsKey(gameplayEffect.GetHashCode())) return;
        
        float elapsed = 0f;
        while (elapsed < gameplayEffect.duration)
        {
            var tickEffect = gameplayEffect.DeepCopy();
            Attributes.Modify(tickEffect);

            await UniTask.Delay(System.TimeSpan.FromSeconds(gameplayEffect.period));
            elapsed += gameplayEffect.period;
        }
    }
    
    #endregion
    
    #region Passive
    
    public void RegisterPassiveEffect(PassiveEffectData passiveData)
    {
        var instancePassive = passiveData.DeepCopy();
        _registeredPassiveEffects.TryAdd(passiveData.GetHashCode(), instancePassive);
    }

    public void RemovePassiveEffect(PassiveEffectData passiveData)
    {
        if(_registeredPassiveEffects.ContainsKey(passiveData.GetHashCode()))
            _registeredPassiveEffects.Remove(passiveData.GetHashCode());
        else
        {
            Debug.Log("해당 이펙트는 이미 제거되었습니다");
        }
    }
    
    public void TriggerEvent(TriggerEventType eventType, AbilitySystem target)
    {
        List<PassiveEffectData> removeList = new List<PassiveEffectData>();
        foreach (var passiveEffect in _registeredPassiveEffects)
        {
            if (passiveEffect.Value.triggerEvent == eventType)
            {
                if (Random.value <= passiveEffect.Value.triggerChance && target != null)
                {
                    if (passiveEffect.Value.hasCount)
                    {
                        if (passiveEffect.Value.triggerCount > 0)
                        {
                            target.ApplyEffect(passiveEffect.Value.effect);
                            passiveEffect.Value.triggerCount--;
                            if(passiveEffect.Value.triggerCount == 0)
                                removeList.Add(passiveEffect.Value);
                        }
                    }
                    else
                    {
                        target.ApplyEffect(passiveEffect.Value.effect);
                    }
                }
            }   
        }
        // 순회 도중 삭제하면 error여서
        foreach (var remove in removeList)
        {
            RemovePassiveEffect(remove);
        }
    }
    
    #endregion
    
}

