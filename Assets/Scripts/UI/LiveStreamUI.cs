using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LiveStreamUI : MonoBehaviour
{
    [Tooltip("武术表演")]
    public Button button1;
    [Tooltip("直播擦边")]
    public Button button2;
    [Tooltip("直播带娃")]
    public Button button3;

    public GameObject showUI;

    void Start()
    {
        button1.onClick.AddListener(OnButton1Click);
        button2.onClick.AddListener(OnButton2Click);
        button3.onClick.AddListener(OnButton3Click);
    }

    private void OnButton3Click()
    {
        gameObject.SetActive(false);
        showUI.SetActive(true);
    }

    private void OnButton2Click()
    {
        gameObject.SetActive(false);
        showUI.SetActive(true);
    }

    private void OnButton1Click()
    {
        gameObject.SetActive(false);
        showUI.SetActive(true);
    }
}
