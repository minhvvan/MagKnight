using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
    public Transform crateAxis;
    public Transform[] itemPoint;
    public int maxSpawnCount;
    
    private bool _isOpen = false;
    private Vector3 _openAngle;
    
    private List<MeshRenderer> _renderers = new List<MeshRenderer>();

    private void Awake()
    {
        _renderers = GetComponentsInChildren<MeshRenderer>().ToList();
        var rb = GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionZ |
                         RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ | 
                         RigidbodyConstraints.FreezeRotationY;
        _openAngle = new Vector3(-90f,0,0);
        
        if(maxSpawnCount == 0) maxSpawnCount = 3;
    }

    private void OpenCrate()
    {
        _isOpen = true;
        crateAxis.DORotate(_openAngle, 1f).OnComplete(() =>
        {
            for (int i = 0; i < maxSpawnCount; i++)
            {
                var item =ItemManager.Instance.CreateItem
                    (crateCategory,crateRarity,itemPoint[i].position,Quaternion.identity);
            }
        });
    }

    public void SetLootCrate(ItemCategory category, ItemRarity rarity)
    {
        crateCategory = category;
        crateRarity = rarity;
    }

    public void Interact(IInteractor interactor)
    {
        if (interactor.GetGameObject().TryGetComponent<PlayerController>(out var player))
        {
            if (!_isOpen) OpenCrate();
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
