using System.Linq;
using UnityEngine;
using Cinemachine;
using Moon; // CameraSettings 네임스페이스

public class LockOnSystem : MonoBehaviour
{
    [Header("레퍼런스")]
    public CameraSettings           camSettings;  // CameraRig의 CameraSettings
    public CinemachineVirtualCamera lockCam;      // LockOnVirtualCamera
    public LayerMask                targetMask;
    public float                    searchRadius = 15f;

    [Header("회전 설정")]
    public float rotationSpeed = 5f;  // 회전 속도 조절

    Transform[] candidates = new Transform[0];
    public Transform   currentTarget;

    InputHandler inputHandler;
    void Update()
    {
        if (currentTarget != null && Input.GetKeyDown(KeyCode.E)) CycleTarget(1);
    }

    void LateUpdate()
    {
        // 락온 중일 때만 VirtualCamera 게임오브젝트를 회전
        if (currentTarget != null)
            RotateVCamTowardTarget();
    }

    public void ToggleLockOn()
    {
        if (currentTarget == null) StartLockOn();
        else                        StopLockOn();
    }

    void StartLockOn()
    {
        var cols = Physics.OverlapSphere(transform.position, searchRadius, targetMask);
        candidates = cols.Select(c => c.transform).ToArray();
        if (candidates.Length == 0) return;

        // 화면 중앙에 가장 가까운 적 선택
        currentTarget = candidates
          .OrderBy(t => {
              var vp = Camera.main.WorldToViewportPoint(t.position);
              return (vp - new Vector3(0.5f, 0.5f, vp.z)).sqrMagnitude;
          })
          .First();

        ApplyLockOn();
    }

    void StopLockOn()
    {
        // 1) 현재 보이는 카메라(락온 카메라)가 바라보는 방향을 가져와서 수평 벡터로 만들기
        Vector3 camForward = Camera.main.transform.forward;
        camForward.y = 0f;
        camForward.Normalize();

        // 2) 이 벡터의 heading 각도(0~360) 계산
        float heading = Mathf.Atan2(camForward.x, camForward.z) * Mathf.Rad2Deg;

        // 3) FreeLook 축 값에 복사
        var rig = camSettings.Current;          // 현재 사용 중인 FreeLook
        rig.m_XAxis.Value = heading;           

        // (선택) 수직(Pitch)까지 완벽히 맞추고 싶다면 아래처럼 YAxis도 설정할 수 있어요:
        // float pitchAngle = Vector3.SignedAngle(
        //     Vector3.ProjectOnPlane(camForward, Vector3.right),
        //     Vector3.ProjectOnPlane(Camera.main.transform.forward, Vector3.forward),
        //     Vector3.right
        // );
        // rig.m_YAxis.Value = Mathf.InverseLerp(rig.m_YAxis.m_MinValue, rig.m_YAxis.m_MaxValue, pitchAngle);

        // 4) Priority 낮춰서 FreeLook으로 복귀
        lockCam.Priority = 0;
        camSettings.EnableCameraMove();

        // 5) LookAt도 원래 플레이어로 복원
        rig.LookAt = camSettings.lookAt;

        currentTarget = null;
    }


    void ApplyLockOn()
    {
        // VirtualCamera를 활성화
        lockCam.Follow   = camSettings.follow;
        lockCam.LookAt   = currentTarget;
        lockCam.Priority = 20;

        camSettings.DisableCameraMove();

        // 즉시 초기 회전 맞춰주고 싶다면 아래 한 줄 추가
        lockCam.transform.rotation = Quaternion.LookRotation(
            (currentTarget.position - camSettings.follow.position).WithY(0f)
        );
    }

    void CycleTarget(int dir)
    {
        if (candidates.Length <= 1) return;
        int idx = System.Array.IndexOf(candidates, currentTarget);
        idx = (idx + dir + candidates.Length) % candidates.Length;
        currentTarget = candidates[idx];
        // LookAt만 교체
        lockCam.LookAt = currentTarget;
    }

    void RotateVCamTowardTarget()
    {
        Vector3 playerPos = camSettings.follow.position;
        // 적 방향 (수평만)
        Vector3 dir = currentTarget.position - playerPos;
        dir.y = 0f;
        if (dir.sqrMagnitude < 0.001f) return;

        Quaternion desired = Quaternion.LookRotation(dir);
        lockCam.transform.rotation = Quaternion.Slerp(
            lockCam.transform.rotation,
            desired,
            Time.deltaTime * rotationSpeed
        );
    }
}

// Vector3 편의 확장 메서드
public static class Vector3Ext
{
    public static Vector3 WithY(this Vector3 v, float y)
    {
        return new Vector3(v.x, y, v.z);
    }
}
