using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerWalkState : PlayerState
{
    public PlayerWalkState(Player _player, StateMechine _stateMechine, string _animName) : base(_player, _stateMechine, _animName)
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

    public override void FixedUpdate()
    {
        base.FixedUpdate();
        
        //角色移动
        player.rb.velocity = new Vector3(inputX * player.walkSpeed, player.rb.velocity.y, inputZ * player.walkSpeed);
        
    }

    public override void Update()
    {
        base.Update();

        //角色转向
        player.transform.rotation = Quaternion.Lerp(player.transform.rotation, 
        Quaternion.LookRotation(new Vector3(inputX, 0, inputZ)), player.turnSmoothTime);

        if(inputDir.sqrMagnitude <= 0.0001)
            stateMechine.ChangeState(player.idleState);
        if(Input.GetKeyDown(KeyCode.LeftShift))
            stateMechine.ChangeState(player.runState);
        
    }
}
