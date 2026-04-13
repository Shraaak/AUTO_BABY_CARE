using UnityEngine;

public class Player : MonoBehaviour
{
    //单例
    public static Player Instance{ get; private set; }
    
#region 角色状态
    public StateMechine stateMechine {get; private set; }
    public PlayerIdleState idleState {get; private set;}
    public PlayerWalkState walkState {get; private set;}
    public PlayerRunState runState {get; private set;}
    public PlayerPickUpState pickUpState {get; private set;}
#endregion
    public Animator anim;
    public Rigidbody rb;

#region 角色参数
    public float walkSpeed = 3f;
    public float runSpeed = 8f;
    [Tooltip("转向平滑时间")]
    public float turnSmoothTime = 0.1f;
    [Tooltip("拾取物品位置")]
    public Transform pickUpPoint;
    [Tooltip("拾取货物位置")]
    public Transform pickUpTruckPoint;
    [Tooltip("当前手持物品")]
    public Things currentThings;
#endregion

#region 角色检测参数
    [Tooltip("胶囊半径")]
    public float capsuleRadius = 0.35f;
    [Tooltip("胶囊总高度（含两端半球）")]
    public float capsuleHeight = 1.8f;
    [Tooltip("胶囊中心相对角色位置的偏移")]
    public Vector3 capsuleCenterOffset = new Vector3(0f, 0.9f, 0f);
    [Tooltip("沿正前方最大检测距离")]
    public float castDistance = 2f;
    [Tooltip("检测用的碰撞层")]
    public LayerMask hitLayers = ~0;

    [Tooltip("是否检测到碰撞")]
    public bool CastHit; //{ get; private set; }
    RaycastHit lastHit;
    public RaycastHit LastHit => lastHit;
#endregion

    private Cashier currentCashier;

#region 物品的拾取和放下接口
    /// <summary>
    /// 正前方胶囊检测到可拾取的物品
    /// </summary>
    /// <param name="thing"></param>
    /// <returns></returns>
    public bool TryGetPickableInFront(out Things thing)
    {
        thing = null;
        
        if (!CastHit || lastHit.collider == null)
            return false;
        if (!lastHit.collider.CompareTag("Things"))
            return false;
        thing = lastHit.collider.GetComponent<Things>();
        return thing != null && !thing.isPickUp;
    }


    /// <summary>
    /// 放下物品
    /// </summary>
    public void DropHeldThing()
    {
        if (currentThings == null)
            return;
        currentThings.Drop();
        currentThings = null;
    }
#endregion

    public void Awake()
    {
        Instance = this;

        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();

        stateMechine = new StateMechine();
        idleState = new PlayerIdleState(this, stateMechine, "Idle");
        walkState = new PlayerWalkState(this, stateMechine, "Walk");
        runState = new PlayerRunState(this, stateMechine, "Run");
        pickUpState = new PlayerPickUpState(this, stateMechine, "PickUp");
    }

    void Start()
    {
        stateMechine.Initialize(idleState);
    }

    public void FixedUpdate()
    {
        stateMechine.currentState.FixedUpdate();
        PlayerRayCast();
    }

    public void Update()
    {
        stateMechine.currentState.Update(); 

        if(Input.GetKeyDown(KeyCode.F)){
            TryTakeCargo();
            TryCheckOut();
        }
            
    }

#region 角色射线检测
    public bool PlayerRayCast()
    {
        CapsuleEndpoints(out Vector3 p1, out Vector3 p2);
        float r = Mathf.Max(0.001f, capsuleRadius);
        float d = Mathf.Max(0f, castDistance);
        CastHit = Physics.CapsuleCast(p1, p2, r, transform.forward, out lastHit, d, hitLayers, QueryTriggerInteraction.Ignore);
        return CastHit;
    }

    void CapsuleEndpoints(out Vector3 top, out Vector3 bottom)
    {
        float r = Mathf.Max(0.001f, capsuleRadius);
        float h = Mathf.Max(r * 2f + 0.01f, capsuleHeight);
        float half = (h * 0.5f) - r;
        Vector3 c = transform.TransformPoint(capsuleCenterOffset);
        Vector3 u = transform.up;
        top = c + u * half;
        bottom = c - u * half;
    }

    void OnDrawGizmos()
    {
        CapsuleEndpoints(out Vector3 p1, out Vector3 p2);
        float r = Mathf.Max(0.001f, capsuleRadius);
        Vector3 f = transform.forward;
        float len = Application.isPlaying && CastHit ? lastHit.distance : Mathf.Max(0f, castDistance);
        Vector3 o = f * len;

        Gizmos.color = Application.isPlaying && CastHit ? Color.red : Color.green;
        Gizmos.DrawLine(p1, p2);
        Gizmos.DrawWireSphere(p1, r);
        Gizmos.DrawWireSphere(p2, r);
        Gizmos.DrawLine(p1 + o, p2 + o);
        Gizmos.DrawWireSphere(p1 + o, r);
        Gizmos.DrawWireSphere(p2 + o, r);
    }
#endregion



#region 角色与Shelf的交互
    public bool TryAddItem()
    {
        if(!CastHit){
            Debug.Log("没有检测到sheft");
            return false;
        }

        if (currentThings == null)
        {
            Debug.Log("手上没拿东西");
            return false;
        }
        
        Shelf shelf = lastHit.collider.GetComponent<Shelf>();
        ThingsData data = currentThings.thingsData;
        bool isPut = shelf.AddItem(currentThings);

        return isPut;
    }
#endregion



#region 角色与Cashier的交互
    
    void TryCheckOut()
    {
        //碰撞获取Cashier组件
        if (! CastHit) return;
        print(lastHit.collider.name);
        
        currentCashier = lastHit.collider.GetComponent<Cashier>();

        if (currentCashier == null)
        {
            print("没有currentCashier");
            return;
        }
        
        Customer customer = currentCashier.CurrentCustomer;

        if (customer == null)
        {
            Debug.Log("没有顾客");
            return;
        }

        if(customer.isWaitingForBy)
        {
            int money = customer.tagertItem.price * customer.takeCount;
            GameEvents.OnCheckoutSuccess?.Invoke(money);
            customer.isSuccess = true;

            Debug.Log("结账成功");
        }
        else
        {
            customer.isSuccess = false;
            Debug.Log("你超时了，顾客生气离开");
        }

        currentCashier.Dequeue();
    }
#endregion



#region 角色与货车的交互
    public void TryTakeCargo()
    {
        if (! CastHit) return;
        print(lastHit.collider.name);

        Truck currentTruck = lastHit.collider.GetComponent<Truck>();

        if(currentTruck == null) return;

        currentTruck.OnPlayerInteract();
    }

#endregion
}
