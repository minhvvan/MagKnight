using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MagCore: MonoBehaviour, IInteractable
{
    private List<MeshRenderer> _renderers = new List<MeshRenderer>();

    private void Awake()
    {
        _renderers = GetComponentsInChildren<MeshRenderer>().ToList();
    }

    public void Interact()
    {
        Debug.Log("MagCore Interact");
        Destroy(gameObject);
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
