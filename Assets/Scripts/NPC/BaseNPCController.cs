using System;
using System.Collections.Generic;
using System.Linq;
using Cinemachine;
using Highlighters;
using Moon;
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
    protected bool _isInteract = false;

    float _currentLookWeight = 0.0f;
    float _targetLookWeight = 0.0f;

    float _currentRightHandWeight = 0.0f;
    float _targetRightHandWeight = 0.0f;

    Vector3 _lastTargetPosition = Vector3.zero;
    
    protected IInteractor _currentInteractor;
    
    private List<Renderer> _renderers = new List<Renderer>();

    protected virtual void Awake()
    {
        animator = GetComponent<Animator>();
        _renderers = GetComponentsInChildren<Renderer>().ToList();
    }

    protected virtual void Update()
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

    public virtual void Interact(IInteractor interactor)
    {
        InteractEnter(interactor);
        InteractionEvent.OnDialogueEnd += InteractExit;
    }

    protected virtual void InteractEnter(IInteractor interactor)
    {
        if(interactor is not PlayerController playerCharacter) return;

        _currentInteractor = interactor;
        _isInteract = true;

        StartDialogue(playerCharacter, 0);
    }
    
    protected virtual void InteractExit()
    {
        if(_currentInteractor is not PlayerController playerCharacter) return;
        
        _isInteract = false;
        EndDialogue(playerCharacter.cameraSettings.interactionCamera);
        InteractionEvent.OnDialogueEnd -= InteractExit;
    }

    protected void StartDialogue(PlayerController playerCharacter, int dialogueID)
    {
        Transform playerHead = playerCharacter.cameraSettings.lookAt;
        FocusOnTarget(playerCharacter.cameraSettings.interactionCamera,  GetHeadTransform(), playerHead);

        //NPC가 대화가 가능할 경우 대화창을 열고 대화를 진행
        UIManager.Instance.inGameUIController.HideInGameUI();
        UIManager.Instance.inGameUIController.ShowDialogUI(npcSO.dialogueData[dialogueID]);

        InteractionEvent.OnDialogueEnd += InteractExit;
        InteractionEvent.DialogueStart();

        
    }
    
    protected void EndDialogue(CinemachineVirtualCamera interactionCamera)
    {
        interactionCamera.Priority = 0;
        UIManager.Instance.inGameUIController.ShowInGameUI();
        
        
    }
    
    protected void FocusOnTarget(CinemachineVirtualCamera interactionCamera, Transform target, Transform lookFrom = null)
    {
        if(lookFrom == null)
        {
            Vector3 offset = target.forward * 1.5f;
            interactionCamera.transform.position = target.position + offset;
        }
        else
        {
            Vector3 offset = lookFrom.forward * 0.5f;
            interactionCamera.transform.position = lookFrom.position + offset;
        }

        interactionCamera.LookAt = target;
        interactionCamera.Priority = 20;

        var composer = interactionCamera.GetCinemachineComponent<CinemachineComposer>();
        if (composer != null)
        {
            composer.m_ScreenX = 0.2f;
            composer.m_ScreenY = 0.55f;
        }
    }

    public void Select(Highlighter highlighter)
    {
        foreach (var crateRenderer in _renderers)
        {
            highlighter.Renderers.Add(new HighlighterRenderer(crateRenderer, 1));
        }
    }

    public void UnSelect(Highlighter highlighter)
    {
        foreach (var crateRenderer in _renderers)
        {
            highlighter.Renderers.Remove(new HighlighterRenderer(crateRenderer, 1));
        }
    }

    public GameObject GetGameObject()
    {
        return gameObject;
    }

    public InteractType GetInteractType()
    {
        return InteractType.Dialogue;
    }

    protected Transform GetHeadTransform()
    {
        return headTransform ? headTransform : transform;
    }
}
