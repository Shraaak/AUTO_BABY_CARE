using Unity.VisualScripting;
using UnityEngine;

public class BabySleepyState : BabyState
{
    public BabySleepyState(Baby _baby, StateMechine _stateMechine, string _animName) : base(_baby, _stateMechine, _animName)
    {
    }

    public override void Enter()
    {
        base.Enter();
        baby.TriggerNeedInteract("sleepy");
    }

    public override void Update()
    {
        base.Update();
    }

    public override void Exit()
    {
        base.Exit();
        if (!baby.isOnBed)
        {
            baby.sleepyRate = 1.0f;
        }
    }
}
