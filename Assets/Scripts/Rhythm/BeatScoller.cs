using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class BeatScoller : MonoBehaviour
{
    public float beatTempo;
    public bool hasStarted;
    public Vector3 initialPosition; // 记录滚动条初始位置

    void Start()
    {
        initialPosition = transform.position; // 启动时保存初始位置
        //beatTempo = beatTempo / 60f;
    }

    void Update()
    {
        if (hasStarted)
        {
            transform.position -= new Vector3(0f, beatTempo * Time.deltaTime, 0f);
        }
    }

    public void Begin()
    {
        hasStarted = true;
    }

    // 重置滚动条位置和状态
    public void ResetPosition()
    {
        transform.position = initialPosition; // 回到初始位置
        hasStarted = false;
    }
}