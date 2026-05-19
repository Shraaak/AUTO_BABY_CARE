using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cashier : MonoBehaviour
{
    public Transform tipShowPos;
    //排队点集合
    public Transform[] queuePoints;
    //每个位置的customer
    private List<Customer> queue = new List<Customer>();

    public Customer CurrentCustomer => queue.Count > 0 ? queue[0] : null;

    void Update()
    {
    }

    //入队
    public int Enqueue(Customer customer)
    {
        queue.Add(customer);
        Debug.Log("入队");
        EventCenter.Instance.EventTrigger<string, Vector3>("ShowTips","有顾客在排队！", tipShowPos.position);
        Invoke("HideTip", 0.6f);
        return queue.Count - 1;
    }

    //出队让顾客离开其他往前走
    public void Dequeue()
    {
        if (queue.Count == 0){
            Debug.Log("没有顾客");
            EventCenter.Instance.EventTrigger("HideTips");
            return;
        }

        Customer first = queue[0];
        queue.RemoveAt(0);

        // 通知这个顾客离开
        first.stateMechine.ChangeState(first.leaveState);

        // 更新队列索引
        for (int i = 0; i < queue.Count; i++)
        {
            queue[i].SetQueueIndex(i);
        }

        // 再让顾客前往对应位置
        for (int i = 0; i < queue.Count; i++)
        {
            queue[i].MoveToQueuePoint();
        }
    }

    //获取排位点
    public Transform GetQueuePoint(int index)
    {
        //s输入保险用
        if (index >= queuePoints.Length)
            index = queuePoints.Length - 1;

        return queuePoints[index];
    }

    void HideTip()
    {
        EventCenter.Instance.EventTrigger("HideTips");
    }
    
}
