using System;
using AYellowpaper.SerializedCollections;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

public enum GateSignType
{
    None,
    Cleared,
    Uncleared,
    Boss,
    Max
}

public class Gate : MonoBehaviour
{
    public Transform playerSpawnPoint;
    
    public RoomDirection roomDirection;
    public Action<RoomDirection> OnEnter;
    public Transform indicatorPoint;
    public SerializedDictionary<GateSignType, GameObject> gateSigns = new();

    public int gateOpenDuration = 3;

    void OnTriggerEnter(Collider other)
    {
        if (!other.gameObject.CompareTag("Player")) return;

        OnEnter?.Invoke(roomDirection);
    }

    public void ActiveGateSign(RoomType connectRoomRoomType, bool cleared)
    {
        GateSignType activeSign = GateSignType.None;
        
        if (connectRoomRoomType == RoomType.BossRoom)
        {
            activeSign = GateSignType.Boss;
        }
        else
        {
            activeSign = cleared ? GateSignType.Cleared : GateSignType.Uncleared;
        }
        
        foreach (var (type, sign) in gateSigns)
        {
            if (type == activeSign)
            {
                sign.SetActive(true);
                
                GetComponent<Collider>().enabled = false;
                _ = UniTask.Delay(gateOpenDuration * 1000).ContinueWith(() =>
                {
                    GetComponent<Collider>().enabled = true;
                });
                
                var particle = sign.GetComponentInChildren<ParticleSystem>();
                particle.transform.DOScale(0f, 0f);
                particle.transform.DOScale(2f, gateOpenDuration);
            }
            else
            {
                sign.SetActive(false);
            }
        }
    }
}
