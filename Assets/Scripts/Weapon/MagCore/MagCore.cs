using System;
using System.Collections.Generic;
using System.Linq;
using Highlighters;
using hvvan;
using JetBrains.Annotations;
using Moon;
using UnityEngine;

public enum WeaponType
{
    None = 0,
    GreatSword = 1,
    Bow = 2,
    Hammer = 3,
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
    public string itemDescription;
    public int currentUpgradeValue;
    public int scrapValue;
    public bool IsProduct { get; set; }
    
    [SerializeField] private MagCoreSO _magCoreSO; //필드 배치시 여기에 임의로 SO할당 해주시면 됩니다.
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
        
        if(_magCoreSO != null) SetMagCoreData(_magCoreSO);
    }

    public void SetMagCoreData(MagCoreSO magCoreData = null, [CanBeNull] MagCore newMagCore = null)
    {
        if (magCoreData == null && newMagCore != null)
        {
            weaponType = newMagCore.weaponType;
            partsType = newMagCore.partsType;
            icon = newMagCore.icon;
            gameObject.name = itemName = newMagCore.itemName;
            itemDescription = newMagCore.itemDescription;
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
        itemDescription = _magCoreSO.description;
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

    public async void Interact(IInteractor interactor)
    {
        if (interactor.GetGameObject().TryGetComponent<PlayerController>(out var player))
        {
            transform.SetParent(player.transform);
            gameObject.SetActive(false);
            
            player.SetCurrentWeapon(weaponType, this);
            var abilitySystem = player.AbilitySystem;
            SetPartsEffect(abilitySystem);
            if (GameManager.Instance.CurrentRunData != null)
            {
                var currentRunData = GameManager.Instance.CurrentRunData;
                currentRunData.currentMagCoreSO = _magCoreSO;
                currentRunData.currentItemName = itemName;
                currentRunData.currentItemCategory = category;
                currentRunData.currentItemRarity = rarity;
                currentRunData.currentWeapon = weaponType;
                currentRunData.currentPartsUpgradeValue = currentUpgradeValue;

                await GameManager.Instance.SaveData(Constants.CurrentRun);
            }
            
            
            onChooseItem?.Invoke();
        }
    }

    public void Dismantle(IInteractor interactor)
    {
        if (interactor.GetGameObject().TryGetComponent<PlayerController>(out var player))
        {
            GameManager.Instance.CurrentRunData.scrap += scrapValue;
            UIManager.Instance.inGameUIController.currencyUIController.UpdateScrap();
            //_= GameManager.Instance.SaveData(Constants.CurrentRun);
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

    public void Select(Highlighter highlighter)
    {
        foreach (var crateRenderer in _renderers)
        {
            highlighter.Renderers.Add(new HighlighterRenderer(crateRenderer, 1));
        }
        
        var uiController =  UIManager.Instance.popupUIController.productUIController;
        uiController.SetItemText(gameObject);
        uiController.ShowUI();
    }

    public void UnSelect(Highlighter highlighter)
    {
        foreach (var crateRenderer in _renderers)
        {
            highlighter.Renderers.Remove(new HighlighterRenderer(crateRenderer, 1));
        }
        
        var uiController =  UIManager.Instance.popupUIController.productUIController;
        uiController.HideUI();
    }

    public GameObject GetGameObject()
    {
        return gameObject;
    }

    public InteractType GetInteractType()
    {
        return InteractType.Loot;
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
        if(rb != null)  rb.isKinematic = true;
        if(col != null) col.isTrigger = true;
    }
}
