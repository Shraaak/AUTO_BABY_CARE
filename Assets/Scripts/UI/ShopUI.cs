using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShopUI : MonoBehaviour
{
    public Transform spawnPoint;
    public Transform despawnPoint;
    public Transform pausePoint;
    public static ShopUI Instance{get; private set;}
    public Truck truckPrefab;
    public bool IsCurrentTruckLeaving = true;

    void Awake()
    {
        Instance = this;
    }
    public void BuyItem(ThingsData _thingsData)
    {
        if (IsCurrentTruckLeaving)
        {
            print("购买成功");
            int amount = 10;
            ThingsData thingsData = _thingsData;
            //创建订单
            OrderManager.Instance.CreateOrder(thingsData, amount);
            //生成货车
            Truck truck = Instantiate(truckPrefab);
            //初始化（让货车自己获取订单）
            truck.Init(spawnPoint, despawnPoint, pausePoint);
        }
        
    }
}
