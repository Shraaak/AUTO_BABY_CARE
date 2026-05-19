using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AI;

public class Customer : MonoBehaviour, IPoolable
{
    public System.Action onDestroyCallback;
    [Tooltip("收银台")]
    public Cashier currentCashier;
    public Transform point;
    [Tooltip("当前排队位置")]
    public int queueIndex;
    [Tooltip("所有商品池")]
    public List<ThingsData> allItems; 
    [Tooltip("状态配置")]
    public List<CustomerStateConfig> stateConfigs;
    public Dictionary<CustomerStateType, CustomerStateConfig> configDict{get; private set;}
    public CustomerStateType currentState;
    public ThingsData tagertItem;
    public int targetCount;
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
    [Tooltip("是否在等待购买")]
    public bool isWaitingForBy;
    public Image fillImage;

    [Header("脚印设置")]
    public GameObject footprintPrefab;
    [Tooltip("脚印间距（控制疏密）")]
    public float footprintSpacing = 1.2f; 
    public LayerMask groundLayer; // 面板选择地面层
    public float footprintOffset = 0.005f; // 贴地微小偏移

    private Vector3 lastFootprintPos;
    private bool isFirstFootprint = true;
    private GameObject selfPrefab;
#endregion



#region 顾客状态机 
    public StateMechine stateMechine{ get; private set; }
    public CustomerWonderState wonderState {get; private set;}
    public CustomerFindState findState {get; private set;}
    public CustomerMoveToCashierState moveToCashierState {get; private set;}
    public CustomerWaitState waitState {get; private set;}
    public CustomerLeaveState leaveState {get; private set;}
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
        SpawnFootprint();
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

        targetCount = Random.Range(1, 4);
        Debug.Log($"顾客想要：{tagertItem.itemName} x {targetCount}");
        takeCount = targetCount;
    }
    public GameObject FindItem()
    {
        // 直接找场景里所有的货架，不用物理检测
        Shelf[] allShelves = Object.FindObjectsOfType<Shelf>();
        
        float nearestDistance = float.MaxValue;
        targetShelf = null;

        foreach (var shelf in allShelves)
        {
            // 过滤类型不匹配的 以及 没货的 (就加了这一个条件)
            if (shelf.itemType != tagertItem || !shelf.HasItem())
                continue;

            // 算距离，找最近的
            float distance = Vector3.Distance(transform.position, shelf.transform.position);
            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                targetShelf = shelf;
            }
        }

        if (targetShelf != null)
        {
            agent.SetDestination(targetShelf.transform.position);
            return targetShelf.gameObject;
        }
        else
        {
            Debug.Log("没有找到（或所有该类货架都没货了）");
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

        // 顾客前往收银台
        point = currentCashier.GetQueuePoint(queueIndex);
        agent.stoppingDistance = 0;
        agent.SetDestination(point.position);

    }

    public void JoinQueue()
    {
        if(currentCashier == null){
            Debug.Log("没有收银台");
            return;
        }
        
        queueIndex = currentCashier.Enqueue(this);

        //队伍排满了的话
        if(queueIndex > 3)
            stateMechine.ChangeState(leaveState);
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

    public void Angry()
    {
        print("顾客等待超时愤怒");
        ChangeStateType(CustomerStateType.Angry);
    }

    public void Happy()
    {
        print("顾客开心");
        ChangeStateType(CustomerStateType.Happy);
    }

    public void leaveShop()
    {
        agent.SetDestination(door.position);

        if(HasReachedDestination(door.position, 1)){
            Debug.Log("已到达门口");
            onDestroyCallback?.Invoke(); // 触发回调
            ObjectPool.Instance.ReturnObject(gameObject, selfPrefab);
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
        EventCenter.Instance.EventTrigger("CustomerStateChange");
    }


    public bool HasReachedDestination(Vector3 target, int _dis)
    {
        float dis = Vector3.Distance(transform.position, target);
        return dis <= _dis;
    }

#region 脚印生成逻辑
    /// <summary>
    /// 生成脚印（在移动过程中调用）
    /// </summary>
    private void SpawnFootprint()
    {
        if (footprintPrefab == null) return;

        // 只有移动时才生成（防止原地旋转生成脚印）
        if (agent.velocity.magnitude < 0.1f) return;

        // 第一个脚印直接生成，后续判断移动距离
        if (isFirstFootprint)
        {
            CreateFootprintAtCurrentPos();
            isFirstFootprint = false;
            return;
        }

        // 间距控制疏密
        float distance = Vector3.Distance(transform.position, lastFootprintPos);
        if (distance >= footprintSpacing)
        {
            CreateFootprintAtCurrentPos();
        }
    }

    /// <summary>
    /// 射线检测地面 + 保底生成（绝不丢失脚印）
    /// </summary>
    private void CreateFootprintAtCurrentPos()
    {
        Vector3 spawnPos = transform.position;
        spawnPos.y = footprintOffset; // 默认保底高度

        // 射线检测优化：起点更低，检测更稳
        Vector3 rayOrigin = transform.position;
        rayOrigin.y += 0.5f;

        // 调试：在Scene视图显示射线（红色），能看到是否碰到地面
        Debug.DrawLine(rayOrigin, rayOrigin + Vector3.down * 2f, Color.red, 0.1f);

        // 如果射线检测到地面，就用地面高度
        if (Physics.Raycast(rayOrigin, Vector3.down, out RaycastHit hit, 2f, groundLayer))
        {
            spawnPos = hit.point;
            spawnPos.y += footprintOffset;
        }

        // 3. 脚印旋转（匹配角色朝向）
        Quaternion spawnRot = Quaternion.Euler(0, transform.rotation.eulerAngles.y, 0);

        // 对象池获取
        GameObject footprint = ObjectPool.Instance.GetObject(footprintPrefab);
        footprint.transform.SetPositionAndRotation(spawnPos, spawnRot);

        lastFootprintPos = transform.position; 
    }

    
    #endregion


#region IPoolable实现

public void SetPrefab(GameObject prefab)
{
    selfPrefab = prefab;
}

public void OnSpawn()
{
    Debug.Log($"顾客 {gameObject.name} 从对象池生成");
    
    // 1. 先激活对象，防止组件操作报错
    gameObject.SetActive(true);

    // 2. 重置NavMeshAgent状态（最核心！）
    if (agent == null)
        agent = GetComponent<NavMeshAgent>();
    
    agent.enabled = true;
    agent.isStopped = false;
    agent.ResetPath(); // ✅ 这一行会自动清除所有路径和pathPending状态
    agent.velocity = Vector3.zero;
    agent.stoppingDistance = 0;

    // 3. 重置所有bool标记位
    isWondering = true;
    isWaiting = false;
    hasTarget = false;
    hasFindItem = false;
    isReachedCashier = false;
    isSuccess = false;
    isArrive = false;
    isWaitingForBy = false;
    isFirstFootprint = true;

    // 4. 重置所有计时器和数值
    waitTimer = 0f;
    currentWonderWaitTime = Random.Range(minWonderWaitTime, maxWonderWaitTime);
    targetCount = 0;
    takeCount = 0;
    queueIndex = -1;

    // 5. 重置所有引用类型（防止残留引用导致空指针）
    currentCashier = null;
    point = null;
    targetShelf = null;
    tagertItem = null;

    // 6. 重置脚印相关
    lastFootprintPos = transform.position;

    // 7. 重置UI
    if (fillImage != null)
    {
        fillImage.fillAmount = 0f;
        fillImage.gameObject.SetActive(false);
    }

    // 8. 停止所有残留协程（防止幽灵协程）
    StopAllCoroutines();

    // 9. 重置状态机并进入闲逛状态
    stateMechine.Initialize(wonderState);
    currentState = CustomerStateType.Wander;

    // 10. 立即设置第一个闲逛目标点（确保顾客立刻开始移动）
    SetNewDestination();
}

public void OnDespawn()
{
    Debug.Log($"顾客 {gameObject.name} 放回对象池");
    
    // 1. 停止所有协程
    StopAllCoroutines();

    // 2. 停止NavMeshAgent
    if (agent != null && agent.enabled)
    {
        agent.isStopped = true;
        agent.ResetPath();
        agent.enabled = false;
    }

    // 3. 退出当前状态机状态
    if (stateMechine.currentState != null)
    {
        stateMechine.currentState.Exit();
    }

    // 4. 隐藏对象
    gameObject.SetActive(false);
}
#endregion
}
