using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cinemachine;
using Cysharp.Threading.Tasks;
using Highlighters;
using hvvan;
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
    
    private Highlighter _interactHighlighter;
    
    private Camera _mainCamera;
    private RaycastHit[] _hits = new RaycastHit[10];

    private InteractIndicator _interactIndicator;
    
    private void Awake()
    {
        _interactor = this.GetInterfaceInParent<IInteractor>();
        cameraSettings = FindObjectOfType<CameraSettings>();
        _interactHighlighter = GetComponent<Highlighter>();
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
            _currentInteractable.UnSelect(_interactHighlighter);
        }
        
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
        if (UIManager.Instance.inGameUIController == null) return;
        
        var targetPoint = GetStartPoint();
        
        //범위 내 감지
        var hitCount = Physics.SphereCastNonAlloc(targetPoint, interactionRadius, _mainCamera.transform.forward, _hits, interactionDistance, interactableMask);
        
        if (hitCount <= 0)
        {
            if(_currentInteractable == null)
            {
                if(UIManager.Instance.popupUIController != null && UIManager.Instance.popupUIController.productUIController != null && UIManager.Instance.popupUIController.productUIController.gameObject.activeSelf)
                {
                    UIManager.Instance.popupUIController.productUIController.HideUI();
                }
            }
            else
            {
                _currentInteractable?.UnSelect(_interactHighlighter);
            }
            _currentInteractable = null;

            if (!_interactIndicator)
            {
                _interactIndicator = UIManager.Instance.inGameUIController.interactIndicator;
            }
            
            _interactIndicator?.InteractUnSelected();
            return;
        }
        
        foreach (var hit in _hits)
        {
            if (hit.collider.IsUnityNull()) continue;
            if (!hit.collider.TryGetComponent<IInteractable>(out var interactable)) continue;
            if(_currentInteractable == interactable) break;
            
            _currentInteractable?.UnSelect(_interactHighlighter);
            _currentInteractable = interactable;
            _currentInteractable?.Select(_interactHighlighter);
            
            if (!_interactIndicator)
            {
                _interactIndicator = UIManager.Instance.inGameUIController.interactIndicator;
            }

            _interactIndicator?.InteractSelected(_currentInteractable.GetInteractType());

            break;
        }
    }

    private void FixedUpdate()
    {
        //cursor가 잠겨있을때만 상호작용 감지
        if(Cursor.lockState == CursorLockMode.Locked){
            FindClosestInteractable();
        }
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
