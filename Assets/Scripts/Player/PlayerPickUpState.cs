using UnityEngine;

public class PlayerPickUpState : PlayerState
{
    public PlayerPickUpState(Player _player, StateMechine _stateMechine, string _animName) : base(_player, _stateMechine, _animName)
    {
    }

    public override void Enter()
    {
        base.Enter();

        if (player.TryGetPickableInFront(out Things things))
        {
            player.currentThings = things;
            things.PickUp(player.pickUpPoint);
        }
    }

    public override void FixedUpdate()
    {
        base.FixedUpdate();

        if (player.currentThings != null){
            player.rb.velocity = new Vector3(inputX * player.walkSpeed, 
            player.rb.velocity.y, inputZ * player.walkSpeed);
        }
    }       

    public override void Update()
    {
        base.Update();

        if (player.currentThings == null)
        {
            stateMechine.ChangeState(player.idleState);
            return;
        }

        if (inputDir.sqrMagnitude >= 0.001f)
            player.transform.rotation = Quaternion.Lerp(player.transform.rotation,
                Quaternion.LookRotation(new Vector3(inputX, 0f, inputZ)), player.turnSmoothTime);

        if (Input.GetKeyDown(KeyCode.E))
        {
            player.DropHeldThing();
            stateMechine.ChangeState(player.idleState);
        }

        if (Input.GetKeyDown(KeyCode.F))
        {
            if (player.TryAddItem())
            {
                Debug.Log("放置成功");
                player.currentThings.DestroySelf();
                player.currentThings = null;
                stateMechine.ChangeState(player.idleState);
            }
            else
            {
                Debug.Log("放置失败");
            }
        }
    }
}
