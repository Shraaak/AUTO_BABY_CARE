using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomerWaitState : CustomerState
{
    public CustomerWaitState(Customer _customer, StateMechine _stateMechine, string _animName) : base(_customer, _stateMechine, _animName)
    {
    }

    public override void Enter()
    {
        base.Enter();
        customer.waitTimer = 0f;
    }

    public override void Update()
    {
        base.Update();

        customer.waitTimer += Time.deltaTime;

        //超时
        if (customer.waitTimer > customer.maxWaitTime)
        {
            customer.isSuccess = false;

            stateMechine.ChangeState(customer.checkOutState);
        }
        if(customer.CanCheckOut()){
            customer.isSuccess = true;
            stateMechine.ChangeState(customer.checkOutState);
        }
    }
}
