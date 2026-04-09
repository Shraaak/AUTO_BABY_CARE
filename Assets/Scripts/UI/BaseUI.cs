using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseUI : MonoBehaviour
{
    public GameObject shopUI;
    public GameObject LivestreamUI;
    public void showShopUI()
    {
        shopUI.SetActive(true);
    }

    public void showLivestreamUI()
    {
        LivestreamUI.SetActive(true);
    }
}
