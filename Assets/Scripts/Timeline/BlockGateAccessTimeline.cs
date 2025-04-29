using Moon;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

[RequireComponent(typeof(PlayableDirector), typeof(SignalReceiver))]
public class BlockGateAccessTimeline: MonoBehaviour
{
    private PlayableDirector _director;
    private SignalReceiver _signalReceiver;
    
    [SerializeField] private WeaponNPCController npc;
    [SerializeField] private PlayerController player;
    private Animator _playerAnimator;
    
    private void Awake()
    {
        _director = GetComponent<PlayableDirector>();
        
        _playerAnimator = player.GetComponent<Animator>();
    }
    
    // === Signal Receiver 메서드들 ===
    
    // 대화 시작 시 호출 (Signal)
    public void OnStartDialogue()
    {
        if (npc == null || player == null) return;
        
        // 대화 시작 및 타임라인 일시 정지
        npc.StartBlockDialogue(OnDialogueComplete);
        _director.Pause();
    }
    
    // 대화 완료 콜백 (NPC 대화 시스템에서 호출)
    private void OnDialogueComplete()
    {
        InteractionEvent.OnDialogueEnd -= OnDialogueComplete;
        
        // 타임라인 재개
        _director.Resume();
    }
    
    // 플레이어 걷기 시작 (Signal)
    public void OnStartPlayerWalk()
    {
        player.MoveForce(npc.interactionForward);
    }
}
