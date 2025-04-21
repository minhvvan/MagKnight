using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cinemachine;
using Cysharp.Threading.Tasks;
using Moon;
using UnityEngine;
using VFolders.Libs;

public class InteractionController : MonoBehaviour
{
    [SerializeField] private float viewAngle = 90f;
    [SerializeField] private LayerMask obstacleMask;
    [SerializeField] public CameraSettings cameraSettings;
    
    private List<IInteractable> _interactables = new List<IInteractable>();
    private IInteractable _currentInteractable;
    private IInteractor _interactor;
    

    private void Awake()
    {
        _interactor = this.GetInterfaceInParent<IInteractor>();
        cameraSettings = FindObjectOfType<CameraSettings>();
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
        
        
        InteractStart();
        
        _currentInteractable.Interact(_interactor);
        //_interactables.Remove(_currentInteractable);
        FindClosestInteractable();

        _currentInteractable = null;
    }

    //NPC와의 인터렉션 시작
    public void InteractStart()
    {
        if(_currentInteractable is BaseNPCController npc)
        {
            Transform playerHead = null;

            if(_interactor is PlayerController player)
            {
                playerHead = player.cameraSettings.lookAt;    
            }
            FocusOnTarget(cameraSettings.interactionCamera,  npc.GetHeadTransform(), playerHead);

            //NPC가 대화가 가능할 경우 대화창을 열고 대화를 진행
            UIManager.Instance.inGameUIController.HideInGameUI();
            UIManager.Instance.inGameUIController.ShowDialogUI(npc.npcSO);
        }

        // //TEST 3초 후 대화 종료 : 대화장 만들고 상태 변화 적용 후 제거
        // UniTask.Delay(TimeSpan.FromSeconds(3)).ContinueWith(() =>
        // {
        //     EndDialogue(cameraSettings.interactionCamera);
        // });
        InteractionEvent.OnDialogueEnd += InteractEnd;
        InteractionEvent.DialogueStart();
    }


    public void InteractEnd()
    {
        EndDialogue(cameraSettings.interactionCamera);
        InteractionEvent.OnDialogueEnd -= InteractEnd;
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

    public void FocusOnTarget(CinemachineVirtualCamera interactionCamera, Transform target, Transform lookFrom = null)
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

    public void EndDialogue(CinemachineVirtualCamera interactionCamera)
    {
        interactionCamera.Priority = 0;
        UIManager.Instance.inGameUIController.ShowInGameUI();
    }
}
