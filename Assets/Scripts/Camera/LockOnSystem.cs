using System.Linq;
using UnityEngine;
using Cinemachine;
using Moon; // CameraSettings 네임스페이스

public class LockOnSystem : MonoBehaviour
{
    private PlayerController _playerController;
    private InputHandler _inputHandler;

    public LayerMask targetMask;
    public float searchRadius = 15f;

    [Header("회전 설정")]
    public float rotationSpeed = 5f;  // 회전 속도 조절

    // 대상 후보 목록 및 현재 락온 대상
    private Transform[] candidates = new Transform[0];
    public Transform currentTarget;

    [Header("입력 설정")]
    [Tooltip("마우스 이동으로 대상 변경 시 최소 화면 비율 값 (X축) ")]
    public float mouseSwitchMaxDist = 8f;
    [Tooltip("패드 우측 스틱 대상 변경 시 최소 입력 강도")]
    public float stickDeadzone = 0.2f;

    // 내부 스위치 상태
    private bool _switchedRight;
    private bool _switchedLeft;

    void Start()
    {
        _playerController = GetComponent<PlayerController>();
        _inputHandler = GetComponent<InputHandler>();
    }

    void Update()
    {
        // 1) 현재 락온 대상이 사망 또는 비활성화되면 다음 후보로 자동 스위치
        if (currentTarget != null)
        {
            if(currentTarget.TryGetComponent(out Enemy enemy))
            {
                if(enemy.blackboard.isDead)
                {
                    HandleDeadTarget();
                }
            }
        }

        // 2) 방향 입력만으로 대상 변경
        if (currentTarget != null)
        {
            // 카메라 입력(마우스 또는 우측 스틱)
            Vector2 camInput = _inputHandler.CameraInput;
            // 사용 중인 제어기 선택
            bool usingController = _playerController.cameraSettings.inputChoice == CameraSettings.InputChoice.Controller;
            float threshold = usingController ? stickDeadzone : mouseSwitchMaxDist;

            // 오른쪽으로 입력했을 때
            if (camInput.x > threshold)
            {
                if (!_switchedRight)
                {
                    CycleTarget(1);
                    _switchedRight = true;
                }
            }
            // 왼쪽으로 입력했을 때
            else if (camInput.x < -threshold)
            {
                if (!_switchedLeft)
                {
                    CycleTarget(-1);
                    _switchedLeft = true;
                }
            }
            else
            {
                // 입력이 데드존으로 돌아오면 다시 스위치 허용
                _switchedRight = false;
                _switchedLeft = false;
            }
        }
    }

    void LateUpdate()
    {
        // 락온 중일 때만 VirtualCamera 회전
        if (currentTarget != null)
            RotateVCamTowardTarget();
    }

    public void ToggleLockOn()
    {
        if (currentTarget == null) StartLockOn();
        else StopLockOn();
    }

    private void HandleDeadTarget()
    {
        // 반경 내 살아 있는 적만 재수집
        var cols = Physics.OverlapSphere(transform.position, searchRadius, targetMask);
        var alive = cols.Select(c => c.transform)
                        .Where(t => t.gameObject.activeInHierarchy)
                        .ToList();
        alive.Remove(currentTarget);

        if (alive.Count > 0)
        {
            // 화면 중앙 기준 가장 가까운 적 선택
            currentTarget = alive.OrderBy(t =>
            {
                var vp = Camera.main.WorldToViewportPoint(t.position);
                return (vp - new Vector3(0.5f, 0.5f, vp.z)).sqrMagnitude;
            }).First();
            ApplyLockOn();
        }
        else
        {
            StopLockOn();
        }
    }

    private void StartLockOn()
    {
        var cols = Physics.OverlapSphere(transform.position, searchRadius, targetMask);
        candidates = cols.Select(c => c.transform).ToArray();
        if (candidates.Length == 0) return;

        // 화면 중앙에 가장 가까운 적 선택
        currentTarget = candidates.OrderBy(t =>
        {
            var vp = Camera.main.WorldToViewportPoint(t.position);
            return (vp - new Vector3(0.5f, 0.5f, vp.z)).sqrMagnitude;
        }).First();

        ApplyLockOn();
    }

    private void StopLockOn()
    {
        var cs = _playerController.cameraSettings;
        var rig = cs.Current;

        // FreeLook 축 값에 락온 카메라 방향 복사
        Vector3 camF = Camera.main.transform.forward;
        camF.y = 0f; camF.Normalize();
        float heading = Mathf.Atan2(camF.x, camF.z) * Mathf.Rad2Deg;
        rig.m_XAxis.Value = heading;

        // 락온 카메라 비활성화
        cs.lockOnCamera.Priority = 0;
        cs.EnableCameraMove();

        // FreeLook 우선순위 복원
        cs.keyboardAndMouseCamera.Priority = cs.inputChoice == CameraSettings.InputChoice.KeyboardAndMouse ? 1 : 0;
        cs.controllerCamera.Priority = cs.inputChoice == CameraSettings.InputChoice.Controller ? 1 : 0;

        // LookAt 복원
        rig.LookAt = cs.lookAt;

        // 초기화
        currentTarget = null;
        candidates = new Transform[0];
    }

    private void ApplyLockOn()
    {
        var cs = _playerController.cameraSettings;
        var lockCam = cs.lockOnCamera;

        lockCam.Follow = cs.follow;
        lockCam.LookAt = currentTarget;
        lockCam.Priority = 20;

        cs.DisableCameraMove();
        lockCam.transform.rotation = Quaternion.LookRotation((currentTarget.position - cs.follow.position).WithY(0f));
    }

    private void CycleTarget(int dir)
    {
        if (candidates.Length <= 1) return;
        int idx = System.Array.IndexOf(candidates, currentTarget);
        idx = (idx + dir + candidates.Length) % candidates.Length;
        currentTarget = candidates[idx];
        _playerController.cameraSettings.lockOnCamera.LookAt = currentTarget;
    }

    private void RotateVCamTowardTarget()
    {
        var cs = _playerController.cameraSettings;
        var lockCam = cs.lockOnCamera;
        Vector3 dir = currentTarget.position - cs.follow.position;
        dir.y = 0f;
        Quaternion desired = Quaternion.LookRotation(dir);
        lockCam.transform.rotation = Quaternion.Slerp(lockCam.transform.rotation, desired, Time.deltaTime * rotationSpeed);
    }
}

public static class Vector3Ext
{
    public static Vector3 WithY(this Vector3 v, float y) => new Vector3(v.x, y, v.z);
}
