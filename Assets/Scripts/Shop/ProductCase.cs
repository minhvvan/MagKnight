using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Highlighters;
using hvvan;
using UnityEngine;

public class ProductCase : MonoBehaviour, IInteractable
{
    public ItemCategory itemCategory;
    public ItemRarity itemRarity;
    public string itemName;
    public string itemDescription;
    public int itemPrice;
    public GameObject inItem;
    private List<MeshRenderer> _renderers = new List<MeshRenderer>();
    
    private void Awake()
    {
        //필드 사전배치 대응
        if(inItem != null) Casing(inItem);
        _renderers = GetComponentsInChildren<MeshRenderer>().ToList();
    }

    private void Update()
    {
        //테스트용 재화 증가.
        if (Input.GetKeyDown(KeyCode.P))
        {
            GameManager.Instance.CurrentRunData.scrap += 100;
        }   
    }
    
    //아이템을 포장.
    public void Casing(GameObject item)
    {
        inItem = item;
        inItem.transform.SetParent(transform);
        var col = inItem.GetComponent<Collider>();
        
        if (inItem.TryGetComponent(out ArtifactObject artifactObject))
        {
            artifactObject.OnStakeMode();
            itemPrice = artifactObject.scrapValue;
            itemName = artifactObject.itemName;
            itemDescription = artifactObject.itemDescription;
            itemRarity = artifactObject.rarity;
            itemCategory = artifactObject.category;
        }
        else if (inItem.TryGetComponent(out MagCore magCore))
        {
            magCore.OnStakeMode();
            itemPrice = magCore.scrapValue;
            itemName = magCore.itemName;
            itemDescription = magCore.itemDescription;
            itemRarity = magCore.rarity;
            itemCategory = magCore.category;
        }
        else if (inItem.TryGetComponent(out HealthPack healthPack))
        {
            healthPack.OnStakeMode();
            itemPrice = healthPack.scrapValue;
            itemName = healthPack.itemName;
            itemDescription = healthPack.itemDescription;
            itemRarity = healthPack.rarity;
            itemCategory = healthPack.category;
        }
        
        col.enabled = false;
    }

    //아이템 구매가 가능한지 체크.
    private bool CheckItemValue()
    {
        if (GameManager.Instance.CurrentRunData.scrap >= itemPrice)
        {
            return true;
        }
        return false;
    }

    private bool SellItem(IInteractor interactor)
    {
        if (CheckItemValue()) GameManager.Instance.CurrentRunData.scrap -= itemPrice;
        else
        {
            Debug.Log("Scrap이 부족합니다.");
            return false;
        }
        
        if (inItem.TryGetComponent(out ArtifactObject artifactObject))
        {
            artifactObject.GetComponent<Collider>().enabled = true;
            artifactObject.Interact(interactor);
        }
        else if (inItem.TryGetComponent(out MagCore magCore))
        {
            magCore.GetComponent<Collider>().enabled = true;
            magCore.Interact(interactor);
        }
        else if (inItem.TryGetComponent(out HealthPack healthPack))
        {
            healthPack.GetComponent<Collider>().enabled = true;
            healthPack.Interact(interactor);
        }
        else
        {
            Debug.Log("아이템에 접근할 수 없습니다.");
            return false;
        }
        
        return true;
    }

    public void Interact(IInteractor interactor)
    {
        if (SellItem(interactor))
        {
            var uiController =  UIManager.Instance.popupUIController.productUIController;
            uiController.HideUI();
            Destroy(gameObject);
        }
    }

    public void Select(Highlighter highlighter)
    {
        var uiController =  UIManager.Instance.popupUIController.productUIController;
        uiController.SetItemText(inItem, true);
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
        return this.gameObject;
    }
}
