using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "CreateThingsData")]
public class ThingsData : ScriptableObject
{
    public string itemName;
    public Sprite icon;
    [Tooltip("售价")]
    public int price;
    [Tooltip("成本")]
    public int cost;
    public bool isCanEat = true;
    public GameObject perfeb;
}

