using UnityEngine;

// 适配你原有状态机的打扫状态
public class PlayerCleanFootprintState : PlayerState
{
    private Footprint targetFootprint;
    private float cleanDuration = 0.5f; // 打扫动作时长
    private float timer;

    public PlayerCleanFootprintState(Player player, StateMechine stateMechine, string animBoolName) 
        : base(player, stateMechine, animBoolName) { }

    public override void Enter()
    {
        base.Enter();
        timer = cleanDuration;


        // 获取要打扫的脚印
        if (player.TryGetFootprint(out Footprint fp))
        {
            targetFootprint = fp;
            Debug.Log("清理");
            // 👇 直接调用你原有脚印的 Clean() 方法！！！
            targetFootprint.Clean();
        }
    }

    public override void Update()
    {
        base.Update();
        // 打扫计时结束 → 返回 idle
        timer -= Time.deltaTime;
        if (timer <= 0)
        {
            stateMechine.ChangeState(player.idleState);
        }
    }

    public override void Exit()
    {
        base.Exit();
        targetFootprint = null;
    }
}