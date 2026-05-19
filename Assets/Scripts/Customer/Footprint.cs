using System.Collections;
using UnityEngine;

public class Footprint : MonoBehaviour, IPoolable
{
    // 静态变量：统计当前场景中所有未清理的脚印数量
    public static int CurrentFootprintCount { get; private set; }
    private Collider col;
    private GameObject prefab;

    private void Awake()
    {
        col = GetComponent<Collider>();

        CurrentFootprintCount++;
    }

    public void Clean()
    {
        col.enabled = false;
        CurrentFootprintCount--;
        ObjectPool.Instance.ReturnObject(gameObject, prefab);
    }

    public void OnSpawn()
    {
        gameObject.SetActive(true);
        col.enabled = true;
        CurrentFootprintCount++;

    }

    public void OnDespawn()
    {
        gameObject.SetActive(false);
    }
    
    public void SetPrefab(GameObject prefab)
    {
        this.prefab = prefab;
    }
}