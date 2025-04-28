using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cinemachine;
using Cysharp.Threading.Tasks;
using Moon;
using Unity.VisualScripting;
using UnityEngine;

public class InteractionController : MonoBehaviour
{
    [SerializeField] private float interactionDistance = 10f;
    [SerializeField] private float interactionRadius = .5f;
    [SerializeField] private LayerMask interactableMask;
    [SerializeField] public CameraSettings cameraSettings;
    
    private IInteractable _currentInteractable;
    private IInteractor _interactor;
    
    private Camera _mainCamera;
    private RaycastHit[] _hits = new RaycastHit[10];
    
    private void Awake()
    {
        _interactor = this.GetInterfaceInParent<IInteractor>();
        cameraSettings = FindObjectOfType<CameraSettings>();
        _mainCamera = Camera.main;
    }

    public void Interact(bool isDismantle = false)
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

        if (isDismantle)//아이템 분해
        {
            if (_currentInteractable.GetType() == typeof(MagCore))
            {
                if (_currentInteractable.GetGameObject().TryGetComponent(out MagCore magCore))
                {
                    magCore.Dismantle(_interactor);
                }
            }
            else if (_currentInteractable.GetType() == typeof(ArtifactObject))
            {
                if (_currentInteractable.GetGameObject().TryGetComponent(out ArtifactObject artifactObject))
                {
                    artifactObject.Dismantle(_interactor);
                }
            }
        }
        else
        {
            _currentInteractable.Interact(_interactor);
        }
        
        _currentInteractable.Interact(_interactor);
        FindClosestInteractable();

        _currentInteractable = null;
    }
    
    private Vector3 GetStartPoint()
    {
        Ray mainCameraRay = new Ray(_mainCamera.transform.position, _mainCamera.transform.forward);
        
        Vector3 rayOrigin = mainCameraRay.origin;
        Vector3 rayDir = mainCameraRay.direction.normalized;
    
        Vector3 playerYAxisOrigin = transform.position;
        Vector3 playerYAxisDir = Vector3.up;

        // 방향 벡터 내적
        Vector3 w0 = rayOrigin - playerYAxisOrigin;

        float a = Vector3.Dot(rayDir, rayDir); // = 1 if rayDir is normalized
        float b = Vector3.Dot(rayDir, playerYAxisDir);
        float c = Vector3.Dot(playerYAxisDir, playerYAxisDir); // = 1 since Vector3.up
        float d = Vector3.Dot(rayDir, w0);
        float e = Vector3.Dot(playerYAxisDir, w0);

        float denominator = a * c - b * b;

        if (Mathf.Abs(denominator) < 0.0001f)
        {
            // 거의 평행하므로 그냥 ray origin 반환
            return rayOrigin;
        }

        float sc = (b * e - c * d) / denominator;

        Vector3 targetPoint = rayOrigin + sc * rayDir;
        
        return targetPoint;
    }
    
    private void FindClosestInteractable()
    {
        if (!_mainCamera) return;
        var targetPoint = GetStartPoint();
        
        //범위 내 감지
        var hitCount = Physics.SphereCastNonAlloc(targetPoint, interactionRadius, _mainCamera.transform.forward, _hits, interactableMask);
        
        if (hitCount <= 0)
        {
            _currentInteractable?.UnSelect();
            _currentInteractable = null;
            return;
        }
        
        foreach (var hit in _hits)
        {
            if (hit.collider.IsUnityNull()) continue;
            if (!hit.collider.TryGetComponent<IInteractable>(out var interactable)) continue;
            
            _currentInteractable?.UnSelect();
            _currentInteractable = interactable;
            _currentInteractable?.Select();

            break;
        }
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

            InteractionEvent.OnDialogueEnd += InteractEnd;
            InteractionEvent.DialogueStart();
        }
    }

    public void InteractEnd()
    {
        EndDialogue(cameraSettings.interactionCamera);
        InteractionEvent.OnDialogueEnd -= InteractEnd;
    }

    private void FixedUpdate()
    {
        FindClosestInteractable();
    }

    private void FocusOnTarget(CinemachineVirtualCamera interactionCamera, Transform target, Transform lookFrom = null)
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

    private void OnDrawGizmos()
    {
#if false

        if (!Application.isPlaying) return;
        
        var start = GetStartPoint();
        var end = start + _mainCamera.transform.forward * interactionDistance;
        Gizmos.color = Color.yellow;
        
        Gizmos.DrawWireSphere(start, interactionRadius);
        Gizmos.DrawWireSphere(end, interactionRadius);
            
        // 캡슐 측면 연결선
        DrawCapsuleLines(start, end);
#endif
    }
    
    // 캡슐 측면 연결선을 그리는 보조 함수
    private void DrawCapsuleLines(Vector3 start, Vector3 end)
    {
        // 방향에 수직인 두 벡터 찾기
        Vector3 perpendicular1 = Vector3.zero;
        Vector3 perpendicular2 = Vector3.zero;
        
        var direction = end - start;
        
        if (direction == Vector3.up || direction == Vector3.down)
        {
            perpendicular1 = Vector3.forward;
            perpendicular2 = Vector3.right;
        }
        else if (direction == Vector3.forward || direction == Vector3.back)
        {
            perpendicular1 = Vector3.up;
            perpendicular2 = Vector3.right;
        }
        else
        {
            perpendicular1 = Vector3.up;
            perpendicular2 = Vector3.forward;
        }
        
        // 네 개의 측면 선 그리기
        Gizmos.DrawLine(start + perpendicular1 * interactionRadius, end + perpendicular1 * interactionRadius);
        Gizmos.DrawLine(start - perpendicular1 * interactionRadius, end - perpendicular1 * interactionRadius);
        Gizmos.DrawLine(start + perpendicular2 * interactionRadius, end + perpendicular2 * interactionRadius);
        Gizmos.DrawLine(start - perpendicular2 * interactionRadius, end - perpendicular2 * interactionRadius);
    }
}
