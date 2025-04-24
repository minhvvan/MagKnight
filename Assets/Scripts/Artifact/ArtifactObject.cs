using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Moon;
using UnityEngine;
using UnityEngine.Serialization;

public class ArtifactObject : MonoBehaviour, IInteractable
{
    [SerializeField] private ArtifactDataSO artifactDataSO;
    
    private List<MeshRenderer> _renderers = new List<MeshRenderer>();


    void Awake()
    {
        _renderers = GetComponentsInChildren<MeshRenderer>().ToList();
    }

    public void SetArtifactData(ArtifactDataSO artifactDataSO)
    {
        this.artifactDataSO = artifactDataSO;
        gameObject.name = this.artifactDataSO.itemName;
    }
    
    public void Interact(IInteractor interactor)
    {
        if (interactor.GetGameObject().TryGetComponent<PlayerController>(out var player))
        {
            UIManager.Instance.ShowArtifactInventoryUI(artifactDataSO);
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
