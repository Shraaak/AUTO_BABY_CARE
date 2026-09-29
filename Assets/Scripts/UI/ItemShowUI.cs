using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ItemShowUI : MonoBehaviour
{
    public ThingsData thingsData;
    public Image image;
    public TextMeshProUGUI name;
    public TextMeshProUGUI price;

    private void Start() {
        image.sprite = thingsData.icon;
        name.text = "name: " + thingsData.itemName;
        price.text = "price: " + thingsData.cost + "$x10";
    }

    public void OnClick()
    {
        ShopUI.Instance.BuyItem(thingsData);
    }
}
