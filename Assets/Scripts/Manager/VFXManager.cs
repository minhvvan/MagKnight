using System;
using System.Collections.Generic;
using AYellowpaper.SerializedCollections;
using Cysharp.Threading.Tasks;
using DamageNumbersPro;
using Managers;
using Unity.VisualScripting;
using UnityEngine;

public class VFXManager : Singleton<VFXManager>
{
    Dictionary<VFXType, Queue<GameObject>> vfxPools = new Dictionary<VFXType, Queue<GameObject>>();
    Dictionary<VFXType, VFXDataSO> vfxDataSOs = new Dictionary<VFXType, VFXDataSO>();
    
    [SerializeField] SerializedDictionary<DAMAGEType, DamageNumber> damageNumberPrefabs;

    public bool IsInitialized { get; private set; }
    
    private async void Start()
    {
        await Initialize();
        IsInitialized = true;
    }
    
    public new async UniTask Initialize()
    {
        await LoadVFXObjects("Loader");
    }

    async UniTask LoadVFXObjects(string sceneName)
    {
        // SceneName에서 필요한 vfx들을 pooling 해놓는다
        // 해당 Scene에서 필요한 vfx의 종류는 VFXLoadSO scriptableObject에서 관리한다.
        VFXLoadSO vfxLoadSO = null;
        switch (sceneName)
        {
            case "Loader":
                vfxLoadSO = await DataManager.Instance.LoadData<VFXLoadSO>(Addresses.Data.FX.VFXLOADER);
                break;
            default:
                Debug.LogError($"{sceneName} not found!");
                break;
        }
        foreach (var vfxDataSO in vfxLoadSO.vfxDataSOs)
        {
            Instance.RegisterVFX(vfxDataSO);
        }
    }
    
    private void RegisterVFX(VFXDataSO vfxDataSO)
    {
        if (!vfxPools.ContainsKey(vfxDataSO.vfxType))
        {
            vfxPools[vfxDataSO.vfxType] = new Queue<GameObject>();
            vfxDataSOs[vfxDataSO.vfxType] = vfxDataSO;

            for (int i = 0; i < vfxDataSO.poolSize; i++)
            {
                GameObject vfxObject = Instantiate(vfxDataSO.vfxPrefab, transform);
                vfxObject.SetActive(false);
                vfxPools[vfxDataSO.vfxType].Enqueue(vfxObject);
            }
        }
    }

    public GameObject TriggerVFX(VFXType vfxType, Vector3 position, Quaternion rotation = default, Vector3 size = default, bool returnAutomatically = true)
    {
        rotation = rotation == default ? Quaternion.identity : rotation;
        size = size == default ? vfxDataSOs[vfxType].size : size;
        
        GameObject vfxObject = DequeueVFX(vfxType);
        
        vfxObject.transform.SetPositionAndRotation(position, rotation);
        vfxObject.transform.localScale = size;
        vfxObject.SetActive(true);

        if (returnAutomatically)
        {
            UniTask.Void(async () => await ReturnVFX(vfxType, vfxObject, vfxDataSOs[vfxType].duration));
        }
        return vfxObject;
    }
    
    public GameObject TriggerVFX(VFXType vfxType, Transform parent, Vector3 position = default, Quaternion rotation = default, Vector3 size = default, bool returnAutomatically = true)
    {
        /// <summary>
        /// parent object가 존재할시, VFX를 실행하는 함수
        /// </summary>
        /// <param name="vfxType">실행하려고 하는 VFX type</param>
        /// <param name="parent">부모 object의 transform</param>
        /// <param name="position">local position(default: parent.position)</param>
        /// <param name="rotation">local rotation(default: parent.rotation) </param>
        /// <param name="size">worldspace scale(default: SO data에 적힌 size)</param>
        /// <param name="returnAutomatically">false일시 자동으로 disable되지 않는다. false로 설정했을 시, ReturnVFX 호출 필요</param>
        /// <returns>실행하는 VFX object</returns>
        
        position = position == default ? Vector3.zero : position;
        rotation = rotation == default ? Quaternion.identity : rotation;
        size = size == default ? vfxDataSOs[vfxType].size : size;
        
        GameObject vfxObject = DequeueVFX(vfxType);
        
        vfxObject.transform.localScale = size;
        vfxObject.transform.SetParent(parent);
        vfxObject.transform.localPosition = position;
        vfxObject.transform.localRotation = rotation;
        
        vfxObject.SetActive(true);

        if (returnAutomatically)
        {
            UniTask.Void(async () => await ReturnVFX(vfxType, vfxObject, vfxDataSOs[vfxType].duration));
        }
        return vfxObject;
    }
    
    public void TriggerVFX(GameObject particlePrefab, Vector3 position, Quaternion rotation = default, Vector3 size = default, bool returnAutomatically = true)
    {
        GameObject particleInstance = Instantiate(particlePrefab);
        particleInstance.transform.SetPositionAndRotation(position, rotation);
    
        if (size != default)
        {
            particleInstance.transform.localScale = size;
        }

        if (returnAutomatically)
        {
            UniTask.Void(async () => await WaitUntilParticleEnd(particleInstance));
        }
    }
    

    private GameObject DequeueVFX(VFXType vfxType)
    {
        if (!vfxPools.ContainsKey(vfxType))
        {
            Debug.LogError($"{vfxType} doesn't exist in VFXPools!");
            return null;
        }
        
        if (vfxPools[vfxType].Count <= 0)
        {
            return Instantiate(vfxDataSOs[vfxType].vfxPrefab);
        }
        else
        {
            return vfxPools[vfxType].Dequeue();
        }
    }

    public void ReturnVFX(VFXType vfxType, GameObject vfxObject)
    {
        vfxObject.transform.SetParent(Instance.transform);
        vfxObject.SetActive(false);
        vfxPools[vfxType].Enqueue(vfxObject);
    }

    private async UniTask ReturnVFX(VFXType vfxType, GameObject vfxObject, float duration)
    {
        await UniTask.Delay(TimeSpan.FromSeconds(duration));
        vfxObject.transform.SetParent(Instance.transform);
        vfxObject.SetActive(false);
        vfxPools[vfxType].Enqueue(vfxObject);
    }

    public void TriggerDamageNumber(Vector3 position, float damage, DAMAGEType damageType = DAMAGEType.NORMAL)
    {
        DamageNumber damageNumberPrefab = damageNumberPrefabs[damageType];

        damageNumberPrefab.Spawn(position, damage);
    }

    private async UniTask WaitUntilParticleEnd(GameObject vfxObject)
    {
        var particle = vfxObject.GetComponent<ParticleSystem>();
        particle.Play();
        
        while (particle != null && vfxObject != null && particle.isPlaying)
        {
            await UniTask.Yield();
        }
        
        // 게임 플레이 모드인지 에디터 모드인지에 따라 Destroy
        if (vfxObject != null)
        {
            if (Application.isPlaying)
                Destroy(vfxObject);
            else
                DestroyImmediate(vfxObject);
        }
    }
}