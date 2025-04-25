using System;
using System.Collections.Generic;
using System.Linq;
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
    public Sprite icon;
    public string itemName;
    
    private MagCoreSO magCoreSO;
    [SerializeField] private WeaponType weaponType;
    [SerializeField] private PartsType partsType;
    
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

    public void SetMagCoreData(MagCoreSO magCoreData)
    {
        magCoreSO = magCoreData;
        weaponType = magCoreSO.weaponType;
        partsType = magCoreSO.partsType;
        icon = magCoreSO.icon;
        gameObject.name = itemName = magCoreSO.itemName;
    }

    public void Interact(IInteractor interactor)
    {
        if (interactor.GetGameObject().TryGetComponent<PlayerController>(out var player))
        {
            //TODO: 무기교체 로직 추가 수행
            player.SetCurrentWeapon(weaponType);
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
