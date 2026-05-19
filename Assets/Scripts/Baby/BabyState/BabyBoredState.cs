using UnityEngine;

public class BabyBoredState : BabyWalkState
{
    public BabyBoredState(Baby _baby, StateMechine _stateMechine, string _animName) : base(_baby, _stateMechine, _animName)
    {
    }

    public override void Enter()
    {
        base.Update();
        
        baby.TriggerNeedInteract("bored");
        baby.TriggerLLMMessage("bored_request");
    }

    public override void Update()
    {
        base.Update();
    }
}
