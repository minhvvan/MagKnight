using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using Cysharp.Threading.Tasks;
using hvvan;
using Moon;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

[RequireComponent(typeof(SignalReceiver), typeof(PlayableDirector))]
public class WeaponNPCController : BaseNPCController
{
    [SerializeField] LootCrate lootCrate;
    [SerializeField] BaseCampGate baseCampGate;
    [SerializeField] private SignalAsset blockGateSignal;

    private SignalReceiver _signalReceiver;
    private PlayableDirector _playableDirector;
    
    protected override void Awake()
    {
        base.Awake();
        _signalReceiver = GetComponent<SignalReceiver>();
        _playableDirector = GetComponent<PlayableDirector>();
            
        // Signal 반응 설정
        var reaction = new UnityEngine.Events.UnityEvent();
        reaction.AddListener(StartBlockDialogue);
        
        // Signal과 반응 연결
        _signalReceiver.AddReaction(blockGateSignal, reaction);
    }
    
    protected void Start()
    {
        //랜덤 무기 생성
        lootCrate.SetLootCrate(ItemCategory.MagCore, ItemRarity.Common);
        baseCampGate.OnEnterWithoutWeapon += BlockGateAccess;
    }

    private void StartBlockDialogue()
    {
        Debug.Log("start dialogue");
        _playableDirector.Pause();
        
        _currentInteractor = GameManager.Instance.Player.GetInterfaceInParent<IInteractor>();
        _isInteract = true;
        StartDialogue(GameManager.Instance.Player, 1, OnDialogueEnded);
    }

    private void OnDialogueEnded()
    {
        InteractionEvent.OnDialogueEnd -= OnDialogueEnded;
        _playableDirector.Resume();
    }

    private void BlockGateAccess(PlayerController playerController)
    {
        // _currentInteractor = playerController.GetInterfaceInParent<IInteractor>();
        // _isInteract = true;
        //
        // //1 -> 무기없이 이동하는 플레이어를 막는 대화
        // StartDialogue(playerController, 1);
        
        _playableDirector.Play();
    }

    protected override void InteractExit()
    {
        base.InteractExit();

        if (!lootCrate)
        {
            Debug.Log("Loot crate is null");
            return;
        }

        //상자 열기
        if (lootCrate.IsOpen) return;
        lootCrate.OpenCrate().Forget();
    }
}
