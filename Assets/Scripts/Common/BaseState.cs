using UnityEngine;

public abstract class BaseState<T> : IState
{
    protected T _controller;

    public BaseState(T controller)
    {
        _controller = controller;
    }

    public abstract void Enter();
    public abstract void UpdateState();
    public abstract void Exit();
}