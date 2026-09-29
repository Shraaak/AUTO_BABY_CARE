using UnityEngine;

public class BabyHungryState : BabyWalkState
{
    public BabyHungryState(Baby _baby, StateMechine _stateMechine, string _animName) : base(_baby, _stateMechine, _animName)
    {
    }

    public override void Enter()
    {
        base.Enter();

        baby.ShowTemporaryTip("宝宝肚子饿了");
        
        // 触发表现层的UI和LLM逻辑
        baby.TriggerNeedInteract("hunger");
        baby.TriggerLLMMessage("肚子好饿");
    }

    public override void Update()
    {
        baby.ChangeStateAfterNeedResolved();
    }
}
