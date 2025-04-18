using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class InteractionController : MonoBehaviour
{
    [SerializeField] private float viewAngle = 90f;
    [SerializeField] private LayerMask obstacleMask;
    
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
            Debug.Log("currentInteractable is null");
            return;
        }
        
        if (_interactor == null)
        {
            Debug.Log("interactor is null");
            return;
        }
        
        _currentInteractable.Interact(_interactor);
        _interactables.Remove(_currentInteractable);
        FindClosestInteractable();

        _currentInteractable = null;
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
        
        _interactables = _interactables.OrderBy(interactable => 
        {
            Vector3 dirToTarget = (interactable.GetGameObject().transform.position - transform.position).normalized;
            float angle = Vector3.Angle(transform.forward, dirToTarget);
            float distance = Vector3.Distance(transform.position, interactable.GetGameObject().transform.position);
    
            // 시야각 체크
            bool isInViewAngle = angle < viewAngle / 2;
            bool noObstacle = !Physics.Raycast(transform.position, dirToTarget, distance, obstacleMask);
            bool isVisible = isInViewAngle && noObstacle;
    
            return (isVisible ? 0f : float.MaxValue) + distance;
        }).ToList();

        if (_interactables.Count == 0) return;
        closest = _interactables.First();
        if (closest == null) return;
        
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
