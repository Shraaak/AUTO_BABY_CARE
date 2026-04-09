using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using Unity.VisualScripting;

public class CustomerWantUI : MonoBehaviour
{
    public Image icon;
    public Customer customer;

    void Start()
    {
        customer.OnStateChanged += UpdateUI;
    }

    void Init(Customer c)
    {
        customer = c;
        UpdateUI();
    }

    public void UpdateUI()
    {
        //无状态隐藏ui
        if(customer.currentState == CustomerStateType.None)
        {
            gameObject.SetActive(false);
            return;
        }

        //有状态显示ui
        gameObject.SetActive(true);

        var config = customer.GetConfig(customer.currentState);

        if (customer.currentState == CustomerStateType.Find)
        {
            //显示商品
            icon.sprite = customer.tagertItem.icon;
        }
        else
        {
            icon.sprite = config.icon;
        }
    }
    
    private void OnDisable() {
        customer.OnStateChanged -= UpdateUI;
    }
}
