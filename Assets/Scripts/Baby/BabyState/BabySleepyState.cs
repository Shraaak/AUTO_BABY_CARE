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

        baby.ShowTemporaryTip("宝宝肚子困了");
        baby.TriggerNeedInteract("sleepy");
        baby.TriggerLLMMessage("好困我要睡觉");
    }

    public override void Update()
    {
        baby.ChangeStateAfterNeedResolved();
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
