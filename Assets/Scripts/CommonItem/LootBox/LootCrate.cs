using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Moon;
using UnityEngine;
using UnityEngine.Serialization;

public class LootCrate : MonoBehaviour, IInteractable
{
    [Header("Crate Type")]
    public ItemCategory crateCategory;
    public ItemRarity crateRarity;
    
    [Header("Crate Elements")]
    public MeshRenderer BaseRenderer;
    public Transform[] itemPoint;
    public int maxSpawnCount;
    
    private Animator _animator;
    private List<GameObject> _items = new List<GameObject>();
    private bool _isOpen = false;
    private bool _isChoose = false;
    private Vector3 _openAngle;
    
    private List<MeshRenderer> _renderers = new List<MeshRenderer>();
    private Material _baseMaterial;

    public Dictionary<ItemRarity, List<GameObject>> rarityVfxObjects;
    private GameObject vfxObj;
    
    private void Awake()
    {
        _renderers = GetComponentsInChildren<MeshRenderer>().ToList();
        _baseMaterial = BaseRenderer.material;
        _animator = GetComponent<Animator>();
        
        var rb = GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionZ |
                         RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ | 
                         RigidbodyConstraints.FreezeRotationY;
        _openAngle = new Vector3(-90f,0,0);
        
        if(maxSpawnCount == 0) maxSpawnCount = 3;
    }

    private void Start()
    {
        InitializeCrate();
    }

    private void InitializeCrate()
    {
        vfxObj = Instantiate(rarityVfxObjects[crateRarity][0], transform);
        vfxObj.transform.localScale = new Vector3(2,2,2);
    }
    
    public void SetLootCrate(ItemCategory category, ItemRarity rarity)
    {
        crateCategory = category;
        crateRarity = rarity;
    }

    private async UniTask OpenCrate()
    {
        _isOpen = true;
        _animator.SetBool("Open", true);
        
        await UniTask.WaitUntil(()=>_animator.GetCurrentAnimatorStateInfo(0).IsName("WeaponCrate_Open") 
                                    && _animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 0.6f);
        Destroy(vfxObj);
        vfxObj = Instantiate(rarityVfxObjects[crateRarity][1], transform);
        vfxObj.transform.localScale = new Vector3(4,4,4);
        
        await UniTask.WaitUntil(()=>_animator.GetCurrentAnimatorStateInfo(0).IsName("WeaponCrate_Open") 
                                    && _animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f);
        
        for (int i = 0; i < maxSpawnCount; i++)
        {
            await UniTask.Delay(250);
            
            var item =ItemManager.Instance.CreateItem
                (crateCategory,crateRarity,itemPoint[i].position,Quaternion.identity, parent: itemPoint[i]);
            item.TryGetComponent(out Rigidbody rb);
            rb.isKinematic = true;
            var flashVfx = Instantiate(rarityVfxObjects[crateRarity][2], item.transform);
            Destroy(flashVfx.gameObject, 0.25f);
            switch (crateCategory)
            {
                case ItemCategory.Artifact:
                    item.GetComponent<ArtifactObject>().onChooseItem = CloseCrate;
                    break;
                case ItemCategory.MagCore:
                    item.GetComponent<MagCore>().onChooseItem = CloseCrate;
                    break;
                case ItemCategory.HealthPack:
                    item.GetComponent<HealthPack>().onChooseItem = CloseCrate;
                    break;
            }
            _items.Add(item);
        }
    }

    private void CloseCrate()
    {
        Closing().Forget();
    }

    private async UniTask Closing()
    {
        _animator.SetBool("Open", false);
        foreach (var item in _items)
        {
            if(item != null) item.GetComponent<Collider>().enabled = false;
        }
        
        //상자 점등 꺼지기
        _baseMaterial.SetColor("_EmissionColor", Color.red);
        
        //상자 위에 제공된 모든 아이템 제거
        await UniTask.WaitUntil(()=>_animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f 
                                    && _animator.GetCurrentAnimatorStateInfo(0).IsName("WeaponCrate_Close"));
        Destroy(vfxObj);
        foreach (var item in _items)
        {
            if(item != null) Destroy(item.gameObject);
        }
    }

    public void Interact(IInteractor interactor)
    {
        if (interactor.GetGameObject().TryGetComponent<PlayerController>(out var player))
        {
            if (!_isOpen) OpenCrate().Forget();
        }
    }

    public void Select()
    {
        //TODO: outline
        if(!_isOpen) _renderers.ForEach(render => render.material.color = Color.green);
    }

    public void UnSelect()
    {
        //TODO: outline 제거
        _renderers.ForEach(render => render.material.color = Color.gray);
    }

    public GameObject GetGameObject()
    {
        return gameObject;
    }   
}
