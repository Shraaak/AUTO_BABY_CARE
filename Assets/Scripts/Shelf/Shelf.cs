using System.Collections;
using System.Collections.Generic;
using UnityEditor.EditorTools;
using UnityEngine;

public class Shelf : MonoBehaviour
{
    [Header("配置")]
    [Tooltip("这个货架卖什么")]
    public ThingsData itemType;   
    [Tooltip("货架容量")]
    public int maxCapacity = 10;
    [Tooltip("当前商品数量")]
    public int currentCount = 0;

    // UI监听用事件
    public System.Action<int, int> OnShelfChanged;

    /// <summary>
    /// 添加商品方法, 默认每次加一
    /// </summary>
    /// <param name="amount"></param>
    /// <returns></returns>
    public bool AddItem(ThingsData item, int amount = 1)
    {
        if (item != itemType){
            Debug.Log("不是对应sheft");
            return false;
        }

        if(currentCount >= maxCapacity)
            return false;
        
        currentCount += amount;
        currentCount = Mathf.Clamp(currentCount, 0, maxCapacity);
        OnShelfChanged?.Invoke(currentCount, maxCapacity);
        return true;
    }

    /// <summary>
    /// 拿取商品
    /// </summary>
    /// <param name="item"></param>
    /// <returns></returns>
    public bool TakeItem(int amount)
    {

        if((currentCount-=amount) <= 0)
            return false;

        currentCount -= amount;

        OnShelfChanged?.Invoke(currentCount, maxCapacity);
        return true;
    }

    /// <summary>
    /// 判断是否有货
    /// </summary>
    /// <returns></returns>
    public bool HasItem()
    {
        return currentCount > 0;
    }

    /// <summary>
    /// 是否满了
    /// </summary>
    /// <returns></returns>
    public bool IsFull()
    {
        return currentCount >= maxCapacity;
    }
}
