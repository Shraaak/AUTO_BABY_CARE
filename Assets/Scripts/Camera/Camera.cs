using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Camera : MonoBehaviour
{
    public Transform lookAt;
    private Vector3 offset;

    public float cameraMoveSmooth = 3f;

    void Start()
    {
        offset = transform.position - lookAt.position;
    }

    void FixedUpdate()
    {
        Vector3 moveDir = lookAt.position + offset;
        transform.position = Vector3.Lerp(transform.position, moveDir, cameraMoveSmooth*Time.fixedDeltaTime);
    }
}
