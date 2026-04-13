using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrderManager : MonoBehaviour
{
    public static OrderManager Instance;
    private Queue<Order> orderQueue = new Queue<Order>();
    private void Awake()
    {
        Instance = this;
    }

    public void CreateOrder(ThingsData thingsData, int amount)
    {
        Order order = new Order(thingsData, amount);
        orderQueue.Enqueue(order);

        Debug.Log("下单：" + thingsData.name + " x" + amount);

        
    }

    public bool HasOrder()
    {
        return orderQueue.Count > 0;
    }

    public Order GetNextOrder()
    {
        if (orderQueue.Count == 0) return null;
        return orderQueue.Dequeue();
    }
}
