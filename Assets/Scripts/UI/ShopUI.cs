using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Rendering;

public class ShopUI : MonoBehaviour
{
    public Transform spawnPoint;
    public Transform despawnPoint;
    public Transform pausePoint;
    public static ShopUI Instance{get; private set;}
    public Truck truckPrefab;
    public bool IsCurrentTruckLeaving = true;
    public Transform tipShowPos;

    void Awake()
    {
        Instance = this;
    }

    public void BuyItem(ThingsData _thingsData)
    {
        if (IsCurrentTruckLeaving)
        {

            if(MainCanvasUI.Instance.currentMoney < _thingsData.price*10)
            {
                Debug.LogError("钱不够");
                EventCenter.Instance.EventTrigger<string, Vector3>("ShowTips", "钱不够", tipShowPos.position);
                Invoke("HideTip", 0.6f);
                return;
            }

            int cost = -_thingsData.cost*10;
            EventCenter.Instance.EventTrigger<int>("MoneyChange", cost);
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

    void HideTip()
    {
        EventCenter.Instance.EventTrigger("HideTips");
    }

}
