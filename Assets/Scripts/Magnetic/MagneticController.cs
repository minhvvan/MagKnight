using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using Managers;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class MagneticController : MagneticObject
{
    public MagneticObject targetMagneticObject;
    
    
    public Camera mainCamera;
    public LayerMask magneticLayer;
    public LayerMask enemyLayer;
    public float rayDistance;
    public float screenOffset;

    private CharacterController _characterController;

    private Vector3 _currentVelocity;
    private Vector3 playerPosOffset;
    private float _dragValue;
    
    private float _minDistance;
    private float _outBoundDistance;
    
    public float structSpeed;
    public float nonStructSpeed;

    private bool _isLongRelease;
    private bool _isShortRelease;
    
    private bool _isReleaseMagnetic; //Magnetic 사용키 눌렀을 때
    private bool _isDetectedMagnetic; //Ray에 MagneticObject감지 되었을 때
    private bool _isActivatedMagnetic; //자기력 로직 활성화 중일 때

    private List<MagneticObject> _magneticObjects = new();
    private bool _onSearchNearMagnetic;
    
    private bool _onGravityBreak; //자기력 붕괴 스킬 사용 시
    public float gravityBreakRange;

    private bool _onCounterPress;
    private float _counterPressTime;
    private float _counterPressRange;
    private float _counterPressPower;
    
    private void Awake()
    {
        //테스트용
        Cursor.lockState = CursorLockMode.Locked;
        
        Initialize();
    }
    
    
    public override void Initialize()
    {
        base.Initialize();
        
        //추후 SO로 받아서 설정하게 될 기본값들
        
        mainCamera = Camera.main;
        screenOffset = 0.15f;
        magneticLayer = LayerMask.NameToLayer("Magnetic");
        enemyLayer = LayerMask.NameToLayer("Enemy");
        
        magneticType = MagneticType.N;
        _characterController = GetComponent<CharacterController>();
        playerPosOffset = new Vector3(0,1f,0);
        
        _minDistance = 1f;
        _outBoundDistance = 8f;
        
        structSpeed = 20f;
        nonStructSpeed = 2f;
        _dragValue = 1.5f;

        gravityBreakRange = 4f;

        _counterPressTime = 0.15f;
        _counterPressRange = 1.5f;
        _counterPressPower = 10f;
    }
    
    //Q 짧게 눌렀을 때
    public void OnShortReleaseEnter()
    {
        _isShortRelease = true;
    }

    public void OnShortReleaseExit()
    {
        _isShortRelease = false;
    }

    //Q 길게 눌렀을 때
    public void OnLongReleaseEnter()
    {
        _isLongRelease = true;
    }

    public void OnLongReleaseExit()
    {
        _isLongRelease = false;
    }

    #region 타겟팅 시스템
    public void MagneticTargetCamera()
    {
        //RaycastHit hit;
        Ray mainCameraRay = new Ray(mainCamera.transform.position, mainCamera.transform.forward);
        Vector3 targetPoint = GetAdjustRayOrigin(mainCameraRay, transform.position);
        rayDistance = _outBoundDistance;

        float sphereRadius = GetDynamicSphereRadius(screenOffset, rayDistance);
        
        Ray sphereRay = new Ray(targetPoint, mainCamera.transform.forward);
        RaycastHit[] hits = Physics.SphereCastAll(sphereRay, sphereRadius, rayDistance,
            (1 << magneticLayer) | (1 << enemyLayer));
        if (hits.Length > 0)
        {
            RaycastHit bestHit = hits.OrderBy(h =>
            {
                //hit지점이 카메라 중심에서 얼마나 떨어져 있는지
                Vector3 toHit = h.point - mainCamera.transform.position;
                float angle = Vector3.Angle(mainCamera.transform.forward, toHit);
                return angle;
            }).First();
            Debug.DrawLine(sphereRay.origin, bestHit.point, Color.green);

            if (bestHit.transform.TryGetComponent(out MagneticObject magneticObject))
            {
                Debug.Log("CATCH : " + magneticObject.name);
                _isDetectedMagnetic = true;
                targetMagneticObject = magneticObject;
                return;
            }
        }
        
        _isDetectedMagnetic = false;
        targetMagneticObject = null;
    }

    //mainCamera가 바라보는 정면과 플레이어의 y축이 교차하는 지점을 보정해주는 함수.
    public Vector3 GetAdjustRayOrigin(Ray ray, Vector3 playerPosition)
    {
        Vector3 rayOrigin = ray.origin;
        Vector3 rayDir = ray.direction.normalized;
    
        Vector3 playerYAxisOrigin = playerPosition;
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

        // Ray 상의 점: ray.origin + sc * rayDir
        Vector3 closestPointOnRay = rayOrigin + sc * rayDir;

        return closestPointOnRay;
    }
    
    //카메라 뷰 시점에서 항상 SphereCast의 원형 범위가 일정하게 보이도록 동적으로 Radius를 조정해주는 함수.
    float GetDynamicSphereRadius(float screenOffset = 0.05f, float castDistance = 10f)
    {
        // 스크린 중심 기준 오프셋 위치 계산 (좌우)
        Vector3 screenCenter = new Vector3(0.5f, 0.5f, castDistance);
        Vector3 screenRightOffset = new Vector3(0.5f + screenOffset, 0.5f, castDistance);

        // 스크린 좌표 -> 월드 좌표
        Vector3 worldCenter = mainCamera.ViewportToWorldPoint(screenCenter);
        Vector3 worldOffset = mainCamera.ViewportToWorldPoint(screenRightOffset);

        // 월드 거리 기준 반지름
        float dynamicRadius = Vector3.Distance(worldCenter, worldOffset);
        return dynamicRadius;
    }
    
    

    #endregion

    #region 물리 효과
    //두 오브젝트 가까워짐
    public async UniTask OnApproach(MagneticObject target, Vector3? another = null)
    {
        var targetPos = target.transform.position;
        var playerPos = another ?? _characterController.transform.position;
        var modifyPlayerPos = another == null ? playerPos + playerPosOffset : playerPos;
        
        var distance = (targetPos - playerPos).magnitude;
        
        var duration = 0f;
        if (target.GetIsStructure()) 
            duration = distance / (structSpeed);
        else duration = distance / (nonStructSpeed);
        
        var elapsedTime = 0f;        

        if (!_onGravityBreak) _isActivatedMagnetic = true;
        //방향까지 가속
        while (elapsedTime < duration/2f)
        {
            if (!_onGravityBreak)
            {
                //동작도중 자석 능력 키 한번 더 눌렀을 때
                if (_isReleaseMagnetic) break;
            
                //자기력 붕괴스킬 활성화 시
                if (_onGravityBreak) break;
            }
            
            elapsedTime += Time.deltaTime;
            
            targetPos = target.transform.position;
            playerPos = another ?? _characterController.transform.position;
            modifyPlayerPos = another == null ? playerPos + playerPosOffset : playerPos;
            
            //구조물 여부에 따라 인력 주체가 달라진다.
            if (target.GetIsStructure())
            {
                var hangDirection= (targetPos - playerPos).normalized;
                var newMovement = hangDirection * (structSpeed * Time.deltaTime);
                 
                _currentVelocity = newMovement;
                
                _characterController.Move(newMovement);
            }
            else
            {
                var newPosition = Vector3.Lerp( targetPos, modifyPlayerPos, nonStructSpeed * Time.deltaTime);
                target.transform.position = (newPosition);
            }

            distance = (targetPos - modifyPlayerPos).magnitude;

            if (distance <= _minDistance)
            {
                break;
            }

            await UniTask.Yield();
        }
        
        //자기력 붕괴사용 시 true가 되지 않음.
        if (!_isActivatedMagnetic) return;
        
        _isActivatedMagnetic = false;
        _isDetectedMagnetic = false;

        //도착 후 관성
        if (target.GetIsStructure() && !_onGravityBreak)
        {
            while (_currentVelocity != Vector3.zero)
            {
                //동작도중 자석 능력 키 한번 더 눌렀을 때
                if (_isReleaseMagnetic) break;
                
                _currentVelocity = Vector3.Lerp(_currentVelocity, Vector3.zero, _dragValue * Time.deltaTime);
                _characterController.Move(_currentVelocity);

                await UniTask.Yield();
            }
        }
    }

    //두 오브젝트 멀어짐
    public async UniTask OnSeparation(MagneticObject target, Vector3? another = null)
    {
        var targetPos = target.transform.position;
        var playerPos = another ?? _characterController.transform.position;
        var modifyPlayerPos = another == null ? playerPos + playerPosOffset : playerPos;
        var distance = (targetPos - modifyPlayerPos).magnitude;
        var maxDistance = _onCounterPress ? _counterPressRange : _outBoundDistance;
        
        var duration = 0f;
        if (target.GetIsStructure()) 
            duration = distance / (structSpeed);
        else duration = distance / (nonStructSpeed);
        
        var elapsedTime = 0f;

        if (!_onGravityBreak || !_onCounterPress) _isActivatedMagnetic = true;
        //방향까지 가속
        while (elapsedTime < duration/2f)
        {
            if (!_onGravityBreak)
            {
                //동작도중 자석 능력 키 한번 더 눌렀을 때 + 반격 중이지 않을 때.
                if (_isReleaseMagnetic && !_onCounterPress) break;
            
                //스킬 활성화 시
                if (_onGravityBreak) break;
            }
            
            elapsedTime += Time.deltaTime;
            
            targetPos = target.transform.position;
            playerPos = another ?? _characterController.transform.position;
            modifyPlayerPos = another == null ? playerPos + playerPosOffset : playerPos;
            
            var destination = (targetPos - modifyPlayerPos).normalized * maxDistance;
    
            //구조물 여부에 따라 다른 액션
            if (target.GetIsStructure())
            {
                var backDirection = -(destination - modifyPlayerPos).normalized;
                var newMovement = backDirection * (structSpeed * Time.deltaTime);
                
                _currentVelocity = newMovement;
                
                _characterController.Move(newMovement);
            }
            else
            {
                var pressDirection = destination + modifyPlayerPos;
                var newPosition = Vector3.Lerp(targetPos, pressDirection, 
                    (_onCounterPress ? _counterPressPower : nonStructSpeed) * Time.fixedDeltaTime);
                target.transform.position = newPosition;
            }
            
            distance = (targetPos - modifyPlayerPos).magnitude;

            if (distance >= maxDistance)
            {
                break;
            }
            
            await UniTask.Yield();
        }

        //자기력 붕괴사용 시 true가 되지 않음.
        if (!_isActivatedMagnetic) return;
        
        _isActivatedMagnetic = false;
        _isDetectedMagnetic = false;
    }
    
    #endregion

    #region 기술(스킬)
    
    //패링
    public async UniTask OnCounterPress()
    {
        Debug.Log("IN");
        _onCounterPress = true;

        _onSearchNearMagnetic = true;
        await UniTask.WaitUntil(() => !_onSearchNearMagnetic);
        Debug.Log("Counter Press");
        foreach (var obj in _magneticObjects)
        {
            OnSeparation(obj).Forget();
        }
        await UniTask.Delay((int)(1000 * _counterPressTime));

        _onCounterPress = false;
    }
    
    
    //자기력 특이점 붕괴
    public async UniTask OnGravityBreak(MagneticObject target)
    {
        _onGravityBreak = true;
        
        _onSearchNearMagnetic = true;
        await UniTask.WaitUntil(() => !_onSearchNearMagnetic);
        Debug.Log("RELEASE ON GRAVITY BREAK");
        
        foreach (var obj in _magneticObjects)
        {
            Debug.DrawLine(target.transform.position, obj.transform.position, Color.green, 1f);
        }
        
        await UniTask.Delay(1000);
        
        foreach (var obj in _magneticObjects)
        {
            OnApproach(obj, target.transform.position).Forget();
        }

        _onGravityBreak = false;
    }

    //주변 MagneticObject를 찾는로직
    public void OnSearchNearMagnetic(MagneticObject target, float radius)
    {
        _magneticObjects?.Clear();

        RaycastHit[] hits = Physics.SphereCastAll(target.transform.position, radius , Vector3.up ,1f, 
            !_onCounterPress ? (1 << magneticLayer) | (1 << enemyLayer) : (1 << enemyLayer));
        if (hits.Length > 0)
        {
            foreach (var hit in hits)
            {
                if (hit.transform.TryGetComponent(out MagneticObject magneticObject) && !magneticObject.GetIsStructure())
                {
                    _magneticObjects.Add(magneticObject);
                }
            }
        }
        _onSearchNearMagnetic = false;
    }

    #endregion
    
    private void FixedUpdate()
    {
        //자석 능력 길게 키 입력 시
        if (_isReleaseMagnetic)
        {
            MagneticTargetCamera();
        }

        //자기력 붕괴 스킬 활성화로 주변 탐색
        if (_onSearchNearMagnetic)
        {
            //자기력 붕괴
            if (targetMagneticObject != null)
            {
                if(_onGravityBreak) OnSearchNearMagnetic(targetMagneticObject, gravityBreakRange);
                return;
            }
            
            //반격
            if(_onCounterPress)  OnSearchNearMagnetic(this, _counterPressRange);
        }
    }

    private void Update()
    {
        //임시 테스트용 키입력
        if (Input.GetKeyDown(KeyCode.Q))
        {
            _isReleaseMagnetic = true;
        }
        else if (Input.GetKeyUp(KeyCode.Q))
        {
            _isReleaseMagnetic = false;
            if (targetMagneticObject != null)
            {
                if (targetMagneticObject.GetMagneticType() != magneticType)
                {
                    Debug.Log("다른극");
                    OnApproach(targetMagneticObject).Forget();
                }
                else if (targetMagneticObject.GetMagneticType() == magneticType)
                {
                    Debug.Log("같은극");
                   OnSeparation(targetMagneticObject).Forget();
                }
            }
        }
        
        if (Input.GetKeyDown(KeyCode.B))
        {
            Debug.Log("B");
            if(!_onGravityBreak && !_onCounterPress) OnCounterPress().Forget();
        }
        
        if (Input.GetKeyDown(KeyCode.V))
        {
            Debug.Log("V");
            if (_isActivatedMagnetic)
            {
                if(!_onGravityBreak && !_onCounterPress) OnGravityBreak(targetMagneticObject).Forget();
            }
            
            SwitchMagneticType();
            Debug.Log("MagType => " + magneticType);
        }
    }
    
    private void OnDrawGizmos()
    {
#if UNITY_EDITOR
        if (!EditorApplication.isPlaying)
            return;
#endif
        Ray mainCameraRay = new Ray(mainCamera.transform.position, mainCamera.transform.forward);
        Vector3 targetPoint = GetAdjustRayOrigin(mainCameraRay, transform.position);
        
        if (_isReleaseMagnetic)
        {
            if (targetMagneticObject != null)
            {
                switch (targetMagneticObject.magneticType)
                {
                    case MagneticType.N:
                        Gizmos.color = Color.red;
                        break;
                    case MagneticType.S:
                        Gizmos.color = Color.blue;
                        break;
                }
            }
            else Gizmos.color = Color.yellow;
        }
        else Gizmos.color = Color.white;

        Gizmos.DrawRay(targetPoint, mainCamera.transform.forward * rayDistance);
        Gizmos.DrawWireSphere(targetPoint, GetDynamicSphereRadius(screenOffset, rayDistance));
        Gizmos.DrawWireSphere(_characterController.transform.position, _outBoundDistance);

        if (_onCounterPress)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(transform.position, _counterPressRange); 
        }
        else if (!_onCounterPress)
        {
            Gizmos.color = Color.clear;
            Gizmos.DrawWireSphere(transform.position, _counterPressRange); 
        }
        
    }
}
