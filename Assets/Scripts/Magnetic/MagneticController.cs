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
using Quaternion = System.Numerics.Quaternion;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class MagneticController : MagneticObject
{
    //-- 타겟팅 시스템 --//
    private MagneticUIController _magneticUIController;
    public MagneticObject targetMagneticObject;
    
    public Camera mainCamera; //메인 카메라
    public Vector3 targetHit;
    public LayerMask magneticLayer; //Magnetic 레이어
    public LayerMask enemyLayer; //Enemy 레이어
    public LayerMask groundLayer; //Enemy 레이어
    public LayerMask environmentLayer; //Enemy 레이어
    public float rayDistance; //정면 인식 사거리
    public float screenOffset; //
    public float sphereRadius; //조준 범위 반지름
    public int maxInCount; //탐지 개체 최대 수
    
    
    //--자기력--//
    private CharacterController _characterController;
    private Vector3 _currentVelocity; // 현재 가속도
    private Vector3 playerPosOffset; // player position을 center정도 위치로 임시보정 해주는 값
    private float _dragValue; //가속 후 감속값
    
    private float _minDistance;//대상 오브젝트가 일정 거리 이내로 다가올 시 자기력 상호작용을 종료할 최소거리.
    private float _outBoundDistance;//자기력이 작용하는 최대 거리
    private float _hangAdjustValue;//Vector만 전달 시 y축 보정
    
    public float structSpeed; //구조물 대상 적용 속도
    public float nonStructSpeed; //비구조물 대상 적용 속도
    
    
    //--입력--//
    
    //길게,짧게 누르기
    private bool _isLongRelease;
    private bool _isShortRelease;
    
    //Magetic 상호작용
    private bool _isPressMagnetic; //Magnetic 사용키 눌렀을 때
    private bool _isDetectedMagnetic; //Ray에 MagneticObject감지 되었을 때
    private bool _isActivatedMagnetic; //자기력 로직 활성화 중일 때

    //주변 탐색
    private List<MagneticObject> _magneticObjects = new();
    private bool _onSearchNearMagnetic;
    
    
    //--기술--//
    
    //자기력 붕괴
    private bool _onGravityBreak; //자기력 붕괴 스킬 사용 시
    public float gravityBreakRange;

    //패링, 반격
    private bool _onCounterPress;
    private float _counterPressTime;
    private float _counterPressRange;
    private float _counterPressPower;
    
    private void Awake()
    {
        Initialize();
    }
    
    public override void Initialize()
    {
        base.Initialize();
        
        //추후 SO로 받아서 설정하게 될 기본값들
        _magneticUIController = FindObjectOfType<MagneticUIController>();
        maxInCount = _magneticUIController.poolSize;
        
        mainCamera = Camera.main;
        screenOffset = 0.15f;
        magneticLayer = LayerMask.NameToLayer("Magnetic");
        enemyLayer = LayerMask.NameToLayer("Enemy");
        groundLayer = LayerMask.NameToLayer("Ground");
        environmentLayer = LayerMask.NameToLayer("Environment");
        
        _characterController = GetComponent<CharacterController>();
        playerPosOffset = new Vector3(0,1f,0);
        
        _minDistance = 1f;
        _outBoundDistance = 15f;
        _hangAdjustValue = _outBoundDistance/10f+0.25f; //1.2~2f
        
        structSpeed = 6.5f;
        nonStructSpeed = 3f;
        _dragValue = 1.5f;

        gravityBreakRange = 4f;

        _counterPressTime = 0.15f;
        _counterPressRange = 1.5f;
        _counterPressPower = 10f;
    }

    #region 입력 처리

    //Q 누르기 유지
    public void OnPressEnter()
    {
        _isShortRelease = true;
        //입력 유지 시 할 로직
        
        //키 입력이 시작됨을 알림.
        _isPressMagnetic = true;
        
        _magneticUIController.ShowFocusArea();
        _magneticUIController.ShowMagneticTypeVisual(GetMagneticType());
        
        //끝
        _isShortRelease = false;
    }
    
    //Q 짧게 누르고 뗐을때
    public void OnShortRelease()
    {
        _isShortRelease = true;
        //짧게 입력시 할 로직
        _isPressMagnetic = false;
        _magneticUIController.HideFocusArea();
        _magneticUIController.HideMagneticTypeVisual();
        
        if(!_onGravityBreak && !_onCounterPress && !_onSearchNearMagnetic) OnCounterPress().Forget();
        
        //끝
        _isShortRelease = false;
    }

    //Q 길게 누르고 뗐을때
    public void OnLongRelease()
    {
        _isLongRelease = true;
        //길게 입력 시 할 로직
        _isPressMagnetic = false;
        _magneticUIController.HideFocusArea();
        _magneticUIController.HideMagneticTypeVisual();
        
        if (targetMagneticObject != null)
        {
            if (targetMagneticObject.GetMagneticType() != magneticType)
            {
                OnApproach(targetMagneticObject).Forget();
            }
            else if (targetMagneticObject.GetMagneticType() == magneticType)
            {
                OnSeparation(targetMagneticObject).Forget();
            }
            _magneticUIController.UnLockOnTarget(targetMagneticObject.transform);
        }
        //끝
        _isLongRelease = false;
    }
    
    //V, 극성전환
    public override void SwitchMagneticType(MagneticType? type = null)
    {
        //극성 전환 이전
        if (_isActivatedMagnetic)
        {
            //자기력 붕괴 스킬 사용
            if(!_onGravityBreak && !_onCounterPress & !_onSearchNearMagnetic) OnGravityBreak(targetMagneticObject).Forget();
        }
        
        base.SwitchMagneticType(type); //극성 전환
        
        //극성 전환 후
        if(_isPressMagnetic) _magneticUIController.ShowMagneticTypeVisual(GetMagneticType());

        ArtifactInventory inventory = GetComponent<ArtifactInventory>();
        if (inventory != null)
            inventory.ConvertArtifact();
    }

    #endregion

    #region 타겟팅 시스템

    // 카메라 중심에서 이어진 선과 플레이어 Y축의 교차점을 TargetPoint로 지정합니다. 동적으로 변화합니다.
    private Vector3 GetTargetPoint()
    {
        Ray mainCameraRay = new Ray(mainCamera.transform.position, mainCamera.transform.forward);
        Vector3 targetPoint = GetAdjustRayOrigin(mainCameraRay, transform.position);
        
        return targetPoint;
    }
    
    //실시간으로 플레이어 주변의 MagneticObject를 탐색합니다.
    private void ScanNearByMagneticTarget()
    {
        var targetPoint = GetTargetPoint();
        
        //범위 내 전체 감지
        var countHits = new Collider[maxInCount];
        var hitCount = Physics.OverlapSphereNonAlloc(targetPoint, _outBoundDistance, countHits, 
            (1 << magneticLayer) | (1 << enemyLayer));
        
        //감지 된 대상 중 카메라 시야 내에 있는 대상만 count
        if (countHits.Length <= 0) return;
        for (int i = 0; i < hitCount; i++)
        {
            InCountVisor(countHits[i].transform, targetPoint);
        }
    }

    //대상의 위치가 자기력 범위 내에 있는지 판단하여 할당과 제외를 판별합니다.
    private void InCountVisor(Transform inCount, Vector3 origin)
    {
        var objTransform = inCount.transform;
        var inCounter = mainCamera.WorldToScreenPoint(objTransform.position);
        if (Vector3.Distance(objTransform.position, origin) <= _outBoundDistance && inCounter.z >= 0)
        {
            _magneticUIController.InCountTarget(objTransform);
        }
        else
        {
            _magneticUIController.UnCountTarget(objTransform);
        }
    }
    
    //조준한 범위 내의
    private void FocusMagneticTarget()
    {
        var targetPoint = GetTargetPoint();
        
        sphereRadius = GetDynamicSphereRadius(screenOffset, Vector3.Distance(mainCamera.transform.position, targetPoint));
        rayDistance = _outBoundDistance - sphereRadius;
        
        //조준 범위만 감지
        Ray sphereRay = new Ray(targetPoint, mainCamera.transform.forward);
        RaycastHit[] hits = new RaycastHit[maxInCount];
        var hitCount = Physics.SphereCastNonAlloc(sphereRay, sphereRadius, hits, rayDistance, (1 << magneticLayer) | (1 << enemyLayer));
        
        if (hits.Length > 0)
        {
            if (hitCount > 0)
            {
                RaycastHit bestHit = hits.OrderBy(h =>
                {
                    //hit지점이 카메라 중심에서 얼마나 떨어져 있는지 검사.
                    Vector3 toHit = h.point - mainCamera.transform.position;
                    float angle = Vector3.Angle(mainCamera.transform.forward, toHit);
                    return angle;
                }).First();
                
                targetHit = bestHit.point;
                Debug.DrawLine(sphereRay.origin, bestHit.point, Color.green);
                
                if (bestHit.collider != null && 
                    bestHit.collider.transform.TryGetComponent(out MagneticObject magneticObject) 
                    && magneticObject != null)
                {
                    //새로 타겟된 대상이 이전과 다르면 언록
                    if (targetMagneticObject != null && magneticObject != targetMagneticObject)
                    {
                        _magneticUIController.UnLockOnTarget(targetMagneticObject.transform);
                        targetMagneticObject = null;
                    }
                
                    _isDetectedMagnetic = true;
                    targetMagneticObject = magneticObject;
                    _magneticUIController.InLockOnTarget(targetMagneticObject.transform);
                    
                    return;
                }
            }
        }

        if (targetMagneticObject != null)
        {
            _magneticUIController.UnLockOnTarget(targetMagneticObject.transform);
        }
        _isDetectedMagnetic = false;
        targetMagneticObject = null;
    }

    //mainCamera가 바라보는 정면과 플레이어의 y축이 교차하는 지점을 보정해주는 함수.
    private Vector3 GetAdjustRayOrigin(Ray ray, Vector3 playerPosition)
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
    private float GetDynamicSphereRadius(float screenOffset = 0.05f, float castDistance = 8f)
    {
        // 스크린 중심 기준 오프셋 위치 계산 (좌우)
        Vector3 screenCenter = new Vector3(0.5f, 0.5f, castDistance);
        Vector3 screenRightOffset = new Vector3(0.5f + screenOffset, 0.5f, castDistance);

        // 스크린 좌표 -> 월드 좌표
        Vector3 worldCenter = mainCamera.ViewportToWorldPoint(screenCenter);
        Vector3 worldOffset = mainCamera.ViewportToWorldPoint(screenRightOffset);

        // 월드 거리 기준 반지름
        float dynamicRadius = Vector3.Distance(worldCenter, worldOffset);
        return dynamicRadius*2f;
    }
    
    

    #endregion

    #region 물리 효과
    
    //상호 겹침 보정
    public void PenetrationFix(MagneticObject magneticObject)
    {
        var obj = magneticObject.gameObject;
        var col = magneticObject.GetComponent<Collider>();
        var center = col.bounds.center;
        var size  = col.bounds.size;
        
        var radius = Mathf.Min(size.x, size.z)/2f;
        var height = size.y;
        var halfHeight = Mathf.Max(height / 2f - radius, 0f);
        var up = Vector3.up;
        var point1 = center + up * halfHeight;
        var point2 = center - up * halfHeight;

        var hitColliders = new Collider[maxInCount];
        var hitCount = Physics.OverlapCapsuleNonAlloc(point1, point2, radius, hitColliders, 
            (1 << magneticLayer) | (1 << enemyLayer) | (1 << groundLayer) | (1 << environmentLayer));

        //감지한 대상 기반으로 보정 수행
        for (int i = 0; i < hitCount; i++)
        {
            Collider hitCol = hitColliders[i];
            
            //본인 제외
            if(hitCol == col) continue;
            
            //ComputePenetration
            if (Physics.ComputePenetration(col, obj.transform.position, obj.transform.rotation,
                    hitCol, hitCol.transform.position, hitCol.transform.rotation,
                    out Vector3 direction, out float distance))
            {
                //겹침(침투) 보정
                obj.transform.position += direction * distance;
            }
        }
    }
    
    //두 오브젝트 가까워짐
    public async UniTask OnApproach(MagneticObject target, Vector3? another = null)
    {
        var targetPos = target.transform.position;
        var playerPos = another ?? transform.position;
        var modifyPlayerPos = another == null ? playerPos + playerPosOffset : playerPos;
        var distance = (targetPos - playerPos).magnitude;
        var staticDistance = distance;
        
        var eta = 0f;
        if (target.GetIsStructure()) 
            eta = distance / (structSpeed);
        else eta = distance / (nonStructSpeed);
        
        var elapsedTime = 0f;
        var elapsedDistance = 0f;
        if (!_onGravityBreak) _isActivatedMagnetic = true;
        //방향까지 가속
        while (elapsedTime < eta) // eta/2f
        {
            if (!_onGravityBreak)
            {
                //동작도중 자석 능력 키 한번 더 눌렀을 때
                if (_isPressMagnetic) break;
            
                //자기력 붕괴스킬 활성화 시
                if (_onGravityBreak) break;
            }
            
            elapsedTime += Time.deltaTime;
            
            //Lerp로도 잘 어울려지도록 ETA 보정
            float progressTime = Mathf.Clamp01(elapsedTime / eta);

            targetPos = target.transform.position;
            playerPos = another ?? transform.position;
            modifyPlayerPos = another == null ? playerPos + playerPosOffset : playerPos;
            
            //구조물 여부에 따라 인력 주체가 달라진다.
            if (target.GetIsStructure())
            {
                var direction = (targetPos - modifyPlayerPos);
                direction.y *= _hangAdjustValue; //normalized로 인한 y값 감소에 대한 보정
                
                var hangDirection= direction.normalized;
                var newMovement = hangDirection * progressTime;
                 
                _currentVelocity = newMovement;
                
                _characterController.Move(newMovement);
                
                elapsedDistance += Vector3.Distance(playerPos+newMovement, playerPos);
            }
            else
            {
                var newPosition = Vector3.MoveTowards( targetPos, modifyPlayerPos, progressTime);
                target.transform.position = (newPosition);
                elapsedDistance += Vector3.Distance(targetPos, newPosition);
            }
            
            PenetrationFix(target);

            distance = (targetPos - modifyPlayerPos).magnitude;
            if (elapsedDistance >= staticDistance) break;
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
                //if (_isPressMagnetic) break;
                
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
        var backDistance = maxDistance - Vector3.Distance(targetPos, modifyPlayerPos);
        
        var direction = (targetPos - modifyPlayerPos).normalized;
        var destination = direction * maxDistance;
        var outDistance = Vector3.Distance(destination + modifyPlayerPos, targetPos);
        
        var eta = 0f;
        if (target.GetIsStructure()) 
            eta = backDistance / (structSpeed);
        else if (_onCounterPress)
        {
            eta = distance / _counterPressPower;
        }
        else
        {
            eta = outDistance / nonStructSpeed;
        }
        
        var elapsedTime = 0f;
        var elapsedDistance = 0f;

        if (!_onGravityBreak || !_onCounterPress) _isActivatedMagnetic = true;
        //방향까지 가속
        while (elapsedTime < eta)
        {
            if (!_onGravityBreak)
            {
                //동작도중 자석 능력 키 한번 더 눌렀을 때 + 반격 중이지 않을 때.
                if (_isPressMagnetic && !_onCounterPress) break;
            
                //스킬 활성화 시
                if (_onGravityBreak) break;
            }
            
            elapsedTime += Time.deltaTime;
            //Lerp로도 잘 어울려지도록 ETA 보정
            float progressTime = Mathf.Clamp01(elapsedTime / eta);
            
            targetPos = target.transform.position;
            playerPos = another ?? _characterController.transform.position;
            modifyPlayerPos = another == null ? playerPos + playerPosOffset : playerPos;
            
            destination = (targetPos - modifyPlayerPos).normalized * maxDistance;
    
            //구조물 여부에 따라 다른 액션
            if (target.GetIsStructure())
            {
                var backDirection = -destination.normalized;
                backDirection.y *= _hangAdjustValue; //normalized로 인한 y값 감소에 대한 보정
                
                var newMovement = (backDirection) * progressTime;
                
                _currentVelocity = newMovement;
                _characterController.Move(newMovement);
                elapsedDistance += Vector3.Distance(newMovement + playerPos, playerPos);
            }
            else
            {
                var pressDirection = destination + modifyPlayerPos;
                var newPosition = Vector3.MoveTowards(targetPos, pressDirection, progressTime);
                target.transform.position = newPosition;
                elapsedDistance += Vector3.Distance(targetPos, newPosition);
            }
            
            PenetrationFix(target);
            distance = (targetPos - modifyPlayerPos).magnitude;
            if (elapsedDistance >= outDistance) break;
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
        _onCounterPress = true;

        _onSearchNearMagnetic = true;
        await UniTask.WaitUntil(() => !_onSearchNearMagnetic);
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
        //실시간 자기력 범위 내 대상 탐색.
        ScanNearByMagneticTarget();
        
        //자석 능력 길게 키 입력 시
        if (_isPressMagnetic)
        {
            FocusMagneticTarget();
        }

        //자기력 붕괴 스킬 활성화로 주변 탐색
        if (_onSearchNearMagnetic)
        {
            //자기력 붕괴
            if(_onGravityBreak && targetMagneticObject != null) OnSearchNearMagnetic(targetMagneticObject, gravityBreakRange);
            
            //반격
            if(_onCounterPress)  OnSearchNearMagnetic(this, _counterPressRange);
        }
    }
    
    private void OnDrawGizmos()
    {
#if UNITY_EDITOR
        if (!EditorApplication.isPlaying)
            return;
        Ray mainCameraRay = new Ray(mainCamera.transform.position, mainCamera.transform.forward);
        Vector3 targetPoint = GetAdjustRayOrigin(mainCameraRay, transform.position);
        
        if (_isPressMagnetic)
        {
            if (targetMagneticObject != null)
            {
                switch (targetMagneticObject.magneticType)
                {
                    case MagneticType.N:
                        Gizmos.color = Color.red;
                        Handles.color = Color.red;
                        break;
                    case MagneticType.S:
                        Gizmos.color = Color.blue;
                        Handles.color = Color.blue;
                        break;
                }
            }
            else
            {
                Gizmos.color = Color.yellow;
                Handles.color = Color.yellow;
            }
        }
        else
        {
            Gizmos.color = Color.white;
            Handles.color = Color.clear;
        }

        Gizmos.DrawRay(targetPoint, mainCamera.transform.forward * _outBoundDistance);
        Gizmos.DrawWireSphere(_characterController.transform.position, _outBoundDistance);
        
        //타겟팅 대상 기즈모
        if (targetHit == Vector3.zero || targetMagneticObject == null)
        {
            Gizmos.color = Color.clear;
        }
        Gizmos.DrawWireSphere(targetHit,1f);
        
        Handles.DrawWireDisc(targetPoint, mainCamera.transform.forward, sphereRadius/2f);
        
        switch (magneticType)
        {
            case MagneticType.N:
                Handles.color = Color.red;
                break;
            case MagneticType.S:
                Handles.color = Color.blue;
                break;
        }
        if (!_isPressMagnetic) Handles.color = Color.clear;
        Handles.DrawWireDisc(targetPoint, mainCamera.transform.forward, (sphereRadius/2f)*1.15f);

        if (_onCounterPress)
        {
            Gizmos.color = Color.magenta;
        }
        else if (!_onCounterPress)
        {
            Gizmos.color = Color.clear;
        }
        Gizmos.DrawWireSphere(_characterController.transform.position + playerPosOffset, _counterPressRange);
#endif
    }
}
