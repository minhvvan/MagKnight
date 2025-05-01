using System;
using System.Collections;
using System.Collections.Generic;
using AYellowpaper.SerializedCollections;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Serialization;


[Serializable]
[CreateAssetMenu(fileName = "MagCoreSO", menuName = "SO/MagCore/MagCoreSO")]
public class MagCoreSO : ScriptableObject
{
    [Header("MagCore Info")]
    public Sprite icon;
    public string itemName;
    public string description;
    public int scrapValue;
    public int maxUpgradeLevel;
    
    [Header("MagCore Status")]
    public WeaponType weaponType;
    public PartsType partsType;
    
    //Key는 파츠의 강화 레벨 단위입니다. 0 이면 강화되지 않은 순정 상태입니다.
    
    [Header("Parts Buff"), Tooltip("파츠 장착 시 고유적으로 증가하는 능력치/효과 입니다.")]
    public string descriptionPassiveEffects;
    public SerializedDictionary<int,List<PassiveEffectData>> passiveEffects = new SerializedDictionary<int,List<PassiveEffectData>>();
    public string descriptionGameplayEffects;
    public SerializedDictionary<int,List<GameplayEffect>> gameplayEffects = new SerializedDictionary<int,List<GameplayEffect>>();

    [Header("Magnet Switch Effect"), Tooltip("극성 전환 시 일시적으로 적용되는 능력치/효과 입니다.")]
    public float magnetEffectDuration; //극성 전환 효과 지속시간
    public string descriptionMagnetPassiveEffects;
    public SerializedDictionary<int,List<PassiveEffectData>> magnetPassiveEffects = new SerializedDictionary<int,List<PassiveEffectData>>();
    public string descriptionMagnetGameplayEffects;
    public SerializedDictionary<int,List<GameplayEffect>> magnetGameplayEffects = new SerializedDictionary<int,List<GameplayEffect>>();
    
    //파츠를 장착하는 동안 영구적으로 적용되는 효과를 적용합니다.
    public void ApplyTo(AbilitySystem target, int currentLevel)
    {
        foreach (var pair in passiveEffects)
        {
            if (pair.Key != currentLevel) continue;
            foreach (var instance in pair.Value)
            {
                target.RegisterPassiveEffect(instance);
            }
        }
        foreach (var pair in gameplayEffects)
        {
            if (pair.Key != currentLevel) continue;
            foreach (var instance in pair.Value)
            {
                target.ApplyEffect(instance);
            }
        }
    }
    
    //파츠 효과를 제거합니다.
    public void RemoveTo(AbilitySystem target, int currentLevel)
    {
        foreach (var pair in passiveEffects)
        {
            if (pair.Key != currentLevel) continue;
            foreach (var instance in pair.Value)
            {
                target.RemovePassiveEffect(instance);
            }
        }
        foreach (var pair in gameplayEffects)
        {
            if (pair.Key != currentLevel) continue;
            foreach (var instance in pair.Value)
            {
                target.RemoveEffect(instance);
            }
        }
    }

    //극성전환 효과를 정해진 시간동안 적용 후, 해제 합니다.
    public IEnumerator MagnetSwitchEffect(AbilitySystem target, int currentLevel, float duration, 
        Action onComplete = null)
    {
        //효과 적용
        foreach (var pair in magnetPassiveEffects)
        {
            if (pair.Key != currentLevel) continue;
            foreach (var instance in pair.Value)
            {
                target.RegisterPassiveEffect(instance);
            }
        }
        foreach (var pair in magnetGameplayEffects)
        {
            if (pair.Key != currentLevel) continue;
            foreach (var instance in pair.Value)
            {
                target.ApplyEffect(instance);
            }
        }
        
        //적용 해제까지의 타이머
        var elapsedTime = 0f;
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        //효과 해제
        foreach (var pair in magnetPassiveEffects)
        {
            if (pair.Key != currentLevel) continue;
            foreach (var instance in pair.Value)
            {
                target.RemovePassiveEffect(instance);
            }
        }
        foreach (var pair in magnetGameplayEffects)
        {
            if (pair.Key != currentLevel) continue;
            foreach (var instance in pair.Value)
            {
                target.RemoveEffect(instance);
            }
        }
        onComplete?.Invoke();
    }
    
    
    
    //
    
    private void OnValidate()
    {
        //음수 방지
        maxUpgradeLevel = Mathf.Max(0, maxUpgradeLevel);
        ResizeArray();
    }
    
    private void ResizeArray()
    {
        // Key를 유지할 집합
        HashSet<int> desiredKeys = new HashSet<int>();
        for (int i = 0; i < maxUpgradeLevel; i++)
        {
            desiredKeys.Add(i);
        }

        // 1. 필요 없는 Key 삭제
        List<int> keysToRemove = new List<int>();
        
        //파츠 패시브
        foreach (var key in passiveEffects.Keys)
        {
            if (!desiredKeys.Contains(key))
            {
                keysToRemove.Add(key);
            }
        }
        foreach (var key in keysToRemove)
        {
            passiveEffects.Remove(key);
        }
        
        //파츠 스탯강화
        foreach (var key in gameplayEffects.Keys)
        {
            if (!desiredKeys.Contains(key))
            {
                keysToRemove.Add(key);
            }
        }
        foreach (var key in keysToRemove)
        {
            gameplayEffects.Remove(key);
        }
        
        //극성 스위칭 패시브 효과
        foreach (var key in magnetPassiveEffects.Keys)
        {
            if (!desiredKeys.Contains(key))
            {
                keysToRemove.Add(key);
            }
        }
        foreach (var key in keysToRemove)
        {
            magnetPassiveEffects.Remove(key);
        }
        
        //극성 스위칭 스탯 효과
        foreach (var key in magnetGameplayEffects.Keys)
        {
            if (!desiredKeys.Contains(key))
            {
                keysToRemove.Add(key);
            }
        }
        foreach (var key in keysToRemove)
        {
            magnetGameplayEffects.Remove(key);
        }

        // 2. 필요한 Key 추가
        for (int i = 0; i < maxUpgradeLevel; i++)
        {
            if (!passiveEffects.ContainsKey(i))
            {
                passiveEffects.Add(i, new List<PassiveEffectData>());
            }
            if (!gameplayEffects.ContainsKey(i))
            {
                gameplayEffects.Add(i, new List<GameplayEffect>());
            }
            if (!magnetPassiveEffects.ContainsKey(i))
            {
                magnetPassiveEffects.Add(i, new List<PassiveEffectData>());
            }
            if (!magnetGameplayEffects.ContainsKey(i))
            {
                magnetGameplayEffects.Add(i, new List<GameplayEffect>());
            }
        }
#if UNITY_EDITOR
        // OnValidate에서 Dictionary 수정 후 ScriptableObject 저장 표시
        UnityEditor.EditorUtility.SetDirty(this);
#endif
    }
}
