using UnityEngine;

public class BabyBoredState : BabyWalkState
{
    public BabyBoredState(Baby _baby, StateMechine _stateMechine, string _animName) : base(_baby, _stateMechine, _animName)
    {
    }

    public override void Enter()
    {
        base.Enter();
        
        baby.ShowTemporaryTip("宝宝好无聊啊");
        baby.TriggerNeedInteract("bored");
        baby.TriggerLLMMessage("好无聊...");
    }

    public override void Update()
    {
        baby.ChangeStateAfterNeedResolved();
    }
}
