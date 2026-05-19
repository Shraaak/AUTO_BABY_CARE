

public class BabyHungryState : BabyWalkState
{
    public BabyHungryState(Baby _baby, StateMechine _stateMechine, string _animName) : base(_baby, _stateMechine, _animName)
    {
    }

    public override void Enter()
    {
        base.Enter();
        
        // 触发表现层的UI和LLM逻辑
        baby.TriggerNeedInteract("hunger");
        baby.TriggerLLMMessage("hunger_request");
    }

    public override void Update()
    {
        if (baby.hunger < baby.hungerThreshold - 10f)
        {
            stateMechine.ChangeState(baby.walkState);
        }
    }
}
