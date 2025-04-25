using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using hvvan;
using Moon;
using UnityEngine;
using UnityEngine.Serialization;

public class ArtifactObject : MonoBehaviour, IInteractable
{
    [SerializeField] private ArtifactDataSO artifactDataSO;
    public ItemCategory category;
    public ItemRarity rarity;
    public Sprite icon;
    public string itemName;
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
    }

    public void SetArtifactData(ArtifactDataSO artifactDataSO)
    {
        this.artifactDataSO = artifactDataSO;
        icon = artifactDataSO.icon;
        gameObject.name = itemName = artifactDataSO.itemName;
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
            var scrap = GameManager.Instance.CurrentRunData.scrap += scrapValue;
            Debug.Log("Scrap:" + scrap);
            Dismantling();
        }
    }

    //제거 연출 넣는 곳
    private void Dismantling()
    {
        Destroy(gameObject);
    }

    public void Select()
    {
        //TODO: outline
        _renderers.ForEach(render => render.material.color = Color.blue);
    }

    public void UnSelect()
    {
        //TODO: outline 제거
        _renderers.ForEach(render => render.material.color = Color.gray);
    }

    public GameObject GetGameObject()
    {
        return gameObject != null ? gameObject : null;
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
    