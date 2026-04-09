using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomerMoveToCashierState : CustomerState
{
    public CustomerMoveToCashierState(Customer _customer, StateMechine _stateMechine, string _animName) : base(_customer, _stateMechine, _animName)
    {

    }

    public override void Enter()
    {
        base.Enter();
        customer.ChangeStateType(CustomerStateType.None);

        //货物充足
        if(customer.targetShelf.TakeItem(customer.takeCount)){
            Debug.Log("顾客购买成功");
            customer.JoinQueue();
            customer.MoveToQueuePoint();
        }
        //货物不足
        else{
            Debug.Log("顾客购买失败");
            customer.Angry();
            stateMechine.ChangeState(customer.leaveState);
        }
    }

    public override void Update()
    {
        base.Update();

        
        
    }


}
