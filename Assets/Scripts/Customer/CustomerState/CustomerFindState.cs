using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomerFindState : CustomerState
{
    private GameObject targetSheft;
    public CustomerFindState(Customer _customer, StateMechine _stateMechine, string _animName) : base(_customer, _stateMechine, _animName)
    {
    }

    public override void Enter()
    {
        base.Enter();
        //随机生成想要的物品以及数量
        customer.GenerateRandomTarget();
        customer.ChangeStateType(CustomerStateType.Find);
        
        targetSheft = customer.FindItem();
    }

    public override void Update()
    {
        base.Update();
        
        if(targetSheft!=null && customer.HasReachedDestination(targetSheft.transform.position, 2)){
            Debug.Log("已到达相应货架");
            stateMechine.ChangeState(customer.moveToCashierState);
        }
    }
}
