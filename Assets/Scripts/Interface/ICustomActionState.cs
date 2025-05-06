public interface ICustomActionState
{
    public void Enter(IStateData stateData = null);
    public void UpdateState();
    public void Exit();
}