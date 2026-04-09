using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomerCheckOutState : CustomerState
{
    public CustomerCheckOutState(Customer _customer, StateMechine _stateMechine, string _animName) : base(_customer, _stateMechine, _animName)
    {
    }

    public override void Enter()
    {
        base.Enter();
        if (customer.isSuccess)
        {
            customer.Pay();
        }
        else{
            customer.Angry();
        }
    }

    public override void Update()
    {
        base.Update();
        stateMechine.ChangeState(customer.leaveState);
    }
}
