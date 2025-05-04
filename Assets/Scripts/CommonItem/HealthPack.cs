using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Moon;
using UnityEngine;
using UnityEngine.Serialization;

public class HealthPack : MonoBehaviour, IInteractable
{
    public ItemCategory category;
    public ItemRarity rarity;
    
    private HealthPackSO healthPackSo;
    public Sprite icon;
    public string itemName;
    public string itemDescription;
    public float healValue;
    public int scrapValue;
    
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

        category = ItemCategory.HealthPack;
    }

    public void SetHealthPotionData(HealthPackSO healthPackData)
    {
        healthPackSo = healthPackData;
        icon = healthPackSo.icon;
        gameObject.name = itemName = healthPackSo.itemName;
        itemDescription = healthPackSo.description;
        healValue = healthPackSo.healValue;
        scrapValue = healthPackSo.scrapValue;
    }
    
    public void SetItemClass((ItemCategory, ItemRarity) itemClass)
    {
        (category, rarity) = itemClass;
    }

    public void Interact(IInteractor interactor)
    {
        if (interactor.GetGameObject().TryGetComponent<PlayerController>(out var player))
        {
            //TODO: 플레이어 체력 회복
            onChooseItem?.Invoke();
            Destroy(gameObject);
        }
    }

    public void Select()
    {
        //TODO: outline
        _renderers.ForEach(render => render.material.color = Color.green);
        
        var uiController =  UIManager.Instance.popupUIController.productUIController;
        uiController.SetItemText(gameObject);
        uiController.ShowUI();
    }

    public void UnSelect()
    {
        //TODO: outline 제거
        _renderers.ForEach(render => render.material.color = Color.gray);
        
        var uiController =  UIManager.Instance.popupUIController.productUIController;
        uiController.HideUI();
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
        if(rb != null)  rb.isKinematic = true;
        if(col != null) col.isTrigger = true;
    }
}
