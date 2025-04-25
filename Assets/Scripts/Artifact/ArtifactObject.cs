using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Moon;
using UnityEngine;
using UnityEngine.Serialization;

public class ArtifactObject : MonoBehaviour, IInteractable
{
    [SerializeField] private ArtifactDataSO artifactDataSO;
    public Sprite icon;
    public string itemName;
    
    public Action onChooseItem;
    private Rigidbody rb;
    private Collider col;
    private List<MeshRenderer> _renderers = new List<MeshRenderer>();
    
    void Awake()
    {
        _renderers = GetComponentsInChildren<MeshRenderer>().ToList();
        rb = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();
    }

    public void SetArtifactData(ArtifactDataSO artifactDataSO)
    {
        this.artifactDataSO = artifactDataSO;
        icon = artifactDataSO.icon;
        itemName = artifactDataSO.itemName;
        gameObject.name = itemName;
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
            rb.isKinematic = true;
            col.isTrigger = true;
        }
    }
}
    