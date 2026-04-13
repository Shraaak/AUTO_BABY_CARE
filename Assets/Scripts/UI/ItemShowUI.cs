using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemShowUI : MonoBehaviour
{
    public ThingsData thingsData;

    public void OnClick()
    {
        ShopUI.Instance.BuyItem(thingsData);
    }
}
