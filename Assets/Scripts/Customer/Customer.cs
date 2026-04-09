using System.Collections;
using System.Collections.Generic;
using Palmmedia.ReportGenerator.Core.Parser.Analysis;
using UnityEditor.EditorTools;
using UnityEngine;
using UnityEngine.AI;

public class Customer : MonoBehaviour
{
    [Tooltip("收银台")]
    public Cashier currentCashier;
    [Tooltip("当前排队位置")]
    public int queueIndex;
    [Tooltip("所有商品池")]
    public List<ThingsData> allItems; 
    [Tooltip("状态配置")]
    public List<CustomerStateConfig> stateConfigs;
    private Dictionary<CustomerStateType, CustomerStateConfig> configDict;
    public CustomerStateType currentState;
    public ThingsData tagertItem;
    public System.Action OnStateChanged;
    [Tooltip("大门位置")]
    public Transform door;

#region 顾客参数
    private Rigidbody rb;
    [Header("闲逛设置")]
    public float wanderRadius = 8f;     // 闲逛范围
    public float minWonderWaitTime = 1.5f;    // 最小停顿
    public float maxWonderWaitTime = 3.5f;    // 最大停顿
    public bool isWondering;
    private float currentWonderWaitTime;
    private bool isWaiting = false;
    private NavMeshAgent agent;
    [Header("寻找设置")]
    public float findRadius = 8f;
    public Shelf targetShelf;
    public int takeCount;
    

    [Tooltip("是否想买")]
    public bool hasTarget;  
    [Tooltip("是否找到物品")]
    public bool hasFindItem;  
    [Tooltip("是否到达收营台")] 
    public bool isReachedCashier; 
    [Tooltip("是否购买成功")] 
    public bool isSuccess;

    [Tooltip("排队时间")]
    public float waitTimer;    
    [Tooltip("最大等待")]
    public float maxWaitTime; 
    public bool isArrive;
#endregion



#region 顾客状态机 
    public StateMechine stateMechine{ get; private set; }
    public CustomerWonderState wonderState {get; private set;}
    public CustomerFindState findState {get; private set;}
    public CustomerMoveToCashierState moveToCashierState {get; private set;}
    public CustomerWaitState waitState {get; private set;}
    public CustomerLeaveState leaveState {get; private set;}
    public CustomerCheckOutState checkOutState {get; private set;}
    #endregion



    public void Awake()
    {
        rb = GetComponent<Rigidbody>();
        agent = GetComponent<NavMeshAgent>();

        configDict = new Dictionary<CustomerStateType, CustomerStateConfig>();
        foreach (var config in stateConfigs)
        {
            configDict[config.stateType] = config;
        }

        stateMechine = new StateMechine();
        wonderState = new CustomerWonderState(this, stateMechine, "Wonder");
        findState = new CustomerFindState(this, stateMechine, "Find");
        moveToCashierState = new CustomerMoveToCashierState(this, stateMechine, "Wonder");
        waitState = new CustomerWaitState(this, stateMechine, "Wait");
        leaveState = new CustomerLeaveState(this, stateMechine, "Wonder");
        checkOutState = new CustomerCheckOutState(this, stateMechine, "CheckOut");
    }

    private void Start() {
        stateMechine.Initialize(wonderState);
    }

    public void FixedUpdate()
    {
        stateMechine.currentState.FixedUpdate();
    }

    public void Update()
    {
        stateMechine.currentState.Update();
    }

#region 移动逻辑
    public void Wander()
    {
        // 还在算路径，直接return
        if (agent.pathPending) return;

        // 判断是否到达
        if (agent.remainingDistance <= 0.2f)
        {
            // 👉 进入“等待状态”
            if (!isWaiting)
            {
                isWaiting = true;
                agent.isStopped = true;
                waitTimer = 0f;
            }

            waitTimer += Time.deltaTime;

            // 等够时间，去下一个点
            if (waitTimer >= currentWonderWaitTime)
            {
                SetNewDestination();

                agent.isStopped = false;
                isWaiting = false;

                SetNewWaitTime();
            }
        }
    }

    // 设置新目标点
    public void SetNewDestination()
    {
        Vector3 target = GetRandomPoint(transform.position, wanderRadius);
        agent.SetDestination(target);
    }

    
    // 随机等待时间
    public void SetNewWaitTime()
    {
        currentWonderWaitTime = Random.Range(minWonderWaitTime, maxWaitTime);
    }

    // NavMesh 随机点
    public Vector3 GetRandomPoint(Vector3 center, float radius)
    {
        for (int i = 0; i < 10; i++)
        {
            // 只在XZ平面随机
            Vector2 random2D = Random.insideUnitCircle * radius;
            Vector3 random = new Vector3(random2D.x, 0, random2D.y);

            Vector3 target = center + random;

            NavMeshHit hit;
            if (NavMesh.SamplePosition(target, out hit, 2f, NavMesh.AllAreas))
            {
                return hit.position;
            }
        }

        return center;
    }

#endregion



#region 购买商品逻辑
    /// <summary>
    /// 获取状态枚举的配置信息
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public CustomerStateConfig GetConfig(CustomerStateType type)
    {
        return configDict[type];
    }

    public void GenerateRandomTarget()
    {
        if (allItems == null || allItems.Count == 0)
        return;

        int index = Random.Range(0, allItems.Count);
        tagertItem = allItems[index];

        int targetCount = Random.Range(1, 4);
        Debug.Log($"顾客想要：{tagertItem.itemName} x {targetCount}");
        takeCount = targetCount;
    }
    public GameObject FindItem()
    {
        print("顾客开始寻找要购买的物品");
        Collider[] hits = Physics.OverlapSphere(transform.position, findRadius);

        foreach(var hit in hits)
        {
            Shelf shelf = hit.GetComponent<Shelf>();
            if(shelf == null) continue;
            if(shelf.itemType == tagertItem)
            {
                targetShelf = shelf;
                break;
            }
        }

        if(targetShelf != null)
        {
            Debug.Log("找到货架："+ targetShelf.name);
            agent.SetDestination(targetShelf.transform.position);

            return targetShelf.gameObject;
        }
        else
        {
            Debug.Log("没有找到");
            return null;
        }

    }

    public IEnumerator DecideTarget(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        hasTarget = true ;//Random.value > 0.7f;
        isWondering = false;
    }
#endregion



#region 收银相关
    /// <summary>
    /// 移动到相关索引的位置
    /// </summary>
    public void MoveToQueuePoint()
    {
        if(currentCashier == null){
            Debug.Log("没有收银台");
            return;
        }

        print("顾客前往收银台");
        Transform point = currentCashier.GetQueuePoint(queueIndex);
        agent.SetDestination(point.position);

    }

    public void JoinQueue()
    {
        if(currentCashier == null){
            Debug.Log("没有收银台");
            return;
        }
        
        queueIndex = currentCashier.Enqueue(this);
    }

    /// <summary>
    /// 更新位置信息
    /// </summary>
    /// <param name="index"></param>
    public void SetQueueIndex(int index)
    {
        queueIndex = index;
    }
#endregion

    public bool CanCheckOut()
    {
        if(Input.GetKeyDown(KeyCode.E))
            return true;
        else 
            return false;
    }

    public void Pay()
    {
        print("顾客成功支付");
    }

    public void Angry()
    {
        print("顾客等待超时愤怒");
        ChangeStateType(CustomerStateType.Angry);
    }

    public void leaveShop()
    {
        Debug.Log("顾客结账完成，离开");
        agent.SetDestination(door.position);

        if(HasReachedDestination(door.position)){
            Debug.Log("已到达门口");
            Destroy(gameObject);
        }
        
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, findRadius);  
    }

    public void ChangeStateType(CustomerStateType newStateType)
    {
        currentState = newStateType;
        OnStateChanged?.Invoke();
    }


    public bool HasReachedDestination(Vector3 target)
    {
        float dis = Vector3.Distance(transform.position, target);
        return dis <= 2f;
    }
}
