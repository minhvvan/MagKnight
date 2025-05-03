using UnityEngine;

public class MagnetActionIdleState : BaseMagnetActionState
{
    public MagnetActionIdleState(PlayerMagnetActionController controller) : base(controller)
    {
    }

    public override void Enter(IStateData stateData = null)
    {
        controller.ResetPlayerState();
        Debug.Log("Idle State Enter");
    }

    public override void Exit()
    {
        Debug.Log("Idle State Exit");
    }

    public override void UpdateState()
    {    
    }
}
