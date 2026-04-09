using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerIdleState : PlayerState
{
    public PlayerIdleState(Player _player, StateMechine _stateMechine, string _animName) : base(_player, _stateMechine, _animName)
    {
    }

    public override void Enter()
    {
        base.Enter();
    }

    public override void Exit()
    {
        base.Exit();
    }

    public override void Update()
    {
        base.Update();

        if(Input.GetKeyDown(KeyCode.E))
            stateMechine.ChangeState(player.pickUpState);
        if(inputDir.sqrMagnitude >= 0.001)
            stateMechine.ChangeState(player.walkState);
    }
}
