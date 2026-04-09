using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomerWonderState : CustomerState
{
    public CustomerWonderState(Customer _customer, StateMechine _stateMechine, string _animName) : base(_customer, _stateMechine, _animName)
    {
    }

    public override void Enter()
    {
        base.Enter();
        customer.ChangeStateType(CustomerStateType.Wander);
        customer.isWondering = true;
        
        float waitTime = Random.Range(0f, 5f);
        Debug.Log(waitTime);
        customer.StartCoroutine(customer.DecideTarget(waitTime));
    }

    public override void Update()
    {
        base.Update();

        if(customer.hasTarget && !customer.isWondering){
            Debug.Log("顾客想买");
            stateMechine.ChangeState(customer.findState);
        }
        else if(!customer.hasTarget && !customer.isWondering){
            Debug.Log("顾客不想买");
            stateMechine.ChangeState(customer.leaveState);
        }
    }

    public override void FixedUpdate()
    {
        base.FixedUpdate();
        customer.Wander();
    }

}
