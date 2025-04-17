using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;


public class PlaceEnemyOnScene : MonoBehaviour
{
    // editor 상에서 터렛 위치 편집을 쉽도록 하는 class
    // play중에는 작동하지 않는다
    private CancellationTokenSource cancelToken;
    public async UniTask UpdateClosestCounter()
    {            
        cancelToken = new CancellationTokenSource();

        while (!cancelToken.IsCancellationRequested)
        {
            if (Physics.Raycast(transform.position + Vector3.up, Vector3.down, out RaycastHit hitInfo, 1.5f))
            {
                if (hitInfo.collider is MeshCollider)
                {
                    // 적이 해당 위치에 정렬됨
                    transform.position = hitInfo.point;
                    // transform.rotation = Quaternion.FromToRotation(Vector3.up, hitInfo.normal); // 경사 대응
                }
            }
            await UniTask.Yield();
        }
    }

    public void Disabled()
    {
        cancelToken.Cancel();
    }
}