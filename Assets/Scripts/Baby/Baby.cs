using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AI;


public class Baby : MonoBehaviour
{
    public static Baby Instance{ get; private set; }


#region 宝宝基础数值
    public float hunger;   // 饥饿 
    public float sleepy;   // 困意
    public float boredom;  // 无聊
    public float maxSize = 100f;

    [Header("数值阈值")]
    public float hungerThreshold = 50f;   
    public float sleepyThreshold = 20f;   
    public float boredomThreshold = 70f;    
    public float sickThreshold = 0f;     

    [Header("数值变化速率")]
    public float hungerRate = 1.5f;
    public float sleepyRate = 1.0f;
    public float boredomRate = 1.2f;

    [Header("wonder设置")]
    [Tooltip("随机移动范围")]
    public float wanderRadius = 5f;    
    [Tooltip("移动速度")]
    public float walkSpeed = 2f; 
    public float minWonderWaitTime = 0f;    
    public float maxWonderWaitTime = 1f; 
    public bool isWondering;
    private float currentWonderWaitTime;
    private bool isWaiting = false;
    private float waitTimer; 

    [Header("LLM对话设置")]
    [Header("对话设置")]
    public float correctReduce = 25f;   // 正确回答降低数值
    public float wrongAdd = 15f;        // 错误回答增加数值

    public bool isOnBed { get; private set; } // 是否在床上
    public bool isNeedSleepDialog { get; private set; } // 床上需要哄睡对话
    
    public bool isDialog { get; private set; }
    public string currentTalkState; // bored / sleepy

    public Collider babyCollider;
    public Rigidbody rb;


# endregion



#region 被抱起相关
    public bool isPickedUp { get; private set; } // 是否被抱起
    private Vector3 originalPosition; // 被抱起前的位置
    private Quaternion originalRotation; // 被抱起前的旋转
    private bool originalAgentEnabled; // NavMeshAgent原本的启用状态
    private bool isValueUpdatePaused; // 数值更新是否暂停
#endregion
    public event System.Action<string> OnNeedInteract; // 触发交互时的事件
    public event System.Action<string> OnLLMMessage;   // 触发LLM对话的事件
    public event System.Action OnDialogEnded;//对话结束事件

    private NavMeshAgent agent;

#region 宝宝状态机
    public StateMechine stateMechine{ get; private set; }
    public BabyBePickUpState bePickUpState { get; private set; }
    public BabyWalkState walkState { get; private set; }
    public BabyHungryState hungryState { get; private set; }
    public BabySleepyState sleepyState { get; private set; }
    public BabySickState sickState { get; private set; }
    public BabyBoredState boredState { get; private set; }
#endregion

    public void Awake()
    {
        Instance = this;

        stateMechine = new StateMechine();
        bePickUpState = new BabyBePickUpState(this, stateMechine, "PickUp");
        walkState = new BabyWalkState(this, stateMechine, "Walk");
        hungryState = new BabyHungryState(this, stateMechine, "Hungry");
        sleepyState = new BabySleepyState(this, stateMechine, "Sleepy");
        sickState = new BabySickState(this, stateMechine, "Sick");
        boredState = new BabyBoredState(this, stateMechine, "Bored");

        babyCollider = GetComponent<Collider>();
        rb = GetComponent<Rigidbody>();
        stateMechine.Initialize(walkState);
    }

    void Start()
    {

        agent = GetComponent<NavMeshAgent>();
        if (agent == null)
        {
            Debug.LogError("Baby 身上没挂 NavMeshAgent！");
        }
        else
        {
            agent.speed = walkSpeed; 
            agent.stoppingDistance = 0.1f; 
        }

        hunger = maxSize;
        sleepy = maxSize;
        boredom = maxSize;

    }

    public void FixedUpdate()
    {
        if (stateMechine.currentState != null)
            stateMechine.currentState.FixedUpdate();
    }

    public void Update()
    {
        // 数值变化
        UpdateValues();

        if (stateMechine.currentState != null)
            stateMechine.currentState.Update();
    }

#region 数据跟新

    /// <summary>
    /// 基础数值增长
    /// </summary>
    private void UpdateValues()
    {
        if (isValueUpdatePaused) return;

        bool isSleeping = isOnBed && !isNeedSleepDialog;
        
        if (!isSleeping)
        {
            hunger -= Time.deltaTime * hungerRate;
            boredom -= Time.deltaTime * boredomRate;
        }
        sleepy -= Time.deltaTime * sleepyRate;
        
        
        // 需要哄睡对话时，sleepy不变化；对话成功后才减少
        if (isNeedSleepDialog)
        {
            sleepy = Mathf.Clamp(sleepy, 0, maxSize);
        }

        // 困意降到0 且 不需要对话 → 醒来下床
        if (sleepy <= 0 && !isNeedSleepDialog)
        {
            WakeUpFromBed();
        }

        ClampAll();
    }


    void ClampAll()
    {
        hunger = Mathf.Clamp(hunger, 0, maxSize);
        sleepy = Mathf.Clamp(sleepy, 0, maxSize);
        boredom = Mathf.Clamp(boredom, 0, maxSize);
    }
#endregion



#region 事件对外接口
    public void TriggerNeedInteract(string type)
    {
        OnNeedInteract?.Invoke(type);
    }

    public void TriggerLLMMessage(string type)
    {
        OnLLMMessage?.Invoke(type);
    }
#endregion



#region 移动逻辑
    public void Wander()
    {
        if (agent == null || !agent.enabled || !agent.isOnNavMesh)
            return;

        // 还在算路径，直接return
        if (agent.pathPending) return;

        // 判断是否到达
        if (agent.remainingDistance <= 0.2f)
        {
            // 进入“等待状态”
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
        currentWonderWaitTime = Random.Range(minWonderWaitTime, maxWonderWaitTime);
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



#region 状态优先级切换
/// <summary>
/// 按优先级自动切换状态
/// </summary>
public void ChangeStatePriority()
{
    // 避免空引用
    if (stateMechine == null) return;

    // 1. 最高优先级：生病（任意数值达到生病阈值）
    if (hunger <= sickThreshold || boredom <= sickThreshold || sleepy <= sickThreshold)
    {

        Debug.LogError("生病");
        stateMechine.ChangeState(sickState);
        return;
    }

    // 3. 饥饿
    else if (hunger <= hungerThreshold)
    {
        if (stateMechine.currentState != hungryState)
        {
            stateMechine.ChangeState(hungryState);
            return;
        }
    }

    // 4. 困意
    else if (sleepy <= sleepyThreshold)
    {
        if (stateMechine.currentState != sleepyState)
        {
            stateMechine.ChangeState(sleepyState);
            return;
        }
    }

    // 5. 无聊
    else if (boredom <= boredomThreshold)
    {
        if (stateMechine.currentState != boredState)
        {
            stateMechine.ChangeState(boredState);
            return;
        }
    }
    else
    {
        if (stateMechine.currentState != walkState)
        {
            stateMechine.ChangeState(walkState);
            return;
        }
    }

}
#endregion



#region 被抱起/放下接口
/// <summary>
/// 被抱起
/// </summary>
/// <param name="holdTransform"></param>
    public void BePickUp(Transform holdTransform)
    {
        if (isPickedUp) return;

        isPickedUp = true;
        originalPosition = transform.position;
        originalRotation = transform.rotation;
        originalAgentEnabled = agent != null ? agent.isActiveAndEnabled : false;

        // 禁用导航组件，停止移动
        if (agent != null)
        {
            agent.enabled = false;
        }

        if (rb != null)
        {
            rb.isKinematic = true;
        }

        // 暂停数值增长
        isValueUpdatePaused = true;

        stateMechine.ChangeState(bePickUpState);

        // 挂载到玩家持有位置，重置相对位置/旋转
        transform.SetParent(holdTransform);
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;

        // 触发LLM反馈
        OnLLMMessage?.Invoke("哇");
    }


    public void BePutDown()
    {
        if (!isPickedUp) return;

        isPickedUp = false;
        isValueUpdatePaused = false;
        isOnBed = false; 
        sleepyRate = 1.0f; 

        transform.SetParent(null);
        transform.rotation = originalRotation;

        if (agent != null && originalAgentEnabled)
        {
            agent.enabled = true;
            agent.speed = walkSpeed;
        }

        if (rb != null)
        {
            rb.isKinematic = false;
        }

        stateMechine.ChangeState(walkState);
    }
#endregion



#region llm对话接口
    public bool CurrentStateIsBoredOrSleepy()
    {
        // 普通场景：无聊/困意状态；床上场景：需要哄睡对话
        return stateMechine.currentState == boredState || 
               stateMechine.currentState == sleepyState ||
               (isOnBed && isNeedSleepDialog);
    }
    
    public void StartPlayerDialog()
    {
        if (isPickedUp || isDialog) return;

        // 床上哄睡对话标记
        if (isOnBed && isNeedSleepDialog)
        {
            currentTalkState = "sleepy";
        }
        else
        {
            currentTalkState = stateMechine.currentState == boredState ? "bored" : "sleepy";
        }

        isDialog = true;

        // 触发对应类型的LLM对话
        string dialogType = isOnBed && isNeedSleepDialog ? "dialog_sleep_bed" : "dialog_" + currentTalkState;
        TriggerLLMMessage(dialogType);

        // 强制停止宝宝移动
        if (agent != null) 
        {
            agent.velocity = Vector3.zero;
        }
        if (rb != null)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
    }

    // 接收LLM结果，修改数值
    public void OnDialogReply(string reply, bool isCorrect)
    {
        // 床上哄睡对话逻辑
        if (isOnBed && isNeedSleepDialog)
        {
            if (isCorrect)
            {
                // 对话成功：减少困意，开始持续睡眠
                sleepy = Mathf.Max(0, sleepy);
                sleepyRate = 5f; // 困意持续减少
                isNeedSleepDialog = false;
            }
            else
            {
                // 对话失败：增加困意，保持需要哄睡状态
                sleepy = Mathf.Min(100, sleepy + wrongAdd);
            }
        }
        // 普通无聊/困意对话逻辑
        else if (currentTalkState == "bored")
        {
            boredom = isCorrect ? Mathf.Max(0, boredom - correctReduce) : Mathf.Min(100, boredom + wrongAdd);
        }
        else if (currentTalkState == "sleepy")
        {
            sleepy = isCorrect ? Mathf.Max(0, sleepy - correctReduce) : Mathf.Min(100, sleepy + wrongAdd);
        }

        isDialog = false;
        OnDialogEnded?.Invoke();
    }
#endregion



#region 哄睡相关
    /// <summary>
    /// 被放到床上哄睡
    /// </summary>
    /// <param name="bedPos">床的位置</param>
    /// <param name="bedRot">床的旋转</param>
    public void PutToBed(Vector3 bedPos, Quaternion bedRot)
    {
        isOnBed = true;
        isNeedSleepDialog = true; // 标记需要哄睡对话
        isPickedUp = false; 
        isValueUpdatePaused = false; 

        if (agent != null)
        {
            agent.enabled = false;
        }
        if (rb != null)
        {
            rb.isKinematic = true;
        }

        transform.SetParent(null);
        transform.position = bedPos;
        transform.rotation = bedRot;

        stateMechine.ChangeState(sleepyState);
        Debug.Log("宝宝躺到床上啦，但是睡不着，需要哄睡");
    }

    /// <summary>
    /// 从床上醒来
    /// </summary>
    private void WakeUpFromBed()
    {
        isOnBed = false;
        isNeedSleepDialog = false; // 重置哄睡对话状态
        sleepyRate = 1.0f; // 恢复困意正常增长速率
        sleepy = 0; // 重置困意

        if (agent != null)
        {
            agent.enabled = true;
            agent.speed = walkSpeed;
        }
        if (rb != null)
        {
            rb.isKinematic = false;
        }

        //恢复
        boredomRate = 1.2f;
        hungerRate = 1.5f;

        stateMechine.ChangeState(walkState);
        Debug.Log("宝宝睡醒啦，下床玩咯～");
    }
#endregion


    public void Eat()
    {
        hunger += 30;
    }

    public bool HasReachedDestination(Vector3 target, int _dis)
    {
        float dis = Vector3.Distance(transform.position, target);
        return dis <= _dis;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, wanderRadius);  
    }
}
