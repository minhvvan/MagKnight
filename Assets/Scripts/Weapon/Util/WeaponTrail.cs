using UnityEngine;
using System.Collections.Generic;

public class WeaponTrail : MonoBehaviour
{
    const int NUM_VERTICES = 12;
     
    [SerializeField] GameObject _tip = null;

    [SerializeField] GameObject _base = null;

    GameObject _meshParent = null;

    [SerializeField] int _trailFrameLength = 15;
    
    [SerializeField] Material _trailMaterial = null;

    Mesh _mesh;
    Vector3[] _vertices;
    int[] _triangles;
    Vector2[] _uvs;
    Vector3 _previousTipPosition;
    Vector3 _previousBasePosition;
    bool _trailEnabled = false;

    List<Vector3[]> _frameQueue = new List<Vector3[]>();

    void Start()
    {
        // 메시 부모 오브젝트 생성
        _meshParent = new GameObject("WeaponTrail");
        _meshParent.AddComponent<MeshFilter>();
        _meshParent.AddComponent<MeshRenderer>();
        _meshParent.GetComponent<MeshRenderer>().material = _trailMaterial;

        // 메시와 삼각형 초기화
        _meshParent.transform.position = Vector3.zero;
        _mesh = new Mesh();
        
        _meshParent.GetComponent<MeshFilter>().mesh = _mesh;

        // 팁과 베이스의 시작 위치 설정
        _previousTipPosition = _tip.transform.position;
        _previousBasePosition = _base.transform.position;
    }
    
    void LateUpdate()
    {
        if (_trailEnabled)
        {
            // 현재 프레임의 위치를 저장
            Vector3[] frame = new Vector3[4];
            frame[0] = _base.transform.position; // 현재 베이스 위치
            frame[1] = _tip.transform.position;  // 현재 팁 위치
            frame[2] = _previousBasePosition;    // 이전 베이스 위치
            frame[3] = _previousTipPosition;     // 이전 팁 위치

            _frameQueue.Add(frame);

            if (_frameQueue.Count > _trailFrameLength)
            {
                _frameQueue.RemoveAt(0);
            }

            _previousTipPosition = _tip.transform.position;
            _previousBasePosition = _base.transform.position;
        }
        else if (_frameQueue.Count > 0)
        {
            // 트레일 비활성화 상태지만 잔상이 남아있으면 서서히 제거
            _frameQueue.RemoveAt(0);
        }

        // 프레임이 없으면 메시 생성하지 않음
        if (_frameQueue.Count == 0)
        {
            _mesh.Clear();
            return;
        }

        // 각 프레임마다 앞면과 뒷면에 2개의 정점이 필요
        int vertexCount = _frameQueue.Count * 4; // 각 프레임마다 4개의 정점(베이스, 팁) x 2면
        _vertices = new Vector3[vertexCount];
        _uvs = new Vector2[vertexCount];
        
        // 프레임이 1개인 경우 삼각형을 만들 수 없음
        if (_frameQueue.Count < 2)
        {
            _mesh.Clear();
            _mesh.vertices = _vertices;
            _mesh.uv = _uvs;
            return;
        }
        
        // 삼각형 계산: 프레임 간 연결에 필요한 삼각형 수
        int triangleIndexCount = (_frameQueue.Count - 1) * 12; // (프레임 수 - 1) * 2면 * 2삼각형 * 3인덱스
        _triangles = new int[triangleIndexCount];

        // 정점 배치
        for (int i = 0; i < _frameQueue.Count; i++)
        {
            Vector3[] frame = _frameQueue[i];
            
            // 각 프레임마다 4개의 정점 설정
            _vertices[i * 4 + 0] = frame[0]; // 현재 베이스
            _vertices[i * 4 + 1] = frame[1]; // 현재 팁
            _vertices[i * 4 + 2] = frame[2]; // 이전 베이스
            _vertices[i * 4 + 3] = frame[3]; // 이전 팁
            
            // UV 좌표 설정 (트레일 전체에 걸쳐 연속적으로)
            float uvX;
            if (_frameQueue.Count == 1)
                uvX = 0;
            else
                uvX = i / (float)(_frameQueue.Count - 1);
            
            _uvs[i * 4 + 0] = new Vector2(uvX, 0); // 베이스 UV
            _uvs[i * 4 + 1] = new Vector2(uvX, 1); // 팁 UV
            _uvs[i * 4 + 2] = new Vector2(uvX, 0); // 이전 베이스 UV
            _uvs[i * 4 + 3] = new Vector2(uvX, 1); // 이전 팁 UV
        }

        // 삼각형 인덱스 구성 (안전 검사 추가)
        int triangleIndex = 0;
        for (int i = 0; i < _frameQueue.Count - 1; i++)
        {
            // 현재 프레임과 다음 프레임 사이의 삼각형 생성
            int currentBaseIndex = i * 4;
            int nextBaseIndex = (i + 1) * 4;
            
            // 앞면 - 첫 번째 삼각형 (현재 베이스 - 현재 팁 - 다음 팁)
            _triangles[triangleIndex++] = currentBaseIndex;
            _triangles[triangleIndex++] = currentBaseIndex + 1;
            _triangles[triangleIndex++] = nextBaseIndex + 1;
            
            // 앞면 - 두 번째 삼각형 (현재 베이스 - 다음 팁 - 다음 베이스)
            _triangles[triangleIndex++] = currentBaseIndex;
            _triangles[triangleIndex++] = nextBaseIndex + 1;
            _triangles[triangleIndex++] = nextBaseIndex;
            
            // 뒷면 - 첫 번째 삼각형 (현재 베이스 - 다음 팁 - 현재 팁)
            _triangles[triangleIndex++] = currentBaseIndex + 2;
            _triangles[triangleIndex++] = nextBaseIndex + 3;
            _triangles[triangleIndex++] = currentBaseIndex + 3;
            
            // 뒷면 - 두 번째 삼각형 (현재 베이스 - 다음 베이스 - 다음 팁)
            _triangles[triangleIndex++] = currentBaseIndex + 2;
            _triangles[triangleIndex++] = nextBaseIndex + 2;
            _triangles[triangleIndex++] = nextBaseIndex + 3;
        }

        // 메시 업데이트
        _mesh.Clear();
        _mesh.vertices = _vertices;
        _mesh.triangles = _triangles;
        _mesh.uv = _uvs;
        _mesh.RecalculateNormals();
    }

    public void EnableTrail(bool enable)
    {
        _trailEnabled = enable;
        
        if (enable)
        {
            // 트레일 활성화 시 위치 초기화
            _previousTipPosition = _tip.transform.position;
            _previousBasePosition = _base.transform.position;
        }
    }
}