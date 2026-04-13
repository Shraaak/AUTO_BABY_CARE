using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomerLeaveState : CustomerState
{
    public CustomerLeaveState(Customer _customer, StateMechine _stateMechine, string _animName) : base(_customer, _stateMechine, _animName)
    {
    }

    public override void Enter()
    {
        base.Enter();
        if(customer.isSuccess)
            customer.Happy();
        else
            customer.Angry();
        
    }

    public override void Update()
    {
        base.Update();
        customer.leaveShop();
    }
}
