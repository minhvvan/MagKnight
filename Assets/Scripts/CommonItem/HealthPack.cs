using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Moon;
using UnityEngine;
using UnityEngine.Serialization;

public class HealthPack : MonoBehaviour, IInteractable
{
    private HealthPackSO healthPackSo;
    public string name;
    public string description;
    public float value;
    
    private List<MeshRenderer> _renderers = new List<MeshRenderer>();
    public Action onChooseItem;
    private Rigidbody rb;
    private Collider col;
    
    private void Awake()
    {
        _renderers = GetComponentsInChildren<MeshRenderer>().ToList();
        rb = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();
    }

    public void SetHealthPotionData(HealthPackSO healthPackData)
    {
        healthPackSo = healthPackData;
        gameObject.name = name = healthPackSo.itemName;
        description = healthPackSo.description;
        value = healthPackSo.value;
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
            rb.isKinematic = true;
            col.isTrigger = true;
        }
    }
}
