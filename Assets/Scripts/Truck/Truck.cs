using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.VisualScripting;

public class Truck : MonoBehaviour
{
    private Transform spawnPoint;
    private Transform despawnPoint;
    private Transform pausePoint;
    public float smooth = 3f;
    private Order currentOrder;
    private int currentAmount;

    public GameObject bubbleUI;
    public Image bubbleImage;
    public TextMeshProUGUI bubbleText;
    public float maxMoveSpeed = 5f;

    public GameObject truckThing;
    
    private enum State
    {
        MovingToPause,
        Waiting,
        Leaving
    }

    private State currentState;

    void Start()
    {
        // 出生点初始化
        transform.position = spawnPoint.position;

        currentState = State.MovingToPause;
    }

    void Update()
    {
        switch (currentState)
        {
            case State.MovingToPause:
                ShopUI.Instance.IsCurrentTruckLeaving = false;
                if (MoveTo(pausePoint.position))
                {
                    currentState = State.Waiting;
                    Debug.Log("到达停靠点，等待玩家互动");
                }
                break;

            case State.Waiting:
                ShopUI.Instance.IsCurrentTruckLeaving = false;
                // 什么都不做等玩家触发
                break;

            case State.Leaving:
                ShopUI.Instance.IsCurrentTruckLeaving = true;

                if (MoveTo(despawnPoint.position))
                {
                    Destroy(gameObject);
                }
                break;
        }
    }

    bool MoveTo(Vector3 target)
    {
        Vector3 nextPos = Vector3.Lerp(
            transform.position,
            target,
            smooth * Time.deltaTime
        );

        float maxStep = maxMoveSpeed * Time.deltaTime; // 限制最大速度
        transform.position = Vector3.MoveTowards(
            transform.position,
            nextPos,
            maxStep
        );


        if (Vector3.Distance(transform.position, target) <= 0.01f)
        {
            return true;
        }
        return false;
    }


    //玩家交互
    public void OnPlayerInteract()
    {
        if (currentState != State.Waiting) return;

        //生成实例
        GameObject item = Instantiate(truckThing);
        item.GetComponent<Things>().thingsData = currentOrder.thingsData;
        item.GetComponent<Things>().iconImage.sprite = currentOrder.thingsData.icon;
        item.GetComponent<Things>().amountText.text = $"x {currentAmount}";
        item.GetComponent<Things>().amount = 10;

        item.GetComponent<Things>().PickUp(Player.Instance.pickUpPoint);
        Player.Instance.currentThings = item.GetComponent<Things>();
        Player.Instance.stateMechine.ChangeState(Player.Instance.pickUpState);

        currentState = State.Leaving;
    }

    public void Init(Transform _spawnPoint, Transform _despawnPoint, Transform _pausePoint)
    {
        spawnPoint = _spawnPoint;
        despawnPoint = _despawnPoint;
        pausePoint = _pausePoint;
        LoadNextOrder();
    }

    void LoadNextOrder()
    {
        if (!OrderManager.Instance.HasOrder())
        {
            Leave();
            return;
        }

        //获取订单数据
        currentOrder = OrderManager.Instance.GetNextOrder();
        currentAmount = currentOrder.amount;

        UpdateUI();
    }

    void UpdateUI()
    {
        if (currentAmount > 0)
        {
            bubbleUI.SetActive(true);
            bubbleImage.sprite = currentOrder.thingsData.icon;
            bubbleText.text = "x" + currentAmount;
        }
        else
        {
            bubbleUI.SetActive(false);
        }
    }

    void Leave()
    {
        Debug.Log("货车离开");
        currentState = State.Leaving;
    }
}
