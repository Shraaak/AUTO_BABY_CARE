using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BaseUI : MonoBehaviour
{
    public GameObject shopUI;
    public GameObject LivestreamUI;
    public Slider hungry;
    public Slider mood;
    public Slider sleepy;

    void Start()
    {
        hungry.value = 1f;
        mood.value = 1f;
        sleepy.value = 1f;
    }

    void Update()
    {
        if (Baby.Instance.stateMechine.currentState != Baby.Instance.sickState)
        {
            hungry.value = Baby.Instance.hunger/Baby.Instance.maxSize;
            mood.value = Baby.Instance.boredom/Baby.Instance.maxSize;
            sleepy.value = Baby.Instance.sleepy/Baby.Instance.maxSize;
        }
    }
}
