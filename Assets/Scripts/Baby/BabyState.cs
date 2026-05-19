using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BabyState : IState
{
    protected Baby baby;
    protected StateMechine stateMechine;
    protected string animName;
    protected Animator anim;
    
    public BabyState(Baby _bady, StateMechine _stateMechine, string _animName)
    {
        baby = _bady;
        stateMechine = _stateMechine;
        animName = _animName;

        anim = baby.GetComponent<Animator>();
    }
    virtual public void Enter()
    {
        if (anim != null)
            anim.Play(animName);

        Debug.Log("进入" + animName);
    }

    virtual public void FixedUpdate() 
    {
        
    }

    virtual public void Update()
    {
        baby.ChangeStatePriority();
    }

    virtual public void Exit()
    {
        Debug.Log("退出" + animName);
    }
}
