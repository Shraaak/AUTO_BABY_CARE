using UnityEngine;
using UnityEngine.AI;

public class BabyWalkState : BabyState
{
    public BabyWalkState(Baby _baby, StateMechine _stateMechine, string _animName) : base(_baby, _stateMechine, _animName)
    {
    }

    public override void Enter()
    {
        base.Enter();
        baby.isWondering = true;
        //baby.TriggerNeedInteract("walk");
    }

    public override void Update()
    {
        base.Update();
    }

    public override void FixedUpdate()
    {
        base.FixedUpdate();
        baby.Wander();
    }
}
