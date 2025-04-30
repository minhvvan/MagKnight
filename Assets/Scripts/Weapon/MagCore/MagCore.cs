using System;
using System.Collections.Generic;
using System.Linq;
using hvvan;
using JetBrains.Annotations;
using Moon;
using UnityEngine;

public enum WeaponType
{
    None = 0,
    SwordKatana,
}

public enum PartsType
{
    None = 0,
    PartA,
    PartB,
    PartC,
    PartD
}

public class MagCore: MonoBehaviour, IInteractable
{
    public ItemCategory category;
    public ItemRarity rarity;
    public Sprite icon;
    public string itemName;
    public int currentUpgradeValue;
    public int scrapValue;
    
    private MagCoreSO _magCoreSO;
    [SerializeField] private WeaponType weaponType;
    [SerializeField] private PartsType partsType;
    
    private List<MeshRenderer> _renderers = new List<MeshRenderer>();
    public Action onChooseItem;
    private Rigidbody rb;
    private Collider col;
    private bool _isStake;
    
    private void Awake()
    {
        _renderers = GetComponentsInChildren<MeshRenderer>().ToList();
        rb = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();
        category = ItemCategory.MagCore;
    }

    public void SetMagCoreData(MagCoreSO magCoreData = null, [CanBeNull] MagCore newMagCore = null)
    {
        if (magCoreData == null && newMagCore != null)
        {
            weaponType = newMagCore.weaponType;
            partsType = newMagCore.partsType;
            icon = newMagCore.icon;
            gameObject.name = itemName = newMagCore.itemName;
            scrapValue = newMagCore.scrapValue;
            currentUpgradeValue = newMagCore.currentUpgradeValue;
            return;
        }
        
        if (magCoreData == null) return;
        _magCoreSO = magCoreData;
        weaponType = _magCoreSO.weaponType;
        partsType = _magCoreSO.partsType;
        icon = _magCoreSO.icon;
        gameObject.name = itemName = _magCoreSO.itemName;
        scrapValue = _magCoreSO.scrapValue;
    }

    public void SetItemClass((ItemCategory, ItemRarity) itemClass)
    {
        (category, rarity) = itemClass;
    }
    
    public void SetPartsEffect(AbilitySystem abilitySystem)
    {
        if (abilitySystem == null) return;
        _magCoreSO.ApplyTo(abilitySystem, currentUpgradeValue);
    }

    public void RemovePartsEffect(AbilitySystem abilitySystem)
    {
        if (abilitySystem == null) return;
        _magCoreSO.RemoveTo(abilitySystem, currentUpgradeValue);
    }

    public MagCoreSO GetMagCoreSO()
    {
        return _magCoreSO;
    }

    public void Upgrade(AbilitySystem abilitySystem)
    {
        if (currentUpgradeValue == _magCoreSO.maxUpgradeLevel)
        {
            Debug.Log("Upgrade Part Max Level");
            return;
        }
        RemovePartsEffect(abilitySystem);
        currentUpgradeValue += 1;
        //Debug.Log("LEVEL: " + currentUpgradeValue);
        SetPartsEffect(abilitySystem);
    }

    public void Interact(IInteractor interactor)
    {
        if (interactor.GetGameObject().TryGetComponent<PlayerController>(out var player))
        {
            //TODO: 무기교체 로직 추가 수행
            transform.SetParent(player.transform);
            gameObject.SetActive(false);
            
            player.SetCurrentWeapon(weaponType, this);
            var abilitySystem = player.GetComponent<AbilitySystem>();
            SetPartsEffect(abilitySystem);
            GameManager.Instance.CurrentRunData.currentMagCore = this;
            GameManager.Instance.CurrentRunData.currentPartsUpgradeValue = currentUpgradeValue;
            
            onChooseItem?.Invoke();
        }
    }

    public void Dismantle(IInteractor interactor)
    {
        if (interactor.GetGameObject().TryGetComponent<PlayerController>(out var player))
        {
            var scrap = GameManager.Instance.CurrentRunData.scrap += scrapValue;
            Debug.Log("Scrap:" + scrap);
            Dismantling();
        }
    }

    //제거 연출 넣는 곳
    private void Dismantling()
    {
        onChooseItem?.Invoke();
        var vfxObj = Instantiate(ItemManager.Instance.dismantleVfxPrefab, transform.position, Quaternion.identity);
        vfxObj.transform.localScale *= 0.1f;
        Destroy(vfxObj, 3f);
        Destroy(gameObject);
    }

    public void Select()
    {
        //TODO: outline
        _renderers.ForEach(render => render.material.color = Color.green);
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
    
    public void OnCollisionEnter(Collision other)
    {
        if(other.gameObject.layer != (1 <<LayerMask.NameToLayer("Player") | 1 << LayerMask.NameToLayer("Enemy")))
        {
            if(!_isStake) OnStakeMode();
        }
    }

    public void OnStakeMode()
    {
        _isStake = true;
        rb.isKinematic = true;
        col.isTrigger = true;
    }
}
