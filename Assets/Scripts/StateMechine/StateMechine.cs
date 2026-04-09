using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IState
{
    void Enter();
    void FixedUpdate();
    void Update();
    void Exit();
}

public class StateMechine
{
    public IState currentState;
    public void Initialize(IState initState)
    {
        currentState = initState;
        currentState.Enter();
    }

    public void ChangeState(IState changeState)
    {
        currentState.Exit();
        currentState = changeState;
        currentState.Enter();
    }
}
