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
    public PlayerCleanFootprintState cleanFootprintState {get; private set;}
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
    [Header("脚印打扫")]
    public LayerMask footprintLayer;

    [Tooltip("是否检测到碰撞")]
    public bool CastHit; //{ get; private set; }
    RaycastHit lastHit;
    public RaycastHit LastHit => lastHit;
#endregion



#region 抱起宝宝相关
    [Tooltip("抱起宝宝的挂载位置")]
    public Transform holdBabyPoint; 
    public Baby heldBaby { get; private set; } // 当前抱着的宝宝
    public bool isInBabyDialog { get; private set; } //是否在跟宝宝对话
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
        rb = GetComponent<Rigidbody>();

        stateMechine = new StateMechine();
        idleState = new PlayerIdleState(this, stateMechine, "Idle");
        walkState = new PlayerWalkState(this, stateMechine, "Walk");
        runState = new PlayerRunState(this, stateMechine, "Run");
        pickUpState = new PlayerPickUpState(this, stateMechine, "PickUp");
        cleanFootprintState = new PlayerCleanFootprintState(this, stateMechine, "Clean");
    }

#region 脚印打扫检测
    /// <summary>
    /// 球形检测附近脚印【兼容Trigger触发器】
    /// </summary>
    public bool TryGetFootprint(out Footprint footprint)
    {
        footprint = null;
        // 检测半径（调整大小，建议1.5~2米）
        float checkRadius = 1.5f;

        // 核心：球形检测，能识别 Trigger 碰撞体！筛选脚印图层
        Collider[] hitColliders = Physics.OverlapSphere(transform.position + Vector3.up, checkRadius, footprintLayer);
        
        foreach (var col in hitColliders)
        {
            Footprint fp = col.GetComponent<Footprint>();
            if (fp != null)
            {
                footprint = fp;
                return true;
            }
        }

        return false;
    }

#endregion


    void Start()
    {
        stateMechine.Initialize(idleState);
    }

    public void FixedUpdate()
    {
        stateMechine.currentState.FixedUpdate();
        PlayerRayCast();
    }

    void Update()
    {
        if (isInBabyDialog)
        {
            if (stateMechine.currentState != idleState)
            {
                stateMechine.ChangeState(idleState);
            }
            return;
        }

        stateMechine.currentState.Update();

        HandleBabyPickUp();

        if (Input.GetKeyDown(KeyCode.F))
        {
            // 抱宝宝时，只处理放床上逻辑
            if (heldBaby != null)
            {
                if (BedTrigger.IsPlayerInBedArea)
                {
                    PutBabyToBed();
                }
                return;
            }

            // 优先触发宝宝对话（含床上哄睡）
            bool hasBabyDialog = TryStartBabyDialog();
            if (hasBabyDialog)
            {
                return;
            }

            // 无宝宝对话时，处理脚印/取货/结账
            if (TryGetFootprint(out Footprint footprint))
            {
                stateMechine.ChangeState(cleanFootprintState);
            }
            else
            {

                TryTakeCargo();
                TryCheckOut();
            }
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

        Shelf shelf = lastHit.collider.GetComponent<Shelf>();
        if(shelf == null) return false;

        if (currentThings == null)
        {
            Debug.Log("手上没拿东西");
            return false;
        }
        
        
        ThingsData data = currentThings.thingsData;
        bool isPut = shelf.AddItem(currentThings);

        return isPut;
    }

    /// <summary>
    /// 从货架取物品（最小化实现，每次取1个）
    /// </summary>
    public bool TryTakeItemFromShelf()
    {
        // 前提：手上无物品 + 射线检测到货架
        if (currentThings != null || !CastHit) 
            return false;

        Shelf shelf = lastHit.collider.GetComponent<Shelf>();
        if (shelf == null || !shelf.HasItem()) 
            return false;

        // 从货架取1个物品
        bool takeSuccess = shelf.TakeItem(1);
        if (!takeSuccess) 
            return false;

        // 生成物品预制体到拾取点
        ThingsData shelfItemData = shelf.itemType;
        GameObject itemObj = Instantiate(shelfItemData.perfeb);
        currentThings = itemObj.GetComponent<Things>();
        currentThings.PickUp(pickUpPoint);
        return true;
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
            EventCenter.Instance.EventTrigger<int>("MoneyChange", money);
            customer.isSuccess = true;
            AudioManager.Instance.PlaySFX("CheckOut");

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



#region 角色与宝宝的交互
    public bool GetEat()
    {
        if(!CastHit){
            Debug.Log("没有检测到Baby");
            return false;
        }

        Baby Baby = lastHit.collider.GetComponent<Baby>();
        if(Baby == null) return false;

        if (currentThings == null )
        {
            return false;
        }
        
        print(currentThings);
        ThingsData data = currentThings.thingsData;
        print(data);
        if (data.isCanEat)
        {
            Baby.Eat();
        }
        else
        {
            return false;
        }
        return true;
    }

    private void HandleBabyPickUp()
    {
        // 已经抱着宝宝按E放下
        if (heldBaby != null)
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                heldBaby.BePutDown();
                heldBaby = null;
            }
            return;
        }

        // 没抱宝宝按E检测并抱起
        if (Input.GetKeyDown(KeyCode.E))
        {
            // 复用你自己的胶囊射线检测
            if (!CastHit || lastHit.collider == null) return;
            // 检测Tag是否为Baby
            if (!lastHit.collider.CompareTag("Baby")) return;

            // 获取Baby组件
            Baby baby = lastHit.collider.GetComponent<Baby>();
            if (baby == null) return;
            // 避免重复抱起
            if (baby.isPickedUp) return;

            // 直接调用你提供的BePickUp方法
            baby.BePickUp(holdBabyPoint);
            heldBaby = baby;
        }
    }

    private bool TryStartBabyDialog()
    {
        if (!CastHit || lastHit.collider == null) return false;

        if (!lastHit.collider.CompareTag("Baby")) return false;

        Baby baby = lastHit.collider.GetComponent<Baby>();
        if (baby == null) return false;

        // 仅当宝宝处于无聊/困意状态 或 床上需要哄睡时触发对话
        if (!baby.CurrentStateIsBoredOrSleepy()) return false;

        print("开启对话");
        baby.OnDialogEnded += OnBabyDialogEnded;
        baby.StartPlayerDialog();
        isInBabyDialog = true;
        stateMechine.ChangeState(idleState);
        return true;
    }

    private void OnBabyDialogEnded()
    {
        isInBabyDialog = false;
        // 正确解除当前对话宝宝的回调
        if (CastHit && lastHit.collider != null && lastHit.collider.CompareTag("Baby"))
        {
            Baby baby = lastHit.collider.GetComponent<Baby>();
            if (baby != null)
            {
                baby.OnDialogEnded -= OnBabyDialogEnded;
            }
        }
    }

    private void PutBabyToBed()
    {
        if (heldBaby == null || !BedTrigger.IsPlayerInBedArea) return;
        print("睡觉");

        // 找到场景中的床Trigger（假设只有一张床，多张床需调整）
        BedTrigger bed = FindObjectOfType<BedTrigger>();
        if (bed == null || bed.bedSleepPoint == null) return;

        // 调用宝宝的“放到床上”方法
        heldBaby.PutToBed(bed.bedSleepPoint.position, bed.bedSleepPoint.rotation);
        heldBaby = null; // 解除持有
    }


#endregion
}
