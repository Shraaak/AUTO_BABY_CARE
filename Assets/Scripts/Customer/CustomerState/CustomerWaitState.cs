using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class CustomerWaitState : CustomerState
{
    public CustomerWaitState(Customer _customer, StateMechine _stateMechine, string _animName) : base(_customer, _stateMechine, _animName)
    {
    }

    public override void Enter()
    {
        base.Enter();
        customer.ChangeStateType(CustomerStateType.Wait);
        customer.waitTimer = 0f;
        customer.isWaitingForBy = true;
    }

    public override void Update()
    {
        base.Update();

        customer.waitTimer += Time.deltaTime;
        customer.fillImage.fillAmount = customer.waitTimer/customer.maxWaitTime;

        //超时
        if (customer.waitTimer > customer.maxWaitTime)
        {
            customer.isWaitingForBy = false;
            customer.currentCashier.Dequeue();
            stateMechine.ChangeState(customer.leaveState);
        }
        
    }
}
