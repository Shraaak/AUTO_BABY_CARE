using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomerState : IState
{
    protected Customer customer;
    protected StateMechine stateMechine;
    protected string animName;
    
    public CustomerState(Customer _customer, StateMechine _stateMechine, string _animName)
    {
        customer = _customer;
        stateMechine = _stateMechine;
        animName = _animName;
    }
    virtual public void Enter()
    {
        customer.anim.SetBool(animName, true);
    }

    virtual public void FixedUpdate() 
    {
        
    }

    virtual public void Update()
    {
        
    }

    virtual public void Exit()
    {
        customer.anim.SetBool(animName, false);
    }
}
