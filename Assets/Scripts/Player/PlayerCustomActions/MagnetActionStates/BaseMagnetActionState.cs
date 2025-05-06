public abstract class BaseMagnetActionState : ICustomActionState
{
    public PlayerMagnetActionController controller;    

    public BaseMagnetActionState(PlayerMagnetActionController controller)
    {
        this.controller = controller;
    }
    

    public abstract void Enter(IStateData stateData = null);
    public abstract void Exit();

    public abstract void UpdateState();
}