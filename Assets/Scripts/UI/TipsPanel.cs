using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using DG.Tweening;
using TMPro;

public class TipsPanel : MonoBehaviour
{
    public TextMeshProUGUI text;
    void Start()
    {
        EventCenter.Instance.AddEventListener<string, Vector3>("ShowTips", showTip);
        EventCenter.Instance.AddEventListener("HideTips", hideTip);
        this.gameObject.SetActive(false);
    }

    void showTip(string contText, Vector3 pos)
    {
        AudioManager.Instance.PlaySFX("Tips");
        DOTween.Kill(gameObject);

        gameObject.SetActive(true);
        text.text = contText;
        transform.position = pos;
        transform.localScale = Vector3.zero;
        transform.rotation = Quaternion.identity;

        Sequence seq = DOTween.Sequence();
        seq.Append(transform.DOScale(Vector3.one, 0.2f).SetEase(Ease.OutBack));
        seq.Append(transform.DORotate(new Vector3(0, 0, 10f), 0.08f));
        seq.Append(transform.DORotate(new Vector3(0, 0, -10f), 0.08f));
        seq.Append(transform.DORotate(Vector3.zero, 0.08f));
        seq.Play();
    }

    void hideTip()
    {
        DOTween.Kill(gameObject);
        transform.DOScale(Vector3.zero, 0.2f).SetEase(Ease.InBack);
    }
}
