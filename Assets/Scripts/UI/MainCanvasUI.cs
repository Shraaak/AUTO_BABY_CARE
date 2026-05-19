using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using DG.Tweening;
public class MainCanvasUI : MonoBehaviour
{
    public static MainCanvasUI Instance{get ; private set ;}
    [Header("金钱显示ui")]
    public TextMeshProUGUI text;
    public int currentMoney = 0;

    public GameObject shopPanel;
    public GameObject livePanel;

    void Awake()
    {
        Instance = this;
        EventCenter.Instance.AddEventListener<int>("MoneyChange", OnCheckout);
    }

    void Update()
    {
        text.text = currentMoney.ToString();
    }

    void OnCheckout(int money)
    {
        currentMoney += money;
    }

    public void showShopUI()
    {
        shopPanel.SetActive(true);
        AudioManager.Instance.PlaySFX("Click");

        //重置初始状态
        shopPanel.transform.localScale = Vector3.zero;
        shopPanel.transform.DOScale(Vector3.one, 0.3f).SetEase(Ease.OutBack);
    }

    public void showLivestreamUI()
    {
        livePanel.SetActive(true);
        AudioManager.Instance.PlaySFX("Click");
        livePanel.transform.localScale = Vector3.zero;
        livePanel.transform.DOScale(Vector3.one, 0.2f).SetEase(Ease.OutBack);
    }

    public void CloseShopPanel()
    {
        AudioManager.Instance.PlaySFX("Click");
        DOTween.Kill(shopPanel);
        shopPanel.transform.DOScale(Vector3.zero, 0.2f)
        .SetEase(Ease.InBack)
        .OnComplete(() => shopPanel.SetActive(false));
    }

    public void CloseLivePanel()
    {
        AudioManager.Instance.PlaySFX("Click");
        
        DOTween.Kill(livePanel);
        livePanel.transform.DOScale(Vector3.zero, 0.2f)
        .SetEase(Ease.InBack)
        .OnComplete(() => livePanel.SetActive(false));
        RhythmManager.Instance.Finish();
    }
}
