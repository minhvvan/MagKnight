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

    private void Awake()
    {
        _renderers = GetComponentsInChildren<MeshRenderer>().ToList();
    }

    public void SetHealthPotionData(HealthPackSO healthPackData)
    {
        healthPackSo = healthPackData;
        name = healthPackSo.itemName;
        description = healthPackSo.description;
        value = healthPackSo.value;
    }

    public void Interact(IInteractor interactor)
    {
        if (interactor.GetGameObject().TryGetComponent<PlayerController>(out var player))
        {
            //player.SetCurrentWeapon(weaponType);
            
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
