using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShelfUI : MonoBehaviour
{
    public Shelf shelf;
    public Image icon;
    public TextMeshPro countText;

    void Start()
    {
        Init();
        shelf.OnShelfChanged += UpdateUI;
    }

    void Init()
    {
        icon.sprite = shelf.itemType.icon;
        UpdateUI(shelf.currentCount, shelf.maxCapacity);
    }

    void UpdateUI(int current, int max)
    {
        countText.text = $"{current}/{max}";
    }

    private void OnDestroy()
    {
        shelf.OnShelfChanged -= UpdateUI;
    }
}
