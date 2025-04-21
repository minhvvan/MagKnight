using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseNPCController : MonoBehaviour, IInteractable
{
    [SerializeField] public NPCSO npcSO;

    public Transform player;
    public Transform headTransform;
    Animator animator;

    [Range(0, 1)]
    public float lookWeight = 0.7f;
    [Range(0, 1)]
    public float rightHandWeight = 1f;

    public float detectDistance = 3.0f;
    bool _isInteract = false;

    float _currentLookWeight = 0.0f;
    float _targetLookWeight = 0.0f;

    float _currentRightHandWeight = 0.0f;
    float _targetRightHandWeight = 0.0f;

    Vector3 _lastTargetPosition = Vector3.zero;

    void Start()
    {
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        IKWeightHandler();
    }

    void IKWeightHandler()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, detectDistance);
        bool isPlayerInRange = false;
        foreach (Collider collider in colliders)
        {
            if (collider.CompareTag("Player"))
            {
                player = collider.transform;
                isPlayerInRange = true;
                break;
            }
        }

        if (!isPlayerInRange)
        {
            player = null;
            InteractExit();
        }

        if (player != null)
        {
            if (_isInteract)
            {
                Vector3 direction = player.position - transform.position;
                direction.y = 0; // Y축 회전 방지
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 5f);
            }
        }

        _targetLookWeight = (player != null) ? lookWeight : 0.0f;
        _currentLookWeight = Mathf.Lerp(_currentLookWeight, _targetLookWeight, Time.deltaTime * 3f);

        _targetRightHandWeight = _isInteract ? rightHandWeight : 0.0f;
        _currentRightHandWeight = Mathf.Lerp(_currentRightHandWeight, _targetRightHandWeight, Time.deltaTime * 3f);
    }

    void OnAnimatorIK(int layerIndex)
    {
        if(player == null && _lastTargetPosition == Vector3.zero) return;
        

        animator.SetLookAtWeight(_currentLookWeight);
        animator.SetIKPositionWeight(AvatarIKGoal.RightHand, _currentRightHandWeight);

        if (player != null)
        {
            _lastTargetPosition = player.position;
        }

        animator.SetLookAtPosition(_lastTargetPosition + Vector3.up * 1.6f);
        animator.SetIKPosition(AvatarIKGoal.RightHand, _lastTargetPosition + Vector3.up * 0.8f);
    }

    public void Interact(IInteractor interactor)
    {
        InteractEnter();
        InteractionEvent.OnDialogueEnd += InteractExit;
    }

    public void InteractEnter()
    {
        _isInteract = true;
    }

    public void InteractExit()
    {
        _isInteract = false;
        InteractionEvent.OnDialogueEnd -= InteractExit;
    }

    public void Select()
    {
        
    }

    public void UnSelect()
    {
        
    }

    public GameObject GetGameObject()
    {
        return gameObject;
    }

    public Transform GetHeadTransform()
    {
        return headTransform ? headTransform : transform;
    }
}
