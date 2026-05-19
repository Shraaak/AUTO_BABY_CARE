using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomerManager : MonoBehaviour
{
    [Header("基础设置")]
    public GameObject customerPrefab; // 顾客预制体
    public Transform spawnPoint;      // 生成位置
    
    [Header("生成间隔（秒）")]
    public float minSpawnTime = 2f;   // 最小间隔
    public float maxSpawnTime = 6f;  // 最大间隔
    
    [Header("数量限制")]
    public int maxCustomers = 5;      // 最多同时存在几个顾客
    
    [Header("干净度影响（脚印）")]
    public int maxFootprint = 30;     // 脚印超过这个数停止生成顾客

    private float spawnTimer;         // 生成计时器
    private float currentWaitTime;     // 当前随机等待时间

    void Start()
    {
        // 第一次生成随机时间
        currentWaitTime = Random.Range(minSpawnTime, maxSpawnTime);
    }

    void Update()
    {
        // 限制：顾客满了 / 脚印太多 → 不生成
        if(GetCurrentCustomerCount() >= maxCustomers || Footprint.CurrentFootprintCount >= maxFootprint)
            return;

        // 计时
        spawnTimer += Time.deltaTime;
        if(spawnTimer >= currentWaitTime)
        {
            SpawnCustomer(); // 生成顾客
            spawnTimer = 0;
            // 重新随机下一次时间（永远不连续）
            currentWaitTime = Random.Range(minSpawnTime, maxSpawnTime);
        }
    }

    void SpawnCustomer()
    {
        // 1. 生成顾客
        GameObject newCustomer = ObjectPool.Instance.GetObject(customerPrefab);
        newCustomer.transform.SetPositionAndRotation(spawnPoint.position, Quaternion.identity);
        // 2. 获取顾客脚本
        Customer customerComp = newCustomer.GetComponent<Customer>();
        
        // 3. 自动在场景里找到收银台，赋值给顾客
        customerComp.currentCashier = FindObjectOfType<Cashier>();
        // 顺便自动找大门，不用你手动拖
        customerComp.door = GameObject.Find("Door")?.transform; // 你的大门物体名叫Door就行

        // 防报错：没找到收银台给提示
        if(customerComp.currentCashier == null)
        {
            Debug.LogError("场景里没找到挂载Cashier脚本的收银台！");
        }
    }
    // 统计当前场景里的顾客数量
    int GetCurrentCustomerCount()
    {
        return FindObjectsOfType<Customer>().Length;
    }
}