using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using VFolders.Libs;

public class InteractionController : MonoBehaviour
{
    private List<IInteractable> _interactables = new List<IInteractable>();
    private IInteractable _currentInteractable;
    private IInteractor _interactor;

    private void Awake()
    {
        _interactor = this.GetInterfaceInParent<IInteractor>();
    }

    public void Interact()
    {
        if (_currentInteractable == null)
        {
            Debug.LogError("currentInteractable is null");
            return;
        }
        
        if (_interactor == null)
        {
            Debug.LogError("interactor is null");
            return;
        }
        
        _currentInteractable.Interact(_interactor);
        _interactables.Remove(_currentInteractable);
        FindClosestInteractable();
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<IInteractable>(out var interactable))
        {
            //*Visual Test(나중에 삭제)
            interactable.GetGameObject().GetComponentsInChildren<MeshRenderer>().ForEach(mesh => mesh.material.color = Color.red);
            
            _interactables.Add(interactable);
            FindClosestInteractable();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent<IInteractable>(out var interactable))
        {
            if (_interactables.Contains(interactable))
            {
                //*Visual Test(나중에 삭제)
                interactable.GetGameObject().GetComponentsInChildren<MeshRenderer>().ForEach(mesh => mesh.material.color = Color.gray);

                _interactables.Remove(interactable);
                FindClosestInteractable();
            }
        }
    }

    private void FixedUpdate()
    {
        FindClosestInteractable();
    }

    private void FindClosestInteractable()
    {
        IInteractable closest = null;
        float closestDistanceSqr = float.MaxValue;

        foreach (var obj in _interactables)
        {
            float sqrDistance = (transform.position - obj.GetGameObject().transform.position).sqrMagnitude;
            if (sqrDistance < closestDistanceSqr)
            {
                closestDistanceSqr = sqrDistance;
                closest = obj;
            }
        }

        if (closest == _currentInteractable) return;
        
        _currentInteractable?.UnSelect();
        
        //*Visual Test(나중에 삭제)
        if (_interactables.Contains(_currentInteractable))
        {
            _currentInteractable?.GetGameObject().GetComponentsInChildren<MeshRenderer>().ForEach(mesh => mesh.material.color = Color.red);
        }
        
        _currentInteractable = closest;
        _currentInteractable?.Select();
    }
}
