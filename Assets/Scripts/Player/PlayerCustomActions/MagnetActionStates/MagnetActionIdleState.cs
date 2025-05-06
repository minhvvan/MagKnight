using UnityEngine;

public class MagnetActionIdleState : BaseMagnetActionState
{
    public MagnetActionIdleState(PlayerMagnetActionController controller) : base(controller)
    {
    }

    public override void Enter(IStateData stateData = null)
    {
        controller.ResetPlayerState();
    }

    public override void Exit()
    {
    }

    public override void UpdateState()
    {    
    }
}
