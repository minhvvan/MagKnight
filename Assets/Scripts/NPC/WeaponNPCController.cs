using System;
using Cysharp.Threading.Tasks;
using hvvan;
using Moon;
using UnityEngine;
using UnityEngine.Playables;

public class WeaponNPCController : BaseNPCController
{
    [SerializeField] LootCrate lootCrate;
    [SerializeField] BaseCampGate baseCampGate;
    [SerializeField] private PlayableDirector playableDirector;

    public Transform interactionForward;
    private Action _dialogueComplete;

    protected void Start()
    {
        //랜덤 무기 생성
        lootCrate.SetLootCrate(ItemCategory.MagCore, ItemRarity.Common);
        baseCampGate.OnEnterWithoutWeapon += BlockGateAccess;
    }

    public void StartBlockDialogue(Action onComplete)
    {
        _currentInteractor = GameManager.Instance.Player.GetInterfaceInParent<IInteractor>();
        _isInteract = true;

        _dialogueComplete = onComplete;
        StartDialogue(GameManager.Instance.Player, 1);
    }

    private void BlockGateAccess(PlayerController playerController)
    {
        //강제 이동중이면 감지 X
        if (playerController.InputHandler.IsControllerInputBlocked()) return;
        playableDirector.Play();
    }

    protected override void InteractExit()
    {
        base.InteractExit();

        if (!lootCrate)
        {
            Debug.Log("Loot crate is null");
            return;
        }
        
        //대화 완료 callback
        _dialogueComplete?.Invoke();

        //상자 열기
        if (lootCrate.IsOpen) return;
        lootCrate.OpenCrate().Forget();
    }
    
    public void OnDrawGizmos()
    {
        Gizmos.DrawSphere(interactionForward.position, .3f);
    }
}
