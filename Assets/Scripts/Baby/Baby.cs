using UnityEngine;

public class Baby : MonoBehaviour
{
    private StateMechine stateMechine;
    public BabyIdleState idleState { get; private set; }
    public BabyWalkState walkState { get; private set; }
    public BabyHungryState hungryState { get; private set; }
    public BabySleepyState sleepyState { get; private set; }
    public BabyPoopState poopState { get; private set; }
    public BabySickState sickState { get; private set; }
    public BabyBoredState boredState { get; private set; }
    public BabyDestroyState destroyState { get; private set; }

    public void Awake()
    {
        stateMechine = new StateMechine();
        idleState = new BabyIdleState(this, stateMechine, "Idle");
        walkState = new BabyWalkState(this, stateMechine, "Walk");
        hungryState = new BabyHungryState(this, stateMechine, "Hungry");
        sleepyState = new BabySleepyState(this, stateMechine, "Sleepy");
        poopState = new BabyPoopState(this, stateMechine, "Poop");
        sickState = new BabySickState(this, stateMechine, "Sick");
        boredState = new BabyBoredState(this, stateMechine, "Bored");
        destroyState = new BabyDestroyState(this, stateMechine, "Destroy");
        stateMechine.Initialize(idleState);
    }

    public void FixedUpdate()
    {
        if (stateMechine.currentState != null)
            stateMechine.currentState.FixedUpdate();
    }

    public void Update()
    {
        if (stateMechine.currentState != null)
            stateMechine.currentState.Update();
    }
}
