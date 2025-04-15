using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum WeaponType
{
    None = 0,
    SwordKatana,
}

public class MagCore: MonoBehaviour, IInteractable
{
    [SerializeField] private WeaponType weaponType;
    
    private List<MeshRenderer> _renderers = new List<MeshRenderer>();

    private void Awake()
    {
        _renderers = GetComponentsInChildren<MeshRenderer>().ToList();
    }

    public void Interact(IInteractor interactor)
    {
        if (interactor.GetGameObject().TryGetComponent<PlayerController>(out var player))
        {
            player.SetCurrentWeapon(weaponType);
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
}
