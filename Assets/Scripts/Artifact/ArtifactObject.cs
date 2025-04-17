using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Moon;
using UnityEngine;

public class ArtifactObject : MonoBehaviour, IInteractable
{
    // 추후 지워야함
    public ArtifactInventoryUI artifactInventoryUI;
    
    [SerializeField] private ArtifactDataSO artifactDataSO;
    
    private List<MeshRenderer> _renderers = new List<MeshRenderer>();


    void Awake()
    {
        _renderers = GetComponentsInChildren<MeshRenderer>().ToList();
    }
    
    public void Interact(IInteractor interactor)
    {
        if (interactor.GetGameObject().TryGetComponent<PlayerController>(out var player))
        {
            if(artifactInventoryUI != null)
                artifactInventoryUI.Show(artifactDataSO);
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
        return gameObject;
    }
}
