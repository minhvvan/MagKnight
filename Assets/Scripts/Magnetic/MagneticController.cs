using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MagneticController : MagneticObject
{
    private float _minDistance;
    private float _outBoundDistance;
    public float moveSpeed;


    private bool _isLongRelease;
    private bool _isShortRelease;
    
    public MagneticObject TestmagneticObject;
    public Camera mainCamera;
    
    private void Awake()
    {
        Initialize();
    }
    
    public override void Initialize()
    {
        base.Initialize();
        _minDistance = 1f;
        _outBoundDistance = 6f;
        moveSpeed = 2f;
    }

    public override void SetPhysic()
    {
        base.SetPhysic();
        objectMass = 1f;
        
        rigidbody.constraints  = RigidbodyConstraints.FreezeRotationX | 
                                 RigidbodyConstraints.FreezeRotationY | 
                                 RigidbodyConstraints.FreezeRotationZ;
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

    #region 타겟 카메라

    public void MagneticTargetCamera()
    {
        RaycastHit hit;
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out hit, _outBoundDistance))
        {
            if (hit.transform.TryGetComponent(out MagneticObject magneticObject))
            {
                
            }
        }
    }

    public void ReadyTarget()
    {
        
    }

    public void LockTarget()
    {
        
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawLine(mainCamera.transform.position, mainCamera.transform.position + new Vector3(0,0,_outBoundDistance));
    }
    

    #endregion

    #region 물리 효과
    
    //인력 작용
    public void Gravitation(Transform target, bool isStructure)
    {
        
    }

    //척력 작용
    public void Repulsion(Transform target, bool isStructure)
    {
        
    }
    
    //두 오브젝트 가까워짐
    public IEnumerator OnApproach(MagneticObject target)
    {
        var targetPos = target.transform.position;
        
        float distance = (targetPos - transform.position).magnitude;
        float duration = distance / moveSpeed;
        float elapsedTime = 0f;

        while (distance > _minDistance)
        {
            //TODO: 경로 단선 대비용 기능 -> ETA시간 초과시 자동종료 로직 추가하기
            
            targetPos = target.transform.position;

            //구조물 여부에 따라 인력 주체가 달라진다.
            var newPosition = Vector3.Lerp(
                target.isStructure ? transform.position : targetPos, 
                target.isStructure ? targetPos :transform.position, 
                2 * Time.fixedDeltaTime);

            if (target.isStructure) rigidbody.MovePosition(newPosition);
            else target.rigidbody.MovePosition(newPosition);

            distance = (targetPos - transform.position).magnitude;
            
            if (distance > _minDistance)
            {
                if(target.isStructure) rigidbody.velocity = Vector3.zero;
                else target.rigidbody.velocity = Vector3.zero;
            }

            yield return null;
        }
    }

    //두 오브젝트 멀어짐
    public IEnumerator OnSeparation(MagneticObject target)
    {
        var targetPos = target.transform.position;
        float distance = (targetPos - transform.position).magnitude;
        var direction = (targetPos - transform.position).normalized;
        
        while (distance < _outBoundDistance)
        {
            targetPos = target.transform.position;
            
            //var newDistance = _outBoundDistance - distance;
            
            var destination = direction * _outBoundDistance;
            var newPosition = Vector3.Lerp(targetPos, destination, 2 * Time.fixedDeltaTime);
            target.rigidbody.MovePosition(newPosition);
            

            distance = (targetPos - transform.position).magnitude;
            if (distance > _minDistance)
            {
                target.rigidbody.velocity = Vector3.zero;
            }
            
            yield return null;
        }
    }

    private void CheckEqualPole()
    {
        
    }
    

    #endregion
    

    //임시 테스트용
    private void FixedUpdate()
    {
        
        
       
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            Debug.Log("Q");

            if (TestmagneticObject.magneticType != magneticType)
            {
                Debug.Log("다른극");
                StartCoroutine(OnApproach(TestmagneticObject));
            }
            else if (TestmagneticObject.magneticType == magneticType)
            {
                Debug.Log("같은극");
                StartCoroutine(OnSeparation(TestmagneticObject));
            }
            
        }
        else if (Input.GetKeyUp(KeyCode.Q))
        {
            
        }
        
        if (Input.GetKeyDown(KeyCode.V))
        {
            Debug.Log("V");
            SwitchMagneticType();
            Debug.Log("MagType => " + magneticType);
        }
    }
}
