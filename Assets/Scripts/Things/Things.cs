using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Things : MonoBehaviour
{
    public ThingsData thingsData;

    public bool isPickUp = false;
    private Rigidbody rb;
    private Collider col;

    private void Awake() {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();
    }

    /// <summary>
    /// 拾取
    /// </summary>
    /// <param name="pickUpPoint"></param>
    public void PickUp(Transform pickUpPoint)
    {
        isPickUp = true;
        transform.position = pickUpPoint.position;
        transform.parent = pickUpPoint;
        rb.isKinematic = true;
        col.enabled = false;
    }

    /// <summary>
    /// 放下
    /// </summary>
    public void Drop()
    {
        isPickUp = false;
        transform.parent = null;
        rb.isKinematic = false;
        col.enabled = true;
    }

    public void DestroySelf()
    {
        Destroy(gameObject);
    }

}
