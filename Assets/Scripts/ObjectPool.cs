using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPool : MonoBehaviour
{
    public static ObjectPool Instance { get; private set; }
    Dictionary<GameObject, Queue<GameObject>> pool = new Dictionary<GameObject, Queue<GameObject>>();

    [SerializeField]
    int maxCapacity = 1000;

    void Awake()
    {
        //TODO:
        Instance = this;
    }

    public GameObject GetObject(GameObject prefab)
    {
        if(prefab == null)
        {
            Debug.LogError("传入的预制体为null");
            return null;
        }

        if(!pool.TryGetValue(prefab, out var queue))
        {
            print($"创建新对象池: {prefab.name}");
            queue = new Queue<GameObject>();
            pool[prefab] = queue;
        }

        GameObject obj;
        
        if(queue.Count > 0)
        {
            print("取对象池队列物品生成");
            obj = queue.Dequeue();
        }else
        { 
            print("对象池队列没东西 物品生成");
            obj = Instantiate(prefab);
        }

        obj.SetActive(true);

        if(obj.TryGetComponent<IPoolable>(out var p)){
            p.OnSpawn();
            p.SetPrefab(prefab); // 把预制体引用传递给对象
        }

        return obj;
    }

    public void ReturnObject(GameObject obj, GameObject prefab)
    {
        
        if(!pool.TryGetValue(prefab, out var queue))
        {
            queue = new Queue<GameObject>();
            pool[prefab] = queue;
        }

        if(queue.Count > maxCapacity)
        {
            Destroy(obj);
            return;
        }

        if(obj.TryGetComponent<IPoolable>(out var p))
            p.OnDespawn();
        
        obj.SetActive(false);
        queue.Enqueue(obj);
    }
}
