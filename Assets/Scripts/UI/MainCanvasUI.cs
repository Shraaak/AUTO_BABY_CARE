using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class MainCanvasUI : MonoBehaviour
{
    [Header("金钱显示ui")]
    public TextMeshProUGUI text;
    private int currentMoney = 0;

    public GameObject shopPanel;
    public GameObject livePanel;

    private void OnEnable()
    {
        GameEvents.OnCheckoutSuccess += OnCheckout;
    }

    void Update()
    {
        text.text = currentMoney.ToString();
    }

    void OnCheckout(int money)
    {
        currentMoney += money;
    }

    private void OnDisable() {
        GameEvents.OnCheckoutSuccess -= OnCheckout;
    }

    public void CloseShopPanel()
    {
        shopPanel.SetActive(false);
    }

    public void CloseLivePanel()
    {
        livePanel.SetActive(false);
    }
}
