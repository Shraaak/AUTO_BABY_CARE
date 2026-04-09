using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerState : IState
{
    protected Player player;
    protected StateMechine stateMechine;
    protected string animName;

    protected float inputX;
    protected float inputZ;
    protected Vector2 inputDir;
    
    public PlayerState(Player _player, StateMechine _stateMechine, string _animName)
    {
        player = _player;
        stateMechine = _stateMechine;
        animName = _animName;
    }
    virtual public void Enter()
    {
        Debug.Log("进入"+ animName +"状态");
    }

    virtual public void FixedUpdate() 
    {
        
    }

    virtual public void Update()
    {
        inputX = Input.GetAxisRaw("Horizontal");
        inputZ = Input.GetAxisRaw("Vertical");
        inputDir = new Vector2(inputX, inputZ);
    }

    virtual public void Exit()
    {
        Debug.Log("退出"+ animName +"状态");
    }
}
