using System;
using System.Collections.Generic;
using System.Linq;
using Highlighters;
using hvvan;
using Moon;
using UnityEngine;

public class ArtifactObject : MonoBehaviour, IInteractable
{
    [SerializeField] private ArtifactDataSO artifactDataSO;
    public ItemCategory category;
    public ItemRarity rarity;
    public Sprite icon;
    public string itemName;
    public string itemDescription;
    public int scrapValue;
    
    public Action onChooseItem;
    private Rigidbody rb;
    private Collider col;
    private bool _isStake;
    
    private List<MeshRenderer> _renderers = new List<MeshRenderer>();
    
    void Awake()
    {
        _renderers = GetComponentsInChildren<MeshRenderer>().ToList();
        rb = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();

        category = ItemCategory.Artifact;
        
        if(artifactDataSO != null) SetArtifactData(artifactDataSO);
    }

    public void SetArtifactData(ArtifactDataSO artifactDataSO)
    {
        this.artifactDataSO = artifactDataSO;
        icon = artifactDataSO.icon;
        gameObject.name = itemName = artifactDataSO.itemName;
        itemDescription = artifactDataSO.description;
        scrapValue = artifactDataSO.scrapValue;
    }
    
    public void SetItemClass((ItemCategory, ItemRarity) itemClass)
    {
        (category, rarity) = itemClass;
    }
    
    public void Interact(IInteractor interactor)
    {
        if (interactor.GetGameObject().TryGetComponent<PlayerController>(out var player))
        {
            UIManager.Instance.ShowArtifactInventoryUI(artifactDataSO);
            onChooseItem?.Invoke();
            Destroy(gameObject);
        }
    }
    
    public void Dismantle(IInteractor interactor)
    {
        //TODO: 아이템 재활용(판매,분해) 로직 수행.
        if (interactor.GetGameObject().TryGetComponent<PlayerController>(out var player))
        { 
            GameManager.Instance.CurrentRunData.scrap += scrapValue;
            UIManager.Instance.inGameUIController.currencyUIController.UpdateUI();
            _= GameManager.Instance.SaveData(Constants.CurrentRun);
            Dismantling();
        }
    }

    //제거 연출 넣는 곳
    private void Dismantling()
    {
        onChooseItem?.Invoke();
        Destroy(gameObject);
    }

    public void Select(Highlighter highlighter)
    {
        foreach (var crateRenderer in _renderers)
        {
            highlighter.Renderers.Add(new HighlighterRenderer(crateRenderer, 1));
        }
        
        _renderers.ForEach(render => render.material.color = Color.blue);
        
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
        
        _renderers.ForEach(render => render.material.color = Color.gray);
        
        var uiController =  UIManager.Instance.popupUIController.productUIController;
        uiController.HideUI();
    }

    public GameObject GetGameObject()
    {
        return gameObject != null ? gameObject : null;
    }

    public InteractType GetInteractType()
    {
        return InteractType.Loot;
    }

    public ArtifactDataSO GetArtifactData()
    {
        return artifactDataSO;
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
    