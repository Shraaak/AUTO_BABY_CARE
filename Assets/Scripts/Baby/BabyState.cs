using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BabyState : IState
{
    protected Baby baby;
    protected StateMechine stateMechine;
    protected string animName;
    
    public BabyState(Baby _bady, StateMechine _stateMechine, string _animName)
    {
        baby = _bady;
        stateMechine = _stateMechine;
        animName = _animName;
    }
    virtual public void Enter()
    {
        
    }

    virtual public void FixedUpdate() 
    {
        
    }

    virtual public void Update()
    {
        
    }

    virtual public void Exit()
    {
        
    }
}
